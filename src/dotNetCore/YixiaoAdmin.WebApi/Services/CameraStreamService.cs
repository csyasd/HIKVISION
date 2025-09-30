using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;

namespace YixiaoAdmin.WebApi.Services
{
    public class CameraStreamService : BackgroundService
    {
        private readonly ILogger<CameraStreamService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _ffmpegExecutablePath;
        
        // 摄像头连接管理
        private readonly ConcurrentDictionary<string, CameraStreamInfo> _activeStreams = new();
        private readonly object _sdkLock = new object();
        private static bool _sdkInitialized = false;

        // HCNetSDK 相关
        [DllImport("HCNetSDK.dll")]
        public static extern bool NET_DVR_Init();

        [DllImport("HCNetSDK.dll")]
        public static extern int NET_DVR_Login_V30(
            string sDVRIP,
            int wDVRPort,
            string sUserName,
            string sPassword,
            ref NET_DVR_DEVICEINFO_V30 lpDeviceInfo);

        [DllImport("HCNetSDK.dll")]
        public static extern bool NET_DVR_Logout(int lUserID);

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_DEVICEINFO_V30
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] sSerialNumber;
            public byte byAlarmInPortNum;
            public byte byAlarmOutPortNum;
            public byte byDiskNum;
            public byte byDVRType;
            public byte byChanNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] byRes;
        }

        public class CameraStreamInfo
        {
            public string CameraId { get; set; }
            public string Name { get; set; }
            public string IP { get; set; }
            public string DeviceCode { get; set; }
            public int LoginUserId { get; set; } = -1;
            public Process FfmpegProcess { get; set; }
            public string HlsPath { get; set; }
            public string LogPath { get; set; }
            public DateTime StartTime { get; set; }
            public bool IsActive { get; set; }
            public string LastError { get; set; }
        }

        public CameraStreamService(
            ILogger<CameraStreamService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            _serviceProvider = serviceProvider;

            _ffmpegExecutablePath = _configuration.GetValue<string>("Hikvision:FfmpegPath");
            if (string.IsNullOrWhiteSpace(_ffmpegExecutablePath))
            {
                _ffmpegExecutablePath = Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "ffmpeg", "ffmpeg.exe");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("摄像头流服务启动");

            // 等待应用程序完全启动
            await Task.Delay(5000, stoppingToken);

            try
            {
                // 初始化海康SDK
                InitializeHikSDK();

                // 加载并启动所有摄像头流
                await LoadAndStartAllCameraStreams(stoppingToken);

                // 监控流状态
                while (!stoppingToken.IsCancellationRequested)
                {
                    await MonitorStreams(stoppingToken);
                    await Task.Delay(30000, stoppingToken); // 每30秒检查一次
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "摄像头流服务执行出错");
            }
        }

        private void InitializeHikSDK()
        {
            lock (_sdkLock)
            {
                if (!_sdkInitialized)
                {
                    string sdkPath = Path.Combine(AppContext.BaseDirectory, "HikSDK");
                    Environment.SetEnvironmentVariable("PATH", sdkPath + ";" + Environment.GetEnvironmentVariable("PATH"));

                    if (!NET_DVR_Init())
                    {
                        throw new Exception("HCNetSDK 初始化失败");
                    }
                    _sdkInitialized = true;
                    _logger.LogInformation("海康SDK初始化成功");
                }
            }
        }

        private async Task LoadAndStartAllCameraStreams(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cameraService = scope.ServiceProvider.GetRequiredService<ICameraServices>();
                
                var cameras = await cameraService.Query();
                _logger.LogInformation($"加载到 {cameras.Count} 个摄像头");

                foreach (var camera in cameras)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await StartCameraStream(camera);
                        await Task.Delay(2000, cancellationToken); // 每个摄像头启动间隔2秒
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"启动摄像头 {camera.Name}({camera.IP}) 流失败");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载摄像头列表失败");
            }
        }

        private async Task StartCameraStream(Camera camera)
        {
            if (string.IsNullOrWhiteSpace(camera.IP))
            {
                _logger.LogWarning($"摄像头 {camera.Name} IP地址为空，跳过");
                return;
            }

            var streamInfo = new CameraStreamInfo
            {
                CameraId = camera.Id,
                Name = camera.Name,
                IP = camera.IP,
                DeviceCode = camera.DeviceCode,
                StartTime = DateTime.Now
            };

            try
            {
                // 1. 登录摄像头
                var loginResult = LoginToCamera(camera);
                if (!loginResult.success)
                {
                    streamInfo.LastError = loginResult.error;
                    _logger.LogError($"摄像头 {camera.Name}({camera.IP}) 登录失败: {loginResult.error}");
                    return;
                }

                streamInfo.LoginUserId = loginResult.userId;
                _logger.LogInformation($"摄像头 {camera.Name}({camera.IP}) 登录成功");

                // 2. 启动FFmpeg流转换
                var ffmpegResult = await StartFFmpegStream(camera, streamInfo);
                if (!ffmpegResult.success)
                {
                    streamInfo.LastError = ffmpegResult.error;
                    _logger.LogError($"摄像头 {camera.Name}({camera.IP}) FFmpeg启动失败: {ffmpegResult.error}");
                    return;
                }

                streamInfo.IsActive = true;
                _activeStreams.TryAdd(camera.Id, streamInfo);
                
                _logger.LogInformation($"摄像头 {camera.Name}({camera.IP}) 流启动成功，HLS路径: {streamInfo.HlsPath}");
            }
            catch (Exception ex)
            {
                streamInfo.LastError = ex.Message;
                _logger.LogError(ex, $"启动摄像头 {camera.Name}({camera.IP}) 流时出错");
            }
        }

        private (bool success, int userId, string error) LoginToCamera(Camera camera)
        {
            try
            {
                // 从配置或摄像头信息中获取登录凭据
                string username = _configuration.GetValue<string>("Hikvision:Device:UserName") ?? "admin";
                string password = _configuration.GetValue<string>("Hikvision:Device:Password") ?? "wzxc2025";
                int port = _configuration.GetValue<int>("Hikvision:Device:Port", 8000);

                var deviceInfo = new NET_DVR_DEVICEINFO_V30();
                int userId = NET_DVR_Login_V30(camera.IP, port, username, password, ref deviceInfo);
                
                if (userId < 0)
                {
                    return (false, -1, "登录失败，请检查IP/端口/用户名/密码");
                }

                return (true, userId, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, -1, ex.Message);
            }
        }

        private async Task<(bool success, string error)> StartFFmpegStream(Camera camera, CameraStreamInfo streamInfo)
        {
            try
            {
                if (!File.Exists(_ffmpegExecutablePath))
                {
                    return (false, $"FFmpeg可执行文件不存在: {_ffmpegExecutablePath}");
                }

                // 创建HLS输出目录
                string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");
                string hlsFolder = Path.Combine(webRoot, "hls");
                Directory.CreateDirectory(hlsFolder);

                // 生成文件名（使用摄像头ID避免冲突）
                string hlsFileName = $"camera_{camera.Id}.m3u8";
                string hlsFile = Path.Combine(hlsFolder, hlsFileName);
                string hlsFileBase = $"camera_{camera.Id}";
                string logFile = Path.Combine(hlsFolder, $"{hlsFileBase}.log");

                streamInfo.HlsPath = $"/hls/{hlsFileName}";
                streamInfo.LogPath = $"/hls/{hlsFileBase}.log";

                // 构建RTSP URL
                string username = _configuration.GetValue<string>("Hikvision:Device:UserName") ?? "admin";
                string password = _configuration.GetValue<string>("Hikvision:Device:Password") ?? "wzxc2025";
                string rtspUrl = $"rtsp://{username}:{password}@{camera.IP}:554/Streaming/Channels/101";

                // FFmpeg参数
                var arguments = $"-rtsp_transport tcp -i \"{rtspUrl}\" -c:v libx264 -preset ultrafast -tune zerolatency -profile:v baseline -level 3.0 -pix_fmt yuv420p -hls_time 2 -hls_list_size 5 -hls_flags delete_segments+append_list -hls_segment_filename \"{Path.Combine(hlsFolder, hlsFileBase)}%03d.ts\" -f hls \"{hlsFile}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegExecutablePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = hlsFolder
                };

                var process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                // 设置日志记录
                var writerStream = new StreamWriter(logFile, false, Encoding.UTF8) { AutoFlush = true };
                var logWriter = TextWriter.Synchronized(writerStream);

                void SafeWrite(string level, string message)
                {
                    if (string.IsNullOrWhiteSpace(message)) return;
                    try
                    {
                        logWriter.WriteLine($"[{DateTime.Now:O}] [{level}] {message}");
                    }
                    catch (ObjectDisposedException) { }
                }

                process.OutputDataReceived += (_, e) => SafeWrite("OUT", e?.Data);
                process.ErrorDataReceived += (_, e) => SafeWrite("ERR", e?.Data);
                process.Exited += (_, __) =>
                {
                    SafeWrite("EXIT", $"ExitCode: {process.ExitCode}");
                    logWriter.Dispose();
                    
                    // 标记流为非活动状态
                    if (_activeStreams.TryGetValue(camera.Id, out var info))
                    {
                        info.IsActive = false;
                        info.LastError = $"FFmpeg进程退出，退出码: {process.ExitCode}";
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                streamInfo.FfmpegProcess = process;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task MonitorStreams(CancellationToken cancellationToken)
        {
            var inactiveStreams = new List<string>();

            foreach (var kvp in _activeStreams)
            {
                var streamInfo = kvp.Value;
                
                // 检查FFmpeg进程状态
                if (streamInfo.FfmpegProcess?.HasExited == true)
                {
                    streamInfo.IsActive = false;
                    inactiveStreams.Add(kvp.Key);
                    _logger.LogWarning($"摄像头 {streamInfo.Name}({streamInfo.IP}) 的流进程已退出");
                }
            }

            // 尝试重启失效的流
            foreach (var cameraId in inactiveStreams)
            {
                if (_activeStreams.TryRemove(cameraId, out var streamInfo))
                {
                    _logger.LogInformation($"尝试重启摄像头 {streamInfo.Name}({streamInfo.IP}) 的流");
                    
                    try
                    {
                        // 清理资源
                        CleanupStreamInfo(streamInfo);
                        
                        // 重新获取摄像头信息并启动
                        using var scope = _serviceProvider.CreateScope();
                        var cameraService = scope.ServiceProvider.GetRequiredService<ICameraServices>();
                        var camera = await cameraService.QueryById(cameraId);
                        
                        if (camera != null)
                        {
                            await StartCameraStream(camera);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"重启摄像头 {streamInfo.Name}({streamInfo.IP}) 流失败");
                    }
                }
            }
        }

        private void CleanupStreamInfo(CameraStreamInfo streamInfo)
        {
            try
            {
                // 停止FFmpeg进程
                if (streamInfo.FfmpegProcess != null && !streamInfo.FfmpegProcess.HasExited)
                {
                    streamInfo.FfmpegProcess.Kill();
                    streamInfo.FfmpegProcess.WaitForExit(2000);
                }
                streamInfo.FfmpegProcess?.Dispose();

                // 登出摄像头
                if (streamInfo.LoginUserId >= 0)
                {
                    NET_DVR_Logout(streamInfo.LoginUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"清理摄像头 {streamInfo.Name} 资源时出错");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止摄像头流服务...");

            // 清理所有活动流
            foreach (var streamInfo in _activeStreams.Values)
            {
                CleanupStreamInfo(streamInfo);
            }

            _activeStreams.Clear();

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("摄像头流服务已停止");
        }

        // 公共方法：获取所有流状态
        public Dictionary<string, object> GetAllStreamStatus()
        {
            return _activeStreams.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new
                {
                    kvp.Value.Name,
                    kvp.Value.IP,
                    kvp.Value.DeviceCode,
                    kvp.Value.IsActive,
                    kvp.Value.StartTime,
                    kvp.Value.HlsPath,
                    kvp.Value.LastError,
                    ProcessId = kvp.Value.FfmpegProcess?.Id,
                    ProcessRunning = kvp.Value.FfmpegProcess?.HasExited == false
                });
        }

        // 公共方法：手动重启指定摄像头流
        public async Task<bool> RestartCameraStream(string cameraId)
        {
            try
            {
                if (_activeStreams.TryRemove(cameraId, out var streamInfo))
                {
                    CleanupStreamInfo(streamInfo);
                }

                using var scope = _serviceProvider.CreateScope();
                var cameraService = scope.ServiceProvider.GetRequiredService<ICameraServices>();
                var camera = await cameraService.QueryById(cameraId);

                if (camera != null)
                {
                    await StartCameraStream(camera);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"重启摄像头流 {cameraId} 失败");
                return false;
            }
        }
    }
}
