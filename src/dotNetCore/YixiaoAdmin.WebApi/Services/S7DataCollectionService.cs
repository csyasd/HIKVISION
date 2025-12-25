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
            _logger.LogInformation("[服务启动] S7 PLC数据采集服务启动");
            _logger.LogDebug("[服务启动] 服务配置 - CollectInterval: {CollectInterval}秒, Rack: {Rack}, Slot: {Slot}, DataBlock: {DataBlock}",
                _configuration.GetValue<int>("S7Plc:CollectInterval", 30),
                _configuration.GetValue<int>("S7Plc:Rack", 0),
                _configuration.GetValue<int>("S7Plc:Slot", 1),
                _configuration.GetValue<int>("S7Plc:DataBlockNumber", 1));

            // 等待应用程序完全启动
            _logger.LogDebug("[服务启动] 等待应用程序完全启动 (5秒)...");
            await Task.Delay(5000, stoppingToken);

            try
            {
                // 加载并连接所有设备
                _logger.LogInformation("[服务启动] 开始加载并连接所有设备...");
                await LoadAndConnectAllDevices(stoppingToken);
                _logger.LogInformation($"[服务启动] 设备连接完成 - 已连接设备数: {_deviceConnections.Values.Count(d => d.IsConnected)}, 总设备数: {_deviceConnections.Count}");

                // 启动设备状态监控任务（独立任务）
                _logger.LogDebug("[服务启动] 启动设备状态监控任务...");
                var monitorTask = Task.Run(() => MonitorDeviceStatus(stoppingToken), stoppingToken);

                // 开始定时采集数据
                _logger.LogInformation("[服务启动] 开始定时采集数据循环...");
                int collectCycle = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    collectCycle++;
                    _logger.LogDebug($"[采集循环] 开始第 {collectCycle} 次数据采集...");
                    var cycleStartTime = DateTime.Now;
                    
                    await CollectAllDevicesData(stoppingToken);
                    
                    var cycleElapsed = (DateTime.Now - cycleStartTime).TotalMilliseconds;
                    _logger.LogDebug($"[采集循环] 第 {collectCycle} 次数据采集完成 - 耗时: {cycleElapsed:F2}ms, 已采集设备数: {_collectedData.Count}");
                    
                    // 获取采集间隔（默认30秒）
                    var interval = _configuration.GetValue<int>("S7Plc:CollectInterval", 30);
                    _logger.LogTrace($"[采集循环] 等待 {interval} 秒后进行下次采集...");
                    await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                }

                // 等待监控任务结束
                _logger.LogInformation("[服务停止] 等待监控任务结束...");
                await monitorTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[服务异常] S7 PLC数据采集服务执行出错");
                _logger.LogDebug($"[服务异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}, 堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 加载并连接所有设备
        /// </summary>
        private async Task LoadAndConnectAllDevices(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            try
            {
                _logger.LogDebug("[设备加载] 开始从数据库加载设备列表...");
                using var scope = _serviceProvider.CreateScope();
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceServices>();
                
                var devices = await deviceService.Query();
                _logger.LogInformation($"[设备加载] 从数据库加载到 {devices.Count} 个设备");
                _logger.LogDebug($"[设备加载] 设备列表: {string.Join(", ", devices.Select(d => $"{d.Name}({d.IP})"))}");

                int successCount = 0;
                int failCount = 0;
                int skipCount = 0;

                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("[设备加载] 收到取消信号，停止加载设备");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(device.IP))
                    {
                        _logger.LogWarning($"[设备加载] 设备 {device.Name} (ID: {device.Id}) IP地址为空，跳过");
                        skipCount++;
                        continue;
                    }

                    try
                    {
                        _logger.LogDebug($"[设备加载] 开始连接设备 - 名称: {device.Name}, IP: {device.IP}, ID: {device.Id}");
                        
                        // 初始化设备在线状态（如果为空则设置为离线）
                        await InitializeDeviceOnlineStatus(device.Id, false);
                        
                        await ConnectDevice(device);
                        
                        if (_deviceConnections.TryGetValue(device.Id, out var connInfo) && connInfo.IsConnected)
                        {
                            successCount++;
                            _logger.LogDebug($"[设备加载] 设备连接成功 - {device.Name}({device.IP})");
                        }
                        else
                        {
                            failCount++;
                            _logger.LogDebug($"[设备加载] 设备连接失败 - {device.Name}({device.IP})");
                        }
                        
                        await Task.Delay(1000, cancellationToken); // 每个设备连接间隔1秒
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        _logger.LogError(ex, $"[设备加载] 连接设备 {device.Name}({device.IP}) 时发生异常");
                        _logger.LogDebug($"[设备加载] 异常详情 - 设备ID: {device.Id}, 异常类型: {ex.GetType().Name}, 消息: {ex.Message}");
                    }
                }

                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogInformation($"[设备加载] 设备加载完成 - 成功: {successCount}, 失败: {failCount}, 跳过: {skipCount}, 总耗时: {elapsed:F2}ms");
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogError(ex, $"[设备加载异常] 加载设备列表失败 - 耗时: {elapsed:F2}ms");
                _logger.LogDebug($"[设备加载异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}, 堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        private async Task ConnectDevice(Device device)
        {
            var startTime = DateTime.Now;
            try
            {
                _logger.LogDebug($"[设备连接] 开始连接设备 - 名称: {device.Name}, IP: {device.IP}, ID: {device.Id}");
                
                // 创建PLC配置
                var config = new S7PlcConfig
                {
                    IpAddress = device.IP,
                    Rack = _configuration.GetValue<int>("S7Plc:Rack", 0),
                    Slot = _configuration.GetValue<int>("S7Plc:Slot", 1),
                    DataBlockNumber = _configuration.GetValue<int>("S7Plc:DataBlockNumber", 1),
                    CollectInterval = _configuration.GetValue<int>("S7Plc:CollectInterval", 30)
                };
                _logger.LogDebug($"[设备连接] PLC配置 - IP: {config.IpAddress}, Rack: {config.Rack}, Slot: {config.Slot}, DB: {config.DataBlockNumber}");

                // 创建连接服务
                var connectionService = new S7PlcConnectionService(config, _plcConnectionLogger);

                // 尝试连接
                _logger.LogDebug($"[设备连接] 尝试建立PLC连接...");
                var connected = await connectionService.ConnectAsync();
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                
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
                    _logger.LogWarning($"[设备连接失败] 设备 {device.Name}({device.IP}) 连接失败，将定期尝试重连 - 耗时: {elapsed:F2}ms");
                    connectionService.Dispose();
                    connectionInfo.DisconnectedTime = DateTime.Now;
                    _logger.LogDebug($"[设备连接失败] 连接信息已保存，将在重连间隔后自动重试");
                    
                    // 更新数据库状态为离线
                    await UpdateDeviceOnlineStatus(device.Id, false);
                }
                else
                {
                    _logger.LogInformation($"[设备连接成功] 设备 {device.Name}({device.IP}) 连接成功 - 耗时: {elapsed:F2}ms");
                    _logger.LogDebug($"[设备连接成功] 连接状态 - IsConnected: {connectionService.IsConnected}, DeviceId: {device.Id}");
                    
                    // 更新数据库状态为在线
                    await UpdateDeviceOnlineStatus(device.Id, true);
                }

                // 保存连接信息（无论连接成功与否）
                _deviceConnections.AddOrUpdate(device.Id, connectionInfo, (key, oldValue) => 
                {
                    _logger.LogDebug($"[设备连接] 更新已存在的设备连接信息 - DeviceId: {device.Id}, 旧状态: {oldValue.IsConnected}, 新状态: {connectionInfo.IsConnected}");
                    return connectionInfo;
                });
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogError(ex, $"[设备连接异常] 连接设备 {device.Name}({device.IP}) 时出错 - 耗时: {elapsed:F2}ms");
                _logger.LogDebug($"[设备连接异常] 异常详情 - 设备ID: {device.Id}, 异常类型: {ex.GetType().Name}, 消息: {ex.Message}, 堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 采集所有设备数据（只采集在线设备）
        /// </summary>
        private async Task CollectAllDevicesData(CancellationToken cancellationToken)
        {
            var cycleStartTime = DateTime.Now;
            var dataReader = new S7DataReaderService(_dataReaderLogger);
            
            var totalDevices = _deviceConnections.Count;
            var onlineDevices = _deviceConnections.Values.Count(d => d.IsConnected);
            _logger.LogDebug($"[数据采集] 开始采集数据 - 总设备数: {totalDevices}, 在线设备数: {onlineDevices}");

            int successCount = 0;
            int failCount = 0;
            int skipCount = 0;

            foreach (var kvp in _deviceConnections)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("[数据采集] 收到取消信号，停止数据采集");
                    break;
                }

                var connectionInfo = kvp.Value;
                var deviceStartTime = DateTime.Now;
                
                // 只采集在线设备的数据
                if (!connectionInfo.IsConnected || connectionInfo.ConnectionService == null)
                {
                    skipCount++;
                    _logger.LogTrace($"[数据采集] 跳过离线设备 - {connectionInfo.DeviceName}({connectionInfo.DeviceIP}), 原因: {(connectionInfo.ConnectionService == null ? "连接服务为空" : "未连接")}");
                    continue;
                }

                try
                {
                    // 检查连接状态
                    if (!connectionInfo.ConnectionService.IsConnected)
                    {
                        _logger.LogWarning($"[数据采集] 设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 连接已断开");
                        connectionInfo.IsConnected = false;
                        connectionInfo.DisconnectedTime = DateTime.Now;
                        
                        // 标记设备为离线并更新数据库
                        await UpdateDeviceOnlineStatus(connectionInfo.DeviceId, false);
                        
                        skipCount++;
                        continue;
                    }

                    _logger.LogDebug($"[数据采集] 开始采集设备数据 - {connectionInfo.DeviceName}({connectionInfo.DeviceIP})");
                    
                    // 读取数据
                    var data = await dataReader.ReadAllDataAsync(
                        connectionInfo.ConnectionService, 
                        connectionInfo.DeviceId);

                    var deviceElapsed = (DateTime.Now - deviceStartTime).TotalMilliseconds;

                    // 保存到内存（临时存储）
                    var isNewData = !_collectedData.ContainsKey(connectionInfo.DeviceId);
                    _collectedData.AddOrUpdate(connectionInfo.DeviceId, data, (key, oldValue) => data);

                    connectionInfo.LastCollectTime = DateTime.Now;
                    connectionInfo.IsConnected = data.IsConnected;

                    // 如果采集成功，重置重连计数并保存到数据库
                    if (data.IsConnected)
                    {
                        connectionInfo.ReconnectAttemptCount = 0;
                        connectionInfo.DisconnectedTime = null;
                        successCount++;
                        _logger.LogDebug($"[数据采集成功] 设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) - 耗时: {deviceElapsed:F2}ms, 数据时间: {data.CollectTime:yyyy-MM-dd HH:mm:ss.fff}, {(isNewData ? "新数据" : "更新数据")}");
                        
                        // 保存数据到数据库（异步执行，不阻塞采集流程）
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                using var saveScope = _serviceProvider.CreateScope();
                                var saveService = new S7DataSaveService(
                                    saveScope.ServiceProvider.GetRequiredService<ILogger<S7DataSaveService>>(),
                                    saveScope.ServiceProvider.GetRequiredService<IWorkOrderServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IDeviceServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IWorkBraceletServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IWorkRecordServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IGasAlarmRecordServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IWorkerStatusRecordServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IBraceletAbnormalServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IGasAbnormalServices>(),
                                    saveScope.ServiceProvider.GetRequiredService<IAbnormalConfigServices>());
                                
                                await saveService.SaveDataToDatabase(data, connectionInfo.DeviceEntity);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"[数据保存] 异步保存数据到数据库时发生异常 - 设备: {connectionInfo.DeviceName}");
                            }
                        });
                    }
                    else
                    {
                        failCount++;
                        _logger.LogWarning($"[数据采集失败] 设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 采集失败 - 耗时: {deviceElapsed:F2}ms, IsConnected: {data.IsConnected}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    var deviceElapsed = (DateTime.Now - deviceStartTime).TotalMilliseconds;
                    _logger.LogError(ex, $"[数据采集异常] 采集设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 数据时出错 - 耗时: {deviceElapsed:F2}ms");
                    _logger.LogDebug($"[数据采集异常] 异常详情 - 设备ID: {connectionInfo.DeviceId}, 异常类型: {ex.GetType().Name}, 消息: {ex.Message}");
                    connectionInfo.IsConnected = false;
                    if (connectionInfo.DisconnectedTime == null)
                    {
                        connectionInfo.DisconnectedTime = DateTime.Now;
                        _logger.LogDebug($"[数据采集异常] 记录设备断开时间: {connectionInfo.DisconnectedTime:yyyy-MM-dd HH:mm:ss}");
                    }
                    
                    // 采集异常时也更新为离线
                    await UpdateDeviceOnlineStatus(connectionInfo.DeviceId, false);
                }
            }

            var cycleElapsed = (DateTime.Now - cycleStartTime).TotalMilliseconds;
            _logger.LogDebug($"[数据采集] 数据采集循环完成 - 成功: {successCount}, 失败: {failCount}, 跳过: {skipCount}, 总耗时: {cycleElapsed:F2}ms");
        }

        /// <summary>
        /// 更新设备在线状态到数据库
        /// </summary>
        private async Task UpdateDeviceOnlineStatus(string deviceId, bool isOnline)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceServices>();
                var device = await deviceService.QueryById(deviceId);
                
                if (device != null)
                {
                    // 确保状态只有"在线"或"离线"两种
                    device.OnlineStatus = isOnline ? "在线" : "离线";
                    await deviceService.Update(device);
                    _logger.LogDebug($"[状态更新] 设备 {device.Name} 在线状态更新为: {device.OnlineStatus}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[状态更新异常] 更新设备 {deviceId} 在线状态时出错");
            }
        }

        /// <summary>
        /// 初始化设备在线状态（加载设备时调用，确保所有设备都有初始状态）
        /// </summary>
        private async Task InitializeDeviceOnlineStatus(string deviceId, bool isOnline)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceServices>();
                var device = await deviceService.QueryById(deviceId);
                
                if (device != null && string.IsNullOrWhiteSpace(device.OnlineStatus))
                {
                    // 如果设备没有在线状态，初始化为离线
                    device.OnlineStatus = isOnline ? "在线" : "离线";
                    await deviceService.Update(device);
                    _logger.LogDebug($"[状态初始化] 设备 {device.Name} 在线状态初始化为: {device.OnlineStatus}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[状态初始化异常] 初始化设备 {deviceId} 在线状态时出错");
            }
        }

        /// <summary>
        /// 监控设备状态并自动重连离线设备
        /// </summary>
        private async Task MonitorDeviceStatus(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[状态监控] 设备状态监控任务启动");

            // 获取重连间隔（默认3分钟）
            var reconnectInterval = _configuration.GetValue<int>("S7Plc:ReconnectInterval", 180);
            _logger.LogDebug($"[状态监控] 监控配置 - 重连间隔: {reconnectInterval}秒, 检查间隔: 30秒");

            int checkCycle = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    checkCycle++;
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken); // 每30秒检查一次

                    var now = DateTime.Now;
                    var totalDevices = _deviceConnections.Count;
                    var onlineDevices = _deviceConnections.Values.Count(d => d.IsConnected);
                    var offlineDevices = _deviceConnections.Values
                        .Where(d => !d.IsConnected)
                        .ToList();

                    _logger.LogDebug($"[状态监控] 第 {checkCycle} 次状态检查 - 总设备: {totalDevices}, 在线: {onlineDevices}, 离线: {offlineDevices.Count}");

                    int reconnectAttemptCount = 0;
                    foreach (var connectionInfo in offlineDevices)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogWarning("[状态监控] 收到取消信号，停止状态监控");
                            break;
                        }

                        // 检查是否到了重连时间
                        var timeSinceLastAttempt = (now - connectionInfo.LastConnectAttemptTime).TotalSeconds;
                        var offlineDuration = connectionInfo.DisconnectedTime.HasValue 
                            ? (now - connectionInfo.DisconnectedTime.Value).TotalSeconds 
                            : 0;
                        
                        _logger.LogTrace($"[状态监控] 检查设备 - {connectionInfo.DeviceName}({connectionInfo.DeviceIP}), 距上次尝试: {timeSinceLastAttempt:F1}秒, 离线时长: {offlineDuration:F1}秒, 重连次数: {connectionInfo.ReconnectAttemptCount}");
                        
                        if (timeSinceLastAttempt >= reconnectInterval)
                        {
                            reconnectAttemptCount++;
                            _logger.LogInformation($"[状态监控] 尝试重连离线设备: {connectionInfo.DeviceName}({connectionInfo.DeviceIP})，第 {connectionInfo.ReconnectAttemptCount + 1} 次尝试，离线时长: {offlineDuration:F1}秒");
                            
                            await ReconnectDevice(connectionInfo);
                            connectionInfo.LastConnectAttemptTime = now;
                            connectionInfo.ReconnectAttemptCount++;
                        }
                    }

                    if (reconnectAttemptCount > 0)
                    {
                        _logger.LogDebug($"[状态监控] 本次检查尝试重连 {reconnectAttemptCount} 个设备");
                    }

                    // 检查是否有新设备需要连接（从数据库加载）
                    await CheckAndConnectNewDevices(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[状态监控异常] 监控设备状态时出错 - 检查周期: {checkCycle}");
                    _logger.LogDebug($"[状态监控异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}, 堆栈: {ex.StackTrace}");
                }
            }

            _logger.LogInformation($"[状态监控] 设备状态监控任务已停止 - 总检查次数: {checkCycle}");
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
            var startTime = DateTime.Now;
            try
            {
                _logger.LogDebug($"[设备重连] 开始重连设备 - {connectionInfo.DeviceName}({connectionInfo.DeviceIP}), 重连次数: {connectionInfo.ReconnectAttemptCount + 1}");
                
                // 如果连接服务存在，先断开
                if (connectionInfo.ConnectionService != null)
                {
                    try
                    {
                        _logger.LogDebug($"[设备重连] 断开旧连接...");
                        await connectionInfo.ConnectionService.DisconnectAsync();
                        connectionInfo.ConnectionService.Dispose();
                        _logger.LogDebug($"[设备重连] 旧连接已断开");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"[设备重连] 断开设备 {connectionInfo.DeviceName} 旧连接时出错");
                        _logger.LogDebug($"[设备重连] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                    }
                }

                _logger.LogDebug($"[设备重连] 等待2秒后重新连接...");
                await Task.Delay(2000); // 等待2秒

                // 重新创建连接服务
                var config = new S7PlcConfig
                {
                    IpAddress = connectionInfo.DeviceIP,
                    Rack = _configuration.GetValue<int>("S7Plc:Rack", 0),
                    Slot = _configuration.GetValue<int>("S7Plc:Slot", 1),
                    DataBlockNumber = _configuration.GetValue<int>("S7Plc:DataBlockNumber", 1),
                    CollectInterval = _configuration.GetValue<int>("S7Plc:CollectInterval", 30)
                };
                _logger.LogDebug($"[设备重连] PLC配置 - IP: {config.IpAddress}, Rack: {config.Rack}, Slot: {config.Slot}, DB: {config.DataBlockNumber}");

                var newConnectionService = new S7PlcConnectionService(config, _plcConnectionLogger);
                _logger.LogDebug($"[设备重连] 尝试建立新连接...");
                var connected = await newConnectionService.ConnectAsync();
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                
                if (connected)
                {
                    connectionInfo.ConnectionService = newConnectionService;
                    connectionInfo.IsConnected = true;
                    connectionInfo.DisconnectedTime = null;
                    connectionInfo.ReconnectAttemptCount = 0;
                    _logger.LogInformation($"[设备重连成功] 设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 重连成功 - 耗时: {elapsed:F2}ms, 重连次数: {connectionInfo.ReconnectAttemptCount + 1}");
                    _logger.LogDebug($"[设备重连成功] 连接状态 - IsConnected: {newConnectionService.IsConnected}, DeviceId: {connectionInfo.DeviceId}");
                    
                    // 更新数据库状态为在线
                    await UpdateDeviceOnlineStatus(connectionInfo.DeviceId, true);
                }
                else
                {
                    newConnectionService.Dispose();
                    connectionInfo.IsConnected = false;
                    var nextRetryTime = _configuration.GetValue<int>("S7Plc:ReconnectInterval", 180);
                    _logger.LogWarning($"[设备重连失败] 设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 重连失败 - 耗时: {elapsed:F2}ms, 将在 {nextRetryTime} 秒后重试");
                    _logger.LogDebug($"[设备重连失败] 连接状态 - IsConnected: false, 下次重连时间: {connectionInfo.LastConnectAttemptTime.AddSeconds(nextRetryTime):yyyy-MM-dd HH:mm:ss}");
                    
                    // 确保数据库状态为离线
                    await UpdateDeviceOnlineStatus(connectionInfo.DeviceId, false);
                }
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogError(ex, $"[设备重连异常] 重连设备 {connectionInfo.DeviceName}({connectionInfo.DeviceIP}) 时出错 - 耗时: {elapsed:F2}ms");
                _logger.LogDebug($"[设备重连异常] 异常详情 - 设备ID: {connectionInfo.DeviceId}, 异常类型: {ex.GetType().Name}, 消息: {ex.Message}, 堆栈: {ex.StackTrace}");
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

