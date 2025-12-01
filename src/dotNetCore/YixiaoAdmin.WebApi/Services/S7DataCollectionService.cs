using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;
using YixiaoAdmin.WebApi.Services;

namespace YixiaoAdmin.WebApi.Services
{
    /// <summary>
    /// S7 PLC数据采集后台服务
    /// </summary>
    public class S7DataCollectionService : BackgroundService
    {
        private readonly ILogger<S7DataCollectionService> _logger;
        private readonly ILogger<S7PlcConnectionService> _plcConnectionLogger;
        private readonly ILogger<S7DataReaderService> _dataReaderLogger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        
        // 设备连接管理
        private readonly ConcurrentDictionary<string, S7DeviceConnectionInfo> _deviceConnections = new();
        
        // 采集的数据存储（临时，不保存到数据库）
        private readonly ConcurrentDictionary<string, S7DataCollectionModel> _collectedData = new();

        public S7DataCollectionService(
            ILogger<S7DataCollectionService> logger,
            ILogger<S7PlcConnectionService> plcConnectionLogger,
            ILogger<S7DataReaderService> dataReaderLogger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _plcConnectionLogger = plcConnectionLogger;
            _dataReaderLogger = dataReaderLogger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("S7 PLC数据采集服务启动");

            // 等待应用程序完全启动
            await Task.Delay(5000, stoppingToken);

            try
            {
                // 加载并连接所有设备
                await LoadAndConnectAllDevices(stoppingToken);

                // 启动设备状态监控任务（独立任务）
                var monitorTask = Task.Run(() => MonitorDeviceStatus(stoppingToken), stoppingToken);

                // 开始定时采集数据
                while (!stoppingToken.IsCancellationRequested)
                {
                    await CollectAllDevicesData(stoppingToken);
                    
                    // 获取采集间隔（默认5秒）
                    var interval = _configuration.GetValue<int>("S7Plc:CollectInterval", 5);
                    await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                }

                // 等待监控任务结束
                await monitorTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S7 PLC数据采集服务执行出错");
            }
        }

        /// <summary>
        /// 加载并连接所有设备
        /// </summary>
        private async Task LoadAndConnectAllDevices(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceServices>();
                
                var devices = await deviceService.Query();
                _logger.LogInformation($"加载到 {devices.Count} 个设备");

                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (string.IsNullOrWhiteSpace(device.IP))
                    {
                        _logger.LogWarning($"设备 {device.Name} IP地址为空，跳过");
                        continue;
                    }

                    try
                    {
                        await ConnectDevice(device);
                        await Task.Delay(1000, cancellationToken); // 每个设备连接间隔1秒
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"连接设备 {device.Name}({device.IP}) 失败");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载设备列表失败");
            }
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        private async Task ConnectDevice(Device device)
        {
            try
            {
                // 创建PLC配置
                var config = new S7PlcConfig
                {
                    IpAddress = device.IP,
                    Rack = _configuration.GetValue<int>("S7Plc:Rack", 0),
                    Slot = _configuration.GetValue<int>("S7Plc:Slot", 1),
                    DataBlockNumber = _configuration.GetValue<int>("S7Plc:DataBlockNumber", 1),
                    CollectInterval = _configuration.GetValue<int>("S7Plc:CollectInterval", 5)
                };

                // 创建连接服务
                var connectionService = new S7PlcConnectionService(config, _plcConnectionLogger);

                // 尝试连接
                var connected = await connectionService.ConnectAsync();
                
                // 创建连接信息（无论连接成功与否都要记录，以便监控任务可以重连）
                var connectionInfo = new S7DeviceConnectionInfo
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    DeviceIP = device.IP,
                    ConnectionService = connected ? connectionService : null,
                    LastCollectTime = DateTime.MinValue,
                    LastConnectAttemptTime = DateTime.Now,
                    IsConnected = connected,
                    ReconnectAttemptCount = 0,
                    DeviceEntity = device
                };

                if (!connected)
                {
                    _logger.LogWarning($"设备 {device.Name}({device.IP}) 连接失败，将定期尝试重连");
                    connectionService.Dispose();
                    connectionInfo.DisconnectedTime = DateTime.Now;
                }
                else
                {
                    _logger.LogInformation($"设备 {device.Name}({device.IP}) 连接成功");
                }

                // 保存连接信息（无论连接成功与否）
                _deviceConnections.AddOrUpdate(device.Id, connectionInfo, (key, oldValue) => connectionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"连接设备 {device.Name}({device.IP}) 时出错");
            }
        }

        /// <summary>
        /// 采集所有设备数据（只采集在线设备）
        /// </summary>
        private async Task CollectAllDevicesData(CancellationToken cancellationToken)
        {
            var dataReader = new S7DataReaderService(_dataReaderLogger);

            foreach (var kvp in _deviceConnections)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var connectionInfo = kvp.Value;
                
                // 只采集在线设备的数据
                if (!connectionInfo.IsConnected || connectionInfo.ConnectionService == null)
                {
                    continue;
                }

                try
                {
                    // 检查连接状态
                    if (!connectionInfo.ConnectionService.IsConnected)
                    {
                        _logger.LogWarning($"设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 连接已断开");
                        connectionInfo.IsConnected = false;
                        connectionInfo.DisconnectedTime = DateTime.Now;
                        continue;
                    }

                    // 读取数据
                    var data = await dataReader.ReadAllDataAsync(
                        connectionInfo.ConnectionService, 
                        connectionInfo.DeviceId);

                    // 保存到内存（临时存储）
                    _collectedData.AddOrUpdate(connectionInfo.DeviceId, data, (key, oldValue) => data);

                    connectionInfo.LastCollectTime = DateTime.Now;
                    connectionInfo.IsConnected = data.IsConnected;

                    // 如果采集成功，重置重连计数
                    if (data.IsConnected)
                    {
                        connectionInfo.ReconnectAttemptCount = 0;
                        connectionInfo.DisconnectedTime = null;
                    }

                    _logger.LogDebug($"成功采集设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 数据");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"采集设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 数据时出错");
                    connectionInfo.IsConnected = false;
                    if (connectionInfo.DisconnectedTime == null)
                    {
                        connectionInfo.DisconnectedTime = DateTime.Now;
                    }
                }
            }
        }

        /// <summary>
        /// 监控设备状态并自动重连离线设备
        /// </summary>
        private async Task MonitorDeviceStatus(CancellationToken cancellationToken)
        {
            _logger.LogInformation("设备状态监控任务启动");

            // 获取重连间隔（默认3分钟）
            var reconnectInterval = _configuration.GetValue<int>("S7Plc:ReconnectInterval", 180);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken); // 每30秒检查一次

                    var now = DateTime.Now;
                    var offlineDevices = _deviceConnections.Values
                        .Where(d => !d.IsConnected)
                        .ToList();

                    foreach (var connectionInfo in offlineDevices)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        // 检查是否到了重连时间
                        var timeSinceLastAttempt = (now - connectionInfo.LastConnectAttemptTime).TotalSeconds;
                        
                        if (timeSinceLastAttempt >= reconnectInterval)
                        {
                            _logger.LogInformation($"尝试重连离线设备: {connectionInfo.DeviceName}({connectionInfo.DeviceIP})，第 {connectionInfo.ReconnectAttemptCount + 1} 次尝试");
                            
                            await ReconnectDevice(connectionInfo);
                            connectionInfo.LastConnectAttemptTime = now;
                            connectionInfo.ReconnectAttemptCount++;
                        }
                    }

                    // 检查是否有新设备需要连接（从数据库加载）
                    await CheckAndConnectNewDevices(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "监控设备状态时出错");
                }
            }

            _logger.LogInformation("设备状态监控任务已停止");
        }

        /// <summary>
        /// 检查并连接新设备（从数据库加载）
        /// </summary>
        private async Task CheckAndConnectNewDevices(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceServices>();
                
                var devices = await deviceService.Query();
                
                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (string.IsNullOrWhiteSpace(device.IP))
                    {
                        continue;
                    }

                        // 如果设备不在连接列表中，尝试连接
                    if (!_deviceConnections.ContainsKey(device.Id))
                    {
                        _logger.LogInformation($"发现新设备，尝试连接: {device.Name}({device.IP})");
                        await ConnectDevice(device);
                        await Task.Delay(1000, cancellationToken); // 每个设备连接间隔1秒
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查新设备时出错");
            }
        }

        /// <summary>
        /// 重连设备
        /// </summary>
        private async Task ReconnectDevice(S7DeviceConnectionInfo connectionInfo)
        {
            try
            {
                // 如果连接服务存在，先断开
                if (connectionInfo.ConnectionService != null)
                {
                    try
                    {
                        await connectionInfo.ConnectionService.DisconnectAsync();
                        connectionInfo.ConnectionService.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"断开设备 {connectionInfo.DeviceName} 旧连接时出错");
                    }
                }

                await Task.Delay(2000); // 等待2秒

                // 重新创建连接服务
                var config = new S7PlcConfig
                {
                    IpAddress = connectionInfo.DeviceIP,
                    Rack = _configuration.GetValue<int>("S7Plc:Rack", 0),
                    Slot = _configuration.GetValue<int>("S7Plc:Slot", 1),
                    DataBlockNumber = _configuration.GetValue<int>("S7Plc:DataBlockNumber", 1),
                    CollectInterval = _configuration.GetValue<int>("S7Plc:CollectInterval", 5)
                };

                var newConnectionService = new S7PlcConnectionService(config, _plcConnectionLogger);
                var connected = await newConnectionService.ConnectAsync();
                
                if (connected)
                {
                    connectionInfo.ConnectionService = newConnectionService;
                    connectionInfo.IsConnected = true;
                    connectionInfo.DisconnectedTime = null;
                    connectionInfo.ReconnectAttemptCount = 0;
                    _logger.LogInformation($"设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 重连成功");
                }
                else
                {
                    newConnectionService.Dispose();
                    connectionInfo.IsConnected = false;
                    _logger.LogWarning($"设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 重连失败，将在 {_configuration.GetValue<int>("S7Plc:ReconnectInterval", 180)} 秒后重试");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"重连设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 时出错");
                connectionInfo.IsConnected = false;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止S7 PLC数据采集服务...");

            // 断开所有设备连接
            foreach (var connectionInfo in _deviceConnections.Values)
            {
                try
                {
                    if (connectionInfo.ConnectionService != null)
                    {
                        await connectionInfo.ConnectionService.DisconnectAsync();
                        connectionInfo.ConnectionService.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"断开设备 {connectionInfo.DeviceName} 连接时出错");
                }
            }

            _deviceConnections.Clear();
            _collectedData.Clear();

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("S7 PLC数据采集服务已停止");
        }

        /// <summary>
        /// 获取指定设备的最新采集数据
        /// </summary>
        public S7DataCollectionModel? GetDeviceData(string deviceId)
        {
            return _collectedData.TryGetValue(deviceId, out var data) ? data : null;
        }

        /// <summary>
        /// 获取所有设备的最新采集数据
        /// </summary>
        public Dictionary<string, S7DataCollectionModel> GetAllDeviceData()
        {
            return _collectedData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// 获取设备连接状态
        /// </summary>
        public Dictionary<string, object> GetDeviceConnectionStatus()
        {
            return _deviceConnections.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new
                {
                    kvp.Value.DeviceId,
                    kvp.Value.DeviceName,
                    kvp.Value.DeviceIP,
                    kvp.Value.IsConnected,
                    kvp.Value.LastCollectTime,
                    kvp.Value.DisconnectedTime,
                    kvp.Value.ReconnectAttemptCount,
                    kvp.Value.LastConnectAttemptTime,
                    OfflineDuration = kvp.Value.DisconnectedTime.HasValue 
                        ? (DateTime.Now - kvp.Value.DisconnectedTime.Value).TotalSeconds 
                        : (double?)null
                });
        }
    }

    /// <summary>
    /// 设备连接信息
    /// </summary>
    public class S7DeviceConnectionInfo
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIP { get; set; }
        public S7PlcConnectionService ConnectionService { get; set; }
        public DateTime LastCollectTime { get; set; }
        public DateTime LastConnectAttemptTime { get; set; }
        public DateTime? DisconnectedTime { get; set; }
        public bool IsConnected { get; set; }
        public int ReconnectAttemptCount { get; set; }
        public Device DeviceEntity { get; set; }
    }
}

