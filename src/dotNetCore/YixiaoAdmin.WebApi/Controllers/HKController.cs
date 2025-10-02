using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using YixiaoAdmin.IServices;

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HKController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ICameraServices _cameraServices;



        private class HikvisionDeviceOptions
        {
            public string Ip { get; set; } = "192.168.1.64";
            public int Port { get; set; } = 8000;
            public string UserName { get; set; } = "admin";
            public string Password { get; set; } = "wzxc2025";
        }

        private class StartStreamResult
        {
            public bool Success { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
            public string PlaylistPath { get; set; } = string.Empty;
            public string PlayUrl { get; set; } = string.Empty;
            public string StatusUrl { get; set; } = string.Empty;
            public string LogFile { get; set; } = string.Empty;
        }

    // ====== HCNetSDK 登录函数 ======
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
        public static extern bool NET_DVR_PTZControlWithSpeed_Other(
            int lUserID,
            int lChannel,
            uint dwPTZCommand,
            uint dwStop,
            uint dwSpeed);

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

        private static bool _sdkInitialized = false;
        private static readonly object _ffmpegLock = new object();
        private static Process _ffmpegProcess;
        private readonly string _ffmpegExecutablePath;
        private readonly HikvisionDeviceOptions _deviceOptions;
        private static readonly object _loginLock = new object();
        private static int _loggedInUserId = -1;
        private static NET_DVR_DEVICEINFO_V30 _deviceInfoCache;

        public HKController(IConfiguration configuration, IWebHostEnvironment environment, ICameraServices cameraServices)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _cameraServices = cameraServices ?? throw new ArgumentNullException(nameof(cameraServices));

            _ffmpegExecutablePath = _configuration.GetValue<string>("Hikvision:FfmpegPath");

            if (string.IsNullOrWhiteSpace(_ffmpegExecutablePath))
            {
                _ffmpegExecutablePath = Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "ffmpeg", "ffmpeg.exe");
            }

            _deviceOptions = _configuration.GetSection("Hikvision:Device").Get<HikvisionDeviceOptions>() ?? new HikvisionDeviceOptions();
            if (string.IsNullOrWhiteSpace(_deviceOptions.Ip))
            {
                _deviceOptions.Ip = "192.168.1.64";
            }
            if (string.IsNullOrWhiteSpace(_deviceOptions.UserName))
            {
                _deviceOptions.UserName = "admin";
            }
            if (string.IsNullOrWhiteSpace(_deviceOptions.Password))
            {
                _deviceOptions.Password = "wzxc2025";
            }
        }

        private void EnsureSdkInit()
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
            }
        }

        private StartStreamResult StartStreamInternal(HttpRequest request, string rtspUrl, string outputFileName, string ffmpegPath)
        {
            try
            {
                var loginResult = EnsureLogin();
                if (!loginResult.Success)
                {
                    return new StartStreamResult
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = loginResult.Message
                    };
                }

                if (string.IsNullOrWhiteSpace(rtspUrl))
                {
                    rtspUrl = $"rtsp://{_deviceOptions.UserName}:{_deviceOptions.Password}@{_deviceOptions.Ip}:554/Streaming/Channels/101";
                }

                string targetFfmpegPath = string.IsNullOrWhiteSpace(ffmpegPath) ? _ffmpegExecutablePath : ffmpegPath;

                if (string.IsNullOrWhiteSpace(targetFfmpegPath) || !System.IO.File.Exists(targetFfmpegPath))
                {
                    return new StartStreamResult
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = $"错误: 未找到 FFmpeg 可执行文件，请在 appsettings.json 配置 \"Hikvision:FfmpegPath\" 或通过参数传入。当前路径: {targetFfmpegPath ?? "(未配置)"}"
                    };
                }

                string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");

                string hlsFolder = Path.Combine(webRoot, "hls");
                Directory.CreateDirectory(hlsFolder);

                string normalizedOutputName = string.IsNullOrWhiteSpace(outputFileName) ? "stream.m3u8" : outputFileName.Trim();
                if (!normalizedOutputName.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedOutputName += ".m3u8";
                }

                string hlsFile = Path.Combine(hlsFolder, normalizedOutputName);
                string hlsFileBase = Path.GetFileNameWithoutExtension(normalizedOutputName);
                string logFile = Path.Combine(hlsFolder, $"{hlsFileBase}.log");

                // 注释掉清空文件夹的功能，避免无限刷新循环
                // foreach (var file in Directory.GetFiles(hlsFolder, $"{hlsFileBase}*").Where(f => !f.Equals(logFile, StringComparison.OrdinalIgnoreCase)))
                // {
                //     System.IO.File.Delete(file);
                // }

                lock (_ffmpegLock)
                {
                    if (_ffmpegProcess != null)
                    {
                        try
                        {
                            if (!_ffmpegProcess.HasExited)
                            {
                                _ffmpegProcess.Kill();
                                _ffmpegProcess.WaitForExit(2000);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            _ffmpegProcess?.Dispose();
                            _ffmpegProcess = null;
                        }
                    }

                    // 优化HLS参数以降低延迟（从6-10秒降低到3-5秒）
                    // hls_time: 1秒片段（降低延迟）
                    // hls_list_size: 3个片段（最小化缓冲）
                    // g: 关键帧间隔30帧（配合1秒片段）
                    // fflags nobuffer: 禁用FFmpeg缓冲
                    // flags low_delay: 低延迟模式
                    var arguments = $"-rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{rtspUrl}\" -c:v libx264 -preset ultrafast -tune zerolatency -profile:v baseline -level 3.0 -pix_fmt yuv420p -g 30 -sc_threshold 0 -hls_time 1 -hls_list_size 3 -hls_flags delete_segments+append_list+omit_endlist -hls_segment_filename \"{Path.Combine(hlsFolder, hlsFileBase)}%03d.ts\" -f hls \"{hlsFile}\"";

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = targetFfmpegPath,
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

                    var writerStream = new StreamWriter(logFile, false, Encoding.UTF8) { AutoFlush = true };
                    var logWriter = TextWriter.Synchronized(writerStream);

                    void SafeWrite(string level, string message)
                    {
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            return;
                        }

                        try
                        {
                            logWriter.WriteLine($"[{DateTime.Now:O}] [{level}] {message}");
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }

                    process.OutputDataReceived += (_, e) => SafeWrite("OUT", e?.Data);
                    process.ErrorDataReceived += (_, e) => SafeWrite("ERR", e?.Data);
                    process.Exited += (_, __) =>
                    {
                        SafeWrite("EXIT", $"ExitCode: {process.ExitCode}");
                        logWriter.Dispose();
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    _ffmpegProcess = process;
                }

                var scheme = request?.Scheme ?? "http";
                var host = request?.Host.Value ?? string.Empty;
                var baseUrl = string.IsNullOrWhiteSpace(host) ? string.Empty : $"{scheme}://{host}";
                var playlistPath = $"/hls/{normalizedOutputName}";
                var statusPath = $"/api/HK/hls-status?outputFileName={normalizedOutputName}";
                var logPath = $"/hls/{hlsFileBase}.log";

                return new StartStreamResult
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "HLS 转码已启动",
                    PlaylistPath = playlistPath,
                    PlayUrl = string.IsNullOrWhiteSpace(baseUrl) ? playlistPath : $"{baseUrl}{playlistPath}",
                    StatusUrl = string.IsNullOrWhiteSpace(baseUrl) ? statusPath : $"{baseUrl}{statusPath}",
                    LogFile = string.IsNullOrWhiteSpace(baseUrl) ? logPath : $"{baseUrl}{logPath}"
                };
            }
            catch (Exception ex)
            {
                return new StartStreamResult
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"错误: {ex.Message}"
                };
            }
        }

        // ====== 登录接口 ======
        [HttpGet("login")]
        public IActionResult Login(string ip = "192.168.1.64", int port = 8000, string user = "admin", string password = "wzxc2025")
        {
            try
            {
                var loginResult = EnsureLogin(true, ip, port, user, password);
                if (!loginResult.Success)
                {
                    return BadRequest(loginResult.Message);
                }

                return Ok(new
                {
                    Message = "登录成功",
                    ChannelCount = loginResult.DeviceInfo.byChanNum
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"错误: {ex.Message}");
            }
        }

        private (bool Success, string Message, NET_DVR_DEVICEINFO_V30 DeviceInfo) EnsureLogin(bool allowOverride = false, string? ip = null, int? port = null, string? user = null, string? password = null)
        {
            EnsureSdkInit();

            if (!allowOverride && _loggedInUserId >= 0)
            {
                return (true, "Already Logged In", _deviceInfoCache);
            }

            lock (_loginLock)
            {
                if (!allowOverride && _loggedInUserId >= 0)
                {
                    return (true, "Already Logged In", _deviceInfoCache);
                }

                string loginIp = allowOverride && !string.IsNullOrWhiteSpace(ip) ? ip : _deviceOptions.Ip;
                int loginPort = allowOverride && port.HasValue ? port.Value : _deviceOptions.Port;
                string loginUser = allowOverride && !string.IsNullOrWhiteSpace(user) ? user : _deviceOptions.UserName;
                string loginPassword = allowOverride && !string.IsNullOrWhiteSpace(password) ? password : _deviceOptions.Password;

                var deviceInfo = new NET_DVR_DEVICEINFO_V30();
                int userId = NET_DVR_Login_V30(loginIp, loginPort, loginUser, loginPassword, ref deviceInfo);
                if (userId < 0)
                {
                    return (false, "登录失败，请检查 IP/端口/用户名/密码", deviceInfo);
                }

                _loggedInUserId = userId;
                _deviceInfoCache = deviceInfo;
                return (true, "登录成功", deviceInfo);
            }
        }

        // ====== 开启 RTSP → HLS 转码接口 ======
        [HttpGet("start-stream")]
        public IActionResult StartStream(
            string rtspUrl = null,
            string outputFileName = "stream.m3u8",
            string ffmpegPath = null)
        {
            try
            {
                var loginResult = EnsureLogin();
                if (!loginResult.Success)
                {
                    return StatusCode(500, loginResult.Message);
                }

                if (string.IsNullOrWhiteSpace(rtspUrl))
                {
                    rtspUrl = $"rtsp://{_deviceOptions.UserName}:{_deviceOptions.Password}@{_deviceOptions.Ip}:554/Streaming/Channels/101";
                }

                string targetFfmpegPath = string.IsNullOrWhiteSpace(ffmpegPath) ? _ffmpegExecutablePath : ffmpegPath;

                if (string.IsNullOrWhiteSpace(targetFfmpegPath) || !System.IO.File.Exists(targetFfmpegPath))
                {
                    return StatusCode(500, $"错误: 未找到 FFmpeg 可执行文件，请在 appsettings.json 配置 \"Hikvision:FfmpegPath\" 或通过参数传入。当前路径: {targetFfmpegPath ?? "(未配置)"}");
                }

                string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");

                // HLS 文件存放路径
                string hlsFolder = Path.Combine(webRoot, "hls");
                Directory.CreateDirectory(hlsFolder);

                string normalizedOutputName = string.IsNullOrWhiteSpace(outputFileName) ? "stream.m3u8" : outputFileName.Trim();
                if (!normalizedOutputName.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedOutputName += ".m3u8";
                }

                string hlsFile = Path.Combine(hlsFolder, normalizedOutputName);
                string hlsFileBase = Path.GetFileNameWithoutExtension(normalizedOutputName);
                string logFile = Path.Combine(hlsFolder, $"{hlsFileBase}.log");

                // 注释掉清空文件夹的功能，避免无限刷新循环
                // foreach (var file in Directory.GetFiles(hlsFolder, $"{hlsFileBase}*").Where(f => !f.Equals(logFile, StringComparison.OrdinalIgnoreCase)))
                // {
                //     System.IO.File.Delete(file);
                // }

                lock (_ffmpegLock)
                {
                    if (_ffmpegProcess != null)
                    {
                        try
                        {
                            if (!_ffmpegProcess.HasExited)
                            {
                                _ffmpegProcess.Kill();
                                _ffmpegProcess.WaitForExit(2000);
                            }
                        }
                        catch (Exception)
                        {
                            // 忽略关闭旧进程时的异常
                        }
                        finally
                        {
                            _ffmpegProcess?.Dispose();
                            _ffmpegProcess = null;
                        }
                    }

                    // 优化HLS参数以降低延迟（从6-10秒降低到3-5秒）
                    // hls_time: 1秒片段（降低延迟）
                    // hls_list_size: 3个片段（最小化缓冲）
                    // g: 关键帧间隔30帧（配合1秒片段）
                    // fflags nobuffer: 禁用FFmpeg缓冲
                    // flags low_delay: 低延迟模式
                    var arguments = $"-rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{rtspUrl}\" -c:v libx264 -preset ultrafast -tune zerolatency -profile:v baseline -level 3.0 -pix_fmt yuv420p -g 30 -sc_threshold 0 -hls_time 1 -hls_list_size 3 -hls_flags delete_segments+append_list+omit_endlist -hls_segment_filename \"{Path.Combine(hlsFolder, hlsFileBase)}%03d.ts\" -f hls \"{hlsFile}\"";

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = targetFfmpegPath,
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

                    var writerStream = new StreamWriter(logFile, false, Encoding.UTF8) { AutoFlush = true };
                    var logWriter = TextWriter.Synchronized(writerStream);

                    void SafeWrite(string level, string message)
                    {
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            return;
                        }

                        try
                        {
                            logWriter.WriteLine($"[{DateTime.Now:O}] [{level}] {message}");
                        }
                        catch (ObjectDisposedException)
                        {
                            // 日志已关闭，忽略
                        }
                    }

                    process.OutputDataReceived += (_, e) => SafeWrite("OUT", e?.Data);
                    process.ErrorDataReceived += (_, e) => SafeWrite("ERR", e?.Data);
                    process.Exited += (_, __) =>
                    {
                        SafeWrite("EXIT", $"ExitCode: {process.ExitCode}");
                        logWriter.Dispose();
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    _ffmpegProcess = process;
                }

                var scheme = Request?.Scheme ?? "http";
                var host = Request?.Host.Value ?? "";
                var baseUrl = string.IsNullOrWhiteSpace(host) ? string.Empty : $"{scheme}://{host}";

                return Ok(new
                {
                    Message = "HLS 转码已启动",
                    PlaylistPath = $"/hls/{normalizedOutputName}",
                    PlayUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/hls/{normalizedOutputName}" : $"{baseUrl}/hls/{normalizedOutputName}",
                    LogFile = string.IsNullOrWhiteSpace(baseUrl) ? $"/hls/{hlsFileBase}.log" : $"{baseUrl}/hls/{hlsFileBase}.log",
                    StatusUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/api/HK/hls-status?outputFileName={normalizedOutputName}" : $"{baseUrl}/api/HK/hls-status?outputFileName={normalizedOutputName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"错误: {ex.Message}");
            }
        }

        [HttpPost("clear-hls")]
        public IActionResult ClearHlsFiles(string outputFileName = "stream.m3u8")
        {
            try
            {
                string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");
                string hlsFolder = Path.Combine(webRoot, "hls");

                if (!Directory.Exists(hlsFolder))
                {
                    return Ok(new { Message = "HLS目录不存在", Cleared = false });
                }

                string normalizedOutputName = string.IsNullOrWhiteSpace(outputFileName) ? "stream.m3u8" : outputFileName.Trim();
                if (!normalizedOutputName.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedOutputName += ".m3u8";
                }

                string hlsFileBase = Path.GetFileNameWithoutExtension(normalizedOutputName);
                string logFile = Path.Combine(hlsFolder, $"{hlsFileBase}.log");

                var filesToDelete = Directory.GetFiles(hlsFolder, $"{hlsFileBase}*")
                    .Where(f => !f.Equals(logFile, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var file in filesToDelete)
                {
                    System.IO.File.Delete(file);
                }

                return Ok(new
                {
                    Message = $"已清除 {filesToDelete.Count} 个HLS文件",
                    Cleared = true,
                    DeletedFiles = filesToDelete.Select(Path.GetFileName).ToArray()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"错误: {ex.Message}");
            }
        }

        [HttpGet("hls-status")]
        public IActionResult GetHlsStatus(string outputFileName = "stream.m3u8")
        {
            try
            {
                string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");
                string hlsFolder = Path.Combine(webRoot, "hls");

                if (!Directory.Exists(hlsFolder))
                {
                    return Ok(new
                    {
                        Exists = false,
                        Message = "HLS 目录不存在",
                        Folder = hlsFolder
                    });
                }

                string normalizedOutputName = string.IsNullOrWhiteSpace(outputFileName) ? "stream.m3u8" : outputFileName.Trim();
                if (!normalizedOutputName.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedOutputName += ".m3u8";
                }

                string hlsFile = Path.Combine(hlsFolder, normalizedOutputName);
                bool exists = System.IO.File.Exists(hlsFile);

                return Ok(new
                {
                    Exists = exists,
                    HlsFile = hlsFile,
                    LastWriteUtc = exists ? System.IO.File.GetLastWriteTimeUtc(hlsFile) : (DateTime?)null,
                    SegmentFiles = exists ? Directory.GetFiles(hlsFolder, $"{Path.GetFileNameWithoutExtension(normalizedOutputName)}*.ts").Select(Path.GetFileName).ToArray() : Array.Empty<string>()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取摄像头HLS播放地址
        /// </summary>
        /// <param name="cameraId">摄像头ID</param>
        /// <returns></returns>
        [HttpGet("camera-hls/{cameraId}")]
        public IActionResult GetCameraHlsUrl(string cameraId)
        {
            try
            {
                // 构建HLS文件路径
                var hlsDirectory = Path.Combine(_environment.WebRootPath, "hls");
                var hlsFile = Path.Combine(hlsDirectory, $"{cameraId}.m3u8");
                
                // 检查HLS文件是否存在
                if (!System.IO.File.Exists(hlsFile))
                {
                    return NotFound(new { 
                        message = $"摄像头 {cameraId} 的HLS流文件不存在",
                        cameraId = cameraId,
                        hlsPath = $"/hls/{cameraId}.m3u8"
                    });
                }
                
                // 构建播放URL
                var request = HttpContext.Request;
                var scheme = request.Scheme;
                var host = request?.Host.Value ?? string.Empty;
                var baseUrl = string.IsNullOrWhiteSpace(host) ? string.Empty : $"{scheme}://{host}";
                
                var hlsPath = $"/hls/{cameraId}.m3u8";
                var playUrl = string.IsNullOrWhiteSpace(baseUrl) ? hlsPath : $"{baseUrl}{hlsPath}";
                
                return Ok(new { 
                    cameraId = cameraId,
                    hlsUrl = playUrl,
                    localPath = hlsPath,
                    exists = true,
                    message = "HLS流可用"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = $"获取摄像头HLS地址失败: {ex.Message}",
                    cameraId = cameraId 
                });
            }
        }

        /// <summary>
        /// 云台控制 - PTZ控制命令常量
        /// </summary>
        public enum PTZCommand
        {
            LIGHT_PWRON = 2,        // 接通灯光电源
            WIPER_PWRON = 3,        // 接通雨刷开关
            FAN_PWRON = 4,          // 接通风扇开关
            HEATER_PWRON = 5,       // 接通加热器开关
            AUX_PWRON1 = 6,         // 接通辅助设备开关
            AUX_PWRON2 = 7,         // 接通辅助设备开关
            ZOOM_IN = 11,           // 焦距变大(倍率变大)
            ZOOM_OUT = 12,          // 焦距变小(倍率变小)
            FOCUS_NEAR = 13,        // 焦点前调
            FOCUS_FAR = 14,         // 焦点后调
            IRIS_OPEN = 15,         // 光圈扩大
            IRIS_CLOSE = 16,        // 光圈缩小
            TILT_UP = 21,           // 云台上仰
            TILT_DOWN = 22,         // 云台下俯
            PAN_LEFT = 23,          // 云台左转
            PAN_RIGHT = 24,         // 云台右转
            UP_LEFT = 25,           // 云台上仰和左转
            UP_RIGHT = 26,          // 云台上仰和右转
            DOWN_LEFT = 27,         // 云台下俯和左转
            DOWN_RIGHT = 28,        // 云台下俯和右转
            PAN_AUTO = 29           // 云台左右自动扫描
        }

        /// <summary>
        /// 云台控制接口
        /// </summary>
        /// <param name="ip">摄像头IP地址</param>
        /// <param name="port">摄像头端口，默认8000</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="channel">通道号，默认1</param>
        /// <param name="command">PTZ命令</param>
        /// <param name="stop">是否停止：0-开始，1-停止</param>
        /// <param name="speed">速度：1-7</param>
        /// <returns></returns>
        [HttpPost("ptz-control")]
        public IActionResult PTZControl(
            [FromBody] PTZControlRequest request)
        {
            try
            {
                // 参数验证
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "请求参数不能为空" });
                }

                if (string.IsNullOrWhiteSpace(request.Ip))
                {
                    return BadRequest(new { success = false, message = "IP地址不能为空" });
                }

                if (request.Speed < 1 || request.Speed > 7)
                {
                    return BadRequest(new { success = false, message = "速度值必须在1-7之间" });
                }

                // 确保SDK初始化
                EnsureSdkInit();

                // 登录设备
                var deviceInfo = new NET_DVR_DEVICEINFO_V30();
                int userId = NET_DVR_Login_V30(
                    request.Ip, 
                    request.Port, 
                    request.Username, 
                    request.Password, 
                    ref deviceInfo);

                if (userId < 0)
                {
                    return StatusCode(500, new 
                    { 
                        success = false, 
                        message = "登录设备失败，请检查IP、端口、用户名和密码" 
                    });
                }

                try
                {
                    // 执行PTZ控制
                    bool result = NET_DVR_PTZControlWithSpeed_Other(
                        userId,
                        request.Channel,
                        (uint)request.Command,
                        request.Stop,
                        request.Speed);

                    if (!result)
                    {
                        return StatusCode(500, new 
                        { 
                            success = false, 
                            message = "PTZ控制命令执行失败" 
                        });
                    }

                    return Ok(new 
                    { 
                        success = true, 
                        message = "PTZ控制命令执行成功",
                        command = request.Command,
                        stop = request.Stop == 1 ? "停止" : "开始",
                        speed = request.Speed
                    });
                }
                finally
                {
                    // 登出设备
                    NET_DVR_Logout(userId);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"PTZ控制异常: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// PTZ控制请求模型
        /// </summary>
        public class PTZControlRequest
        {
            public string Ip { get; set; }
            public int Port { get; set; } = 8000;
            public string Username { get; set; }
            public string Password { get; set; }
            public int Channel { get; set; } = 1;
            public PTZCommand Command { get; set; }
            public uint Stop { get; set; } = 0;  // 0-开始，1-停止
            public uint Speed { get; set; } = 4; // 1-7
        }

        /// <summary>
        /// FLV实时流 - 超低延迟方案（1-3秒）
        /// 通过HTTP流式传输FLV格式视频，适合云台控制等需要实时反馈的场景
        /// </summary>
        /// <param name="cameraId">摄像头ID</param>
        /// <returns></returns>
        [HttpGet("flv-stream/{cameraId}")]
        public async Task GetFlvStream(string cameraId)
        {
            Process ffmpegProcess = null;
            try
            {
                // 1. 获取摄像头信息
                var camera = await _cameraServices.QueryById(cameraId);
                if (camera == null)
                {
                    Response.StatusCode = 404;
                    await Response.WriteAsync($"摄像头 {cameraId} 不存在");
                    return;
                }

                if (string.IsNullOrWhiteSpace(camera.IP))
                {
                    Response.StatusCode = 400;
                    await Response.WriteAsync("摄像头IP地址为空");
                    return;
                }

                // 2. 构建RTSP URL
                // 注意：这里需要根据实际摄像头配置调整，Camera模型可能需要添加Username和Password字段
                var username = "admin";  // TODO: 从Camera模型获取
                var password = "wzxc2025";  // TODO: 从Camera模型获取
                var port = 554;
                var channel = 1;
                var rtspUrl = $"rtsp://{username}:{password}@{camera.IP}:{port}/Streaming/Channels/{channel}01";

                // 3. 配置HTTP响应
                Response.ContentType = "video/x-flv";
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                // 4. FFmpeg转码参数（超低延迟优化）
                var ffmpegPath = string.IsNullOrWhiteSpace(_ffmpegExecutablePath) 
                    ? "ffmpeg" 
                    : _ffmpegExecutablePath;

                var arguments = $"-rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{rtspUrl}\" " +
                                $"-c:v libx264 -preset ultrafast -tune zerolatency " +
                                $"-profile:v baseline -level 3.0 -pix_fmt yuv420p " +
                                $"-g 30 -sc_threshold 0 -keyint_min 30 " +
                                $"-an " +  // 禁用音频以减少延迟
                                $"-f flv -";

                // 5. 启动FFmpeg进程
                ffmpegProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                // 6. 错误日志处理（异步，避免阻塞主流）
                ffmpegProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        System.Diagnostics.Debug.WriteLine($"[FLV-{cameraId}] {e.Data}");
                    }
                };

                ffmpegProcess.Start();
                ffmpegProcess.BeginErrorReadLine();

                // 7. 将FFmpeg输出流式传输到HTTP响应
                await ffmpegProcess.StandardOutput.BaseStream.CopyToAsync(Response.Body, HttpContext.RequestAborted);
            }
            catch (TaskCanceledException)
            {
                // 客户端断开连接，正常情况
                System.Diagnostics.Debug.WriteLine($"[FLV-{cameraId}] 客户端断开连接");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FLV-{cameraId}] 错误: {ex.Message}");
                
                if (!Response.HasStarted)
                {
                    Response.StatusCode = 500;
                    await Response.WriteAsync($"流传输错误: {ex.Message}");
                }
            }
            finally
            {
                // 8. 清理FFmpeg进程
                if (ffmpegProcess != null)
                {
                    try
                    {
                        if (!ffmpegProcess.HasExited)
                        {
                            ffmpegProcess.Kill();
                            ffmpegProcess.WaitForExit(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[FLV-{cameraId}] 清理进程错误: {ex.Message}");
                    }
                    finally
                    {
                        ffmpegProcess?.Dispose();
                    }
                }
            }
        }
    }
}