using S7.Net;
using YixiaoAdmin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YixiaoAdmin.WebApi.Services
{
    /// <summary>
    /// S7 PLC连接服务（管理单个PLC连接）
    /// </summary>
    public class S7PlcConnectionService : IDisposable
    {
        private readonly Plc _plc;
        private readonly S7PlcConfig _config;
        private readonly ILogger<S7PlcConnectionService> _logger;

        public S7PlcConnectionService(S7PlcConfig config, ILogger<S7PlcConnectionService> logger)
        {
            _config = config;
            _logger = logger;
            _plc = new Plc(CpuType.S71200, config.IpAddress, (short)config.Rack, (short)config.Slot);
        }

        /// <summary>
        /// 连接到PLC
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            var startTime = DateTime.Now;
            try
            {
                _logger.LogInformation($"[连接] 开始连接PLC - IP: {_config.IpAddress}, Rack: {_config.Rack}, Slot: {_config.Slot}");
                _logger.LogDebug($"[连接] PLC配置信息 - DataBlock: {_config.DataBlockNumber}, CollectInterval: {_config.CollectInterval}秒");
                
                await Task.Run(() => _plc.Open());
                var result = _plc.IsConnected ? ErrorCode.NoError : ErrorCode.ConnectionError;
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                
                if (result == ErrorCode.NoError)
                {
                    _logger.LogInformation($"[连接成功] PLC连接成功 - IP: {_config.IpAddress}, 耗时: {elapsed:F2}ms");
                    _logger.LogDebug($"[连接成功] 连接状态验证 - IsConnected: {_plc.IsConnected}");
                    return true;
                }
                else
                {
                    _logger.LogError($"[连接失败] PLC连接失败 - IP: {_config.IpAddress}, 错误码: {result}, 耗时: {elapsed:F2}ms");
                    _logger.LogDebug($"[连接失败] 连接状态 - IsConnected: {_plc.IsConnected}, ErrorCode: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogError(ex, $"[连接异常] 连接PLC时发生异常 - IP: {_config.IpAddress}, 耗时: {elapsed:F2}ms");
                _logger.LogDebug($"[连接异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_plc.IsConnected)
                {
                    await Task.Run(() => _plc.Close());
                    _logger.LogInformation($"PLC连接已断开: {_config.IpAddress}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"断开PLC连接时发生异常: {_config.IpAddress}");
            }
        }

        /// <summary>
        /// 检查PLC连接状态
        /// </summary>
        public bool IsConnected => _plc.IsConnected;

        /// <summary>
        /// 获取PLC对象（用于数据读取）
        /// </summary>
        public Plc GetPlc() => _plc;

        /// <summary>
        /// 获取配置
        /// </summary>
        public S7PlcConfig GetConfig() => _config;

        public void Dispose()
        {
            try
            {
                if (_plc.IsConnected)
                {
                    _plc.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"释放S7PlcConnectionService资源时发生异常: {_config.IpAddress}");
            }
        }
    }

    /// <summary>
    /// S7 PLC数据读取服务（统一的数据读取方法）
    /// </summary>
    public class S7DataReaderService
    {
        private readonly ILogger<S7DataReaderService> _logger;

        public S7DataReaderService(ILogger<S7DataReaderService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 读取所有数据（统一入口）
        /// 根据PLC数据块的实际偏移量读取数据
        /// </summary>
        public async Task<S7DataCollectionModel> ReadAllDataAsync(S7PlcConnectionService connectionService, string deviceId)
        {
            var startTime = DateTime.Now;
            var data = new S7DataCollectionModel
            {
                DeviceId = deviceId,
                DeviceIP = connectionService.GetConfig().IpAddress,
                CollectTime = DateTime.Now,
                IsConnected = connectionService.IsConnected
            };

            _logger.LogDebug($"[数据读取] 开始读取数据 - DeviceId: {deviceId}, IP: {data.DeviceIP}, 时间: {data.CollectTime:yyyy-MM-dd HH:mm:ss.fff}");

            if (!connectionService.IsConnected)
            {
                _logger.LogWarning($"[数据读取] PLC未连接，无法读取数据 - DeviceId: {deviceId}, IP: {data.DeviceIP}");
                _logger.LogDebug($"[数据读取] 连接状态检查 - IsConnected: {connectionService.IsConnected}");
                return data;
            }

            var plc = connectionService.GetPlc();
            var dbNumber = connectionService.GetConfig().DataBlockNumber;
            _logger.LogDebug($"[数据读取] PLC连接信息 - DB编号: {dbNumber}, 连接状态: {plc.IsConnected}");

            try
            {
                // 检查数据块大小（根据偏移量表，最大偏移量是38332，加上Real类型的4字节，至少需要38336字节）
                const int requiredDbSize = 38336; // 最小需要的数据块大小
                _logger.LogDebug($"[数据读取] 检查数据块大小 - DB{dbNumber}, 需要至少: {requiredDbSize}字节");
                
                try
                {
                    // 尝试读取数据块末尾的数据来验证数据块大小
                    var testBytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, requiredDbSize - 4, 4));
                    if (testBytes == null || testBytes.Length < 4)
                    {
                        _logger.LogWarning($"[数据读取] 数据块大小可能不足 - DB{dbNumber}, 尝试读取偏移量 {requiredDbSize - 4} 失败");
                        _logger.LogWarning($"[数据读取] 请检查PLC中DB{dbNumber}的大小，至少需要 {requiredDbSize} 字节");
                    }
                    else
                    {
                        _logger.LogDebug($"[数据读取] 数据块大小检查通过 - DB{dbNumber}");
                    }
                }
                catch (Exception sizeCheckEx)
                {
                    _logger.LogError(sizeCheckEx, $"[数据读取] 数据块大小检查失败 - DB{dbNumber}, 偏移量: {requiredDbSize - 4}");
                    _logger.LogWarning($"[数据读取] 错误: {sizeCheckEx.Message}");
                    _logger.LogWarning($"[数据读取] 请确认PLC中DB{dbNumber}的大小至少为 {requiredDbSize} 字节");
                    _logger.LogWarning($"[数据读取] 如果使用S7-1500，请确保数据块属性设置为'非优化访问'");
                    
                    // 不抛出异常，继续尝试读取，让具体的读取操作来报告错误
                }

                _logger.LogDebug($"[数据读取] 开始读取11个工单数据...");
                // ============================================
                // 读取11个工单数据（Construction_Order[0-10]）
                // 每个工单从偏移量开始：0, 3190, 6380, 9570, 12760, 15950, 19140, 22330, 25520, 28710, 31900
                // ============================================
                int[] orderOffsets = { 0, 3190, 6380, 9570, 12760, 15950, 19140, 22330, 25520, 28710, 31900 };
                
                for (int orderIndex = 0; orderIndex < 11; orderIndex++)
                {
                    var orderStartTime = DateTime.Now;
                    int orderBaseOffset = orderOffsets[orderIndex];
                    var order = data.Construction_Order[orderIndex];
                    _logger.LogDebug($"[工单{orderIndex}] 开始读取工单数据 - 偏移量: {orderBaseOffset}");

                    // 读取Workers_Name数组（0-10），每个String占256字节
                    _logger.LogDebug($"[工单{orderIndex}] 读取Workers_Name数组 (11个String, 每个256字节)...");
                    for (int i = 0; i <= 10; i++)
                    {
                        int nameOffset = orderBaseOffset + i * 256;
                        order.Workers_Name[i] = await ReadStringAsync(plc, dbNumber, nameOffset, 256) ?? string.Empty;
                        if (!string.IsNullOrEmpty(order.Workers_Name[i]))
                        {
                            _logger.LogDebug($"[工单{orderIndex}] Workers_Name[{i}] = '{order.Workers_Name[i]}' (偏移量: {nameOffset})");
                        }
                    }

                    // 读取Construction_Order_No (Int，2字节)，偏移量：2816
                    order.Construction_Order_No = await ReadIntAsync(plc, dbNumber, orderBaseOffset + 2816) ?? 0;
                    _logger.LogDebug($"[工单{orderIndex}] Construction_Order_No = {order.Construction_Order_No} (偏移量: {orderBaseOffset + 2816})");

                    // 读取SmartBand_No数组（0-10），每个Int占2字节，偏移量：2818
                    _logger.LogDebug($"[工单{orderIndex}] 读取SmartBand_No数组 (11个Int, 每个2字节)...");
                    for (int i = 0; i <= 10; i++)
                    {
                        int bandOffset = orderBaseOffset + 2818 + i * 2;
                        order.SmartBand_No[i] = await ReadIntAsync(plc, dbNumber, bandOffset) ?? 0;
                        if (order.SmartBand_No[i] != 0)
                        {
                            _logger.LogDebug($"[工单{orderIndex}] SmartBand_No[{i}] = {order.SmartBand_No[i]} (偏移量: {bandOffset})");
                        }
                    }

                    // 读取Construction_Order_Content (String，256字节)，偏移量：2840
                    order.Construction_Order_Content = await ReadStringAsync(plc, dbNumber, orderBaseOffset + 2840, 256) ?? string.Empty;
                    if (!string.IsNullOrEmpty(order.Construction_Order_Content))
                    {
                        _logger.LogDebug($"[工单{orderIndex}] Construction_Order_Content = '{order.Construction_Order_Content}' (偏移量: {orderBaseOffset + 2840})");
                    }

                    // 读取Workers_Status数组（0-10），每个Int占2字节，偏移量：3096
                    _logger.LogDebug($"[工单{orderIndex}] 读取Workers_Status数组 (11个Int, 每个2字节)...");
                    for (int i = 0; i <= 10; i++)
                    {
                        int statusOffset = orderBaseOffset + 3096 + i * 2;
                        order.Workers_Status[i] = await ReadIntAsync(plc, dbNumber, statusOffset) ?? 0;
                        if (order.Workers_Status[i] != 0)
                        {
                            _logger.LogDebug($"[工单{orderIndex}] Workers_Status[{i}] = {order.Workers_Status[i]} (0=未进入,1=申请进入,2=刷卡成功,3=进入,4=申请签出,5=已签出) (偏移量: {statusOffset})");
                        }
                    }

                    // 读取Construction_Status (Int，2字节)，偏移量：3118
                    order.Construction_Status = await ReadIntAsync(plc, dbNumber, orderBaseOffset + 3118) ?? 0;
                    _logger.LogDebug($"[工单{orderIndex}] Construction_Status = {order.Construction_Status} (0=未开始,1=工单开始,2=工单结束) (偏移量: {orderBaseOffset + 3118})");

                    // 读取Button_In数组（0-10），Bool类型，打包在字节中
                    // 偏移量：3120，Button_In[0-7]在3120字节的0-7位，Button_In[8-10]在3121字节的0-2位
                    _logger.LogDebug($"[工单{orderIndex}] 读取Button_In数组 (11个Bool, 位打包)...");
                    var buttonInBytes = await ReadBytesAsync(plc, dbNumber, orderBaseOffset + 3120, 2);
                    if (buttonInBytes != null && buttonInBytes.Length >= 2)
                    {
                        _logger.LogDebug($"[工单{orderIndex}] Button_In字节数据: [{buttonInBytes[0]:X2}] [{buttonInBytes[1]:X2}] (偏移量: {orderBaseOffset + 3120})");
                        for (int i = 0; i <= 7; i++)
                        {
                            order.Button_In[i] = (buttonInBytes[0] & (1 << i)) != 0;
                            if (order.Button_In[i])
                            {
                                _logger.LogDebug($"[工单{orderIndex}] Button_In[{i}] = true (位{i})");
                            }
                        }
                        for (int i = 8; i <= 10; i++)
                        {
                            order.Button_In[i] = (buttonInBytes[1] & (1 << (i - 8))) != 0;
                            if (order.Button_In[i])
                            {
                                _logger.LogDebug($"[工单{orderIndex}] Button_In[{i}] = true (位{i - 8})");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[工单{orderIndex}] Button_In数组读取失败 - 字节数据为空或长度不足");
                    }

                    // 读取Button_Out数组（0-10），Bool类型，打包在字节中
                    // 偏移量：3122，Button_Out[0-7]在3122字节的0-7位，Button_Out[8-10]在3123字节的0-2位
                    _logger.LogDebug($"[工单{orderIndex}] 读取Button_Out数组 (11个Bool, 位打包)...");
                    var buttonOutBytes = await ReadBytesAsync(plc, dbNumber, orderBaseOffset + 3122, 2);
                    if (buttonOutBytes != null && buttonOutBytes.Length >= 2)
                    {
                        _logger.LogDebug($"[工单{orderIndex}] Button_Out字节数据: [{buttonOutBytes[0]:X2}] [{buttonOutBytes[1]:X2}] (偏移量: {orderBaseOffset + 3122})");
                        for (int i = 0; i <= 7; i++)
                        {
                            order.Button_Out[i] = (buttonOutBytes[0] & (1 << i)) != 0;
                            if (order.Button_Out[i])
                            {
                                _logger.LogDebug($"[工单{orderIndex}] Button_Out[{i}] = true (位{i})");
                            }
                        }
                        for (int i = 8; i <= 10; i++)
                        {
                            order.Button_Out[i] = (buttonOutBytes[1] & (1 << (i - 8))) != 0;
                            if (order.Button_Out[i])
                            {
                                _logger.LogDebug($"[工单{orderIndex}] Button_Out[{i}] = true (位{i - 8})");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[工单{orderIndex}] Button_Out数组读取失败 - 字节数据为空或长度不足");
                    }

                    // 读取Maximum_HeartRate数组（0-10），每个Int占2字节，偏移量：3124
                    _logger.LogDebug($"[工单{orderIndex}] 读取Maximum_HeartRate数组 (11个Int, 每个2字节)...");
                    for (int i = 0; i <= 10; i++)
                    {
                        int maxHrOffset = orderBaseOffset + 3124 + i * 2;
                        order.Maximum_HeartRate[i] = await ReadIntAsync(plc, dbNumber, maxHrOffset) ?? 0;
                        if (order.Maximum_HeartRate[i] > 0)
                        {
                            _logger.LogDebug($"[工单{orderIndex}] Maximum_HeartRate[{i}] = {order.Maximum_HeartRate[i]} (偏移量: {maxHrOffset})");
                        }
                    }

                    // 读取MInimum_HeartRate数组（0-10），每个Int占2字节，偏移量：3146
                    _logger.LogDebug($"[工单{orderIndex}] 读取MInimum_HeartRate数组 (11个Int, 每个2字节)...");
                    for (int i = 0; i <= 10; i++)
                    {
                        int minHrOffset = orderBaseOffset + 3146 + i * 2;
                        order.MInimum_HeartRate[i] = await ReadIntAsync(plc, dbNumber, minHrOffset) ?? 0;
                        if (order.MInimum_HeartRate[i] > 0)
                        {
                            _logger.LogDebug($"[工单{orderIndex}] MInimum_HeartRate[{i}] = {order.MInimum_HeartRate[i]} (偏移量: {minHrOffset})");
                        }
                    }

                    // 读取Heart_Rate数组（0-10），每个Int占2字节，偏移量：3168
                    _logger.LogDebug($"[工单{orderIndex}] 读取Heart_Rate数组 (11个Int, 每个2字节)...");
                    for (int i = 0; i <= 10; i++)
                    {
                        int hrOffset = orderBaseOffset + 3168 + i * 2;
                        order.Heart_Rate[i] = await ReadIntAsync(plc, dbNumber, hrOffset) ?? 0;
                        if (order.Heart_Rate[i] > 0)
                        {
                            _logger.LogDebug($"[工单{orderIndex}] Heart_Rate[{i}] = {order.Heart_Rate[i]} (偏移量: {hrOffset})");
                        }
                    }

                    var orderElapsed = (DateTime.Now - orderStartTime).TotalMilliseconds;
                    _logger.LogDebug($"[工单{orderIndex}] 工单数据读取完成 - 耗时: {orderElapsed:F2}ms");
                }

                // ============================================
                // 读取全局控制按钮
                // ============================================
                _logger.LogDebug("[全局控制] 读取全局控制按钮...");
                // 读取ConstructionOrder_Start_PB (Bool)，偏移量：35090，位0
                data.ConstructionOrder_Start_PB = await ReadBoolAsync(plc, dbNumber, 35090, 0) ?? false;
                _logger.LogDebug($"[全局控制] ConstructionOrder_Start_PB = {data.ConstructionOrder_Start_PB} (偏移量: 35090, 位0)");

                // 读取ConstructionOrder_Stop_PB (Bool)，偏移量：35090，位1
                data.ConstructionOrder_Stop_PB = await ReadBoolAsync(plc, dbNumber, 35090, 1) ?? false;
                _logger.LogDebug($"[全局控制] ConstructionOrder_Stop_PB = {data.ConstructionOrder_Stop_PB} (偏移量: 35090, 位1)");

                // ============================================
                // 读取空工单（Construction_Order_Null），偏移量：35092
                // ============================================
                _logger.LogDebug("[空工单] 开始读取空工单数据 (Construction_Order_Null)...");
                int nullOrderOffset = 35092;
                var nullOrder = data.Construction_Order_Null;

                // 读取Workers_Name数组（0-10），每个String占256字节
                for (int i = 0; i <= 10; i++)
                {
                    int nameOffset = nullOrderOffset + i * 256;
                    nullOrder.Workers_Name[i] = await ReadStringAsync(plc, dbNumber, nameOffset, 256) ?? string.Empty;
                }

                // 读取Construction_Order_No (Int，2字节)，偏移量：35092 + 2816 = 37908
                nullOrder.Construction_Order_No = await ReadIntAsync(plc, dbNumber, nullOrderOffset + 2816) ?? 0;

                // 读取SmartBand_No数组（0-10），每个Int占2字节，偏移量：35092 + 2818 = 37910
                for (int i = 0; i <= 10; i++)
                {
                    int bandOffset = nullOrderOffset + 2818 + i * 2;
                    nullOrder.SmartBand_No[i] = await ReadIntAsync(plc, dbNumber, bandOffset) ?? 0;
                }

                // 读取Construction_Order_Content (String，256字节)，偏移量：35092 + 2840 = 37932
                nullOrder.Construction_Order_Content = await ReadStringAsync(plc, dbNumber, nullOrderOffset + 2840, 256) ?? string.Empty;

                // 读取Workers_Status数组（0-10），每个Int占2字节，偏移量：35092 + 3096 = 38188
                for (int i = 0; i <= 10; i++)
                {
                    int statusOffset = nullOrderOffset + 3096 + i * 2;
                    nullOrder.Workers_Status[i] = await ReadIntAsync(plc, dbNumber, statusOffset) ?? 0;
                }

                // 读取Construction_Status (Int，2字节)，偏移量：35092 + 3118 = 38210
                nullOrder.Construction_Status = await ReadIntAsync(plc, dbNumber, nullOrderOffset + 3118) ?? 0;

                // 读取Button_In数组（0-10），Bool类型，偏移量：35092 + 3120 = 38212
                var nullButtonInBytes = await ReadBytesAsync(plc, dbNumber, nullOrderOffset + 3120, 2);
                if (nullButtonInBytes != null && nullButtonInBytes.Length >= 2)
                {
                    for (int i = 0; i <= 7; i++)
                    {
                        nullOrder.Button_In[i] = (nullButtonInBytes[0] & (1 << i)) != 0;
                    }
                    for (int i = 8; i <= 10; i++)
                    {
                        nullOrder.Button_In[i] = (nullButtonInBytes[1] & (1 << (i - 8))) != 0;
                    }
                }

                // 读取Button_Out数组（0-10），Bool类型，偏移量：35092 + 3122 = 38214
                var nullButtonOutBytes = await ReadBytesAsync(plc, dbNumber, nullOrderOffset + 3122, 2);
                if (nullButtonOutBytes != null && nullButtonOutBytes.Length >= 2)
                {
                    for (int i = 0; i <= 7; i++)
                    {
                        nullOrder.Button_Out[i] = (nullButtonOutBytes[0] & (1 << i)) != 0;
                    }
                    for (int i = 8; i <= 10; i++)
                    {
                        nullOrder.Button_Out[i] = (nullButtonOutBytes[1] & (1 << (i - 8))) != 0;
                    }
                }

                // 读取Maximum_HeartRate数组（0-10），每个Int占2字节，偏移量：35092 + 3124 = 38216
                for (int i = 0; i <= 10; i++)
                {
                    int maxHrOffset = nullOrderOffset + 3124 + i * 2;
                    nullOrder.Maximum_HeartRate[i] = await ReadIntAsync(plc, dbNumber, maxHrOffset) ?? 0;
                }

                // 读取MInimum_HeartRate数组（0-10），每个Int占2字节，偏移量：35092 + 3146 = 38238
                for (int i = 0; i <= 10; i++)
                {
                    int minHrOffset = nullOrderOffset + 3146 + i * 2;
                    nullOrder.MInimum_HeartRate[i] = await ReadIntAsync(plc, dbNumber, minHrOffset) ?? 0;
                }

                // 读取Heart_Rate数组（0-10），每个Int占2字节，偏移量：35092 + 3168 = 38260
                for (int i = 0; i <= 10; i++)
                {
                    int hrOffset = nullOrderOffset + 3168 + i * 2;
                    nullOrder.Heart_Rate[i] = await ReadIntAsync(plc, dbNumber, hrOffset) ?? 0;
                }

                // ============================================
                // 读取全局状态和传感器数据
                // ============================================
                _logger.LogDebug("[传感器] 开始读取传感器数据...");
                // 读取Hazardous_Gas_Alarm_Online (Bool)，偏移量：38282，位0
                data.Hazardous_Gas_Alarm_Online = await ReadBoolAsync(plc, dbNumber, 38282, 0) ?? false;
                _logger.LogDebug($"[传感器] Hazardous_Gas_Alarm_Online = {data.Hazardous_Gas_Alarm_Online} (偏移量: 38282, 位0)");

                // 读取GPS_online (Bool)，偏移量：38282，位1
                data.GPS_online = await ReadBoolAsync(plc, dbNumber, 38282, 1) ?? false;
                _logger.LogDebug($"[传感器] GPS_online = {data.GPS_online} (偏移量: 38282, 位1)");

                // 读取Gas_Alarm数组（0-10），每个Real占4字节，偏移量：38284
                _logger.LogDebug("[传感器] 读取Gas_Alarm数组 (11个Real, 每个4字节)...");
                for (int i = 0; i <= 10; i++)
                {
                    int gasOffset = 38284 + i * 4;
                    data.Gas_Alarm[i] = await ReadRealAsync(plc, dbNumber, gasOffset) ?? 0f;
                    if (data.Gas_Alarm[i] > 0)
                    {
                        _logger.LogDebug($"[传感器] Gas_Alarm[{i}] = {data.Gas_Alarm[i]:F2} (偏移量: {gasOffset})");
                    }
                }

                // 读取GPS_lon (Real，4字节)，偏移量：38328
                data.GPS_lon = await ReadRealAsync(plc, dbNumber, 38328) ?? 0f;
                _logger.LogDebug($"[传感器] GPS_lon = {data.GPS_lon:F6} (偏移量: 38328)");

                // 读取GPS_lat (Real，4字节)，偏移量：38332
                data.GPS_lat = await ReadRealAsync(plc, dbNumber, 38332) ?? 0f;
                _logger.LogDebug($"[传感器] GPS_lat = {data.GPS_lat:F6} (偏移量: 38332)");

                var totalElapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogInformation($"[数据读取成功] 成功读取S7数据 - DeviceId: {deviceId}, IP: {data.DeviceIP}, 总耗时: {totalElapsed:F2}ms");
                
                // 输出数据读取摘要
                _logger.LogInformation($"[数据读取成功] ========== 数据读取摘要 ==========");
                _logger.LogInformation($"[数据读取成功] 设备信息 - DeviceId: {data.DeviceId}, IP: {data.DeviceIP}, 采集时间: {data.CollectTime:yyyy-MM-dd HH:mm:ss.fff}");
                _logger.LogInformation($"[数据读取成功] 全局控制 - 开始按钮: {data.ConstructionOrder_Start_PB}, 结束按钮: {data.ConstructionOrder_Stop_PB}");
                
                // 统计工单数据
                int activeOrderCount = 0;
                int totalWorkers = 0;
                int activeWorkers = 0;
                for (int i = 0; i < 11; i++)
                {
                    var order = data.Construction_Order[i];
                    if (order.Construction_Order_No > 0)
                    {
                        activeOrderCount++;
                        _logger.LogInformation($"[数据读取成功] 工单[{i}] - 工单号: {order.Construction_Order_No}, 状态: {order.Construction_Status}, 内容: '{order.Construction_Order_Content}'");
                        
                        // 统计工人数据
                        for (int j = 0; j <= 10; j++)
                        {
                            if (!string.IsNullOrEmpty(order.Workers_Name[j]))
                            {
                                totalWorkers++;
                                if (order.Workers_Status[j] > 0)
                                {
                                    activeWorkers++;
                                }
                                _logger.LogInformation($"[数据读取成功]   工人[{j}] - 姓名: '{order.Workers_Name[j]}', 手环号: {order.SmartBand_No[j]}, 状态: {order.Workers_Status[j]}, 心率: {order.Heart_Rate[j]}");
                            }
                        }
                    }
                }
                _logger.LogInformation($"[数据读取成功] 工单统计 - 活跃工单数: {activeOrderCount}/11, 总工人数: {totalWorkers}, 活跃工人数: {activeWorkers}");
                
                // 传感器数据
                int gasAlarmCount = data.Gas_Alarm.Count(g => g > 0);
                _logger.LogInformation($"[数据读取成功] 传感器状态 - 有害气体报警器在线: {data.Hazardous_Gas_Alarm_Online}, GPS在线: {data.GPS_online}");
                if (gasAlarmCount > 0)
                {
                    _logger.LogInformation($"[数据读取成功] 气体报警数据 - 非零数据: {gasAlarmCount}个");
                    for (int i = 0; i <= 10; i++)
                    {
                        if (data.Gas_Alarm[i] > 0)
                        {
                            _logger.LogInformation($"[数据读取成功]   Gas_Alarm[{i}] = {data.Gas_Alarm[i]:F2}");
                        }
                    }
                }
                if (data.GPS_lon != 0 || data.GPS_lat != 0)
                {
                    _logger.LogInformation($"[数据读取成功] GPS位置 - 经度: {data.GPS_lon:F6}, 纬度: {data.GPS_lat:F6}");
                }
                
                _logger.LogInformation($"[数据读取成功] ========================================");
                _logger.LogDebug($"[数据读取成功] 数据统计 - 工单数: 11, 空工单: 1, 传感器数据: Gas({gasAlarmCount}个非零), GPS: ({data.GPS_lon:F6}, {data.GPS_lat:F6})");
            }
            catch (Exception ex)
            {
                var totalElapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogError(ex, $"[数据读取异常] 读取S7数据时发生异常 - DeviceId: {deviceId}, 耗时: {totalElapsed:F2}ms");
                _logger.LogDebug($"[数据读取异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}, 堆栈: {ex.StackTrace}");
                
                // 检查是否是地址超出范围错误
                if (ex.Message.Contains("Address out of range") || ex.Message.Contains("地址超出范围"))
                {
                    _logger.LogError($"[数据读取异常] 地址超出范围错误！");
                    _logger.LogError($"[数据读取异常] 可能的原因：");
                    _logger.LogError($"[数据读取异常] 1. DB{dbNumber}数据块大小不足（至少需要38336字节）");
                    _logger.LogError($"[数据读取异常] 2. 数据块编号配置错误（当前配置: DB{dbNumber}）");
                    _logger.LogError($"[数据读取异常] 3. S7-1500数据块属性未设置为'非优化访问'");
                    _logger.LogError($"[数据读取异常] 解决方案：");
                    _logger.LogError($"[数据读取异常] - 在TIA Portal中检查DB{dbNumber}的大小");
                    _logger.LogError($"[数据读取异常] - 将DB{dbNumber}大小设置为至少40000字节（建议50000字节）");
                    _logger.LogError($"[数据读取异常] - 如果使用S7-1500，右键DB{dbNumber} -> 属性 -> 取消'优化的块访问'");
                    _logger.LogError($"[数据读取异常] - 或者修改appsettings.json中的DataBlockNumber为正确的数据块编号");
                }
                
                data.IsConnected = false;
            }

            return data;
        }

        /// <summary>
        /// 读取字节数组
        /// </summary>
        private async Task<byte[]?> ReadBytesAsync(Plc plc, int dbNumber, int startByte, int length)
        {
            try
            {
                _logger.LogTrace($"[字节读取] 读取字节数组 - DB{dbNumber}.DBB{startByte}, 长度: {length}");
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, length));
                if (bytes != null)
                {
                    _logger.LogTrace($"[字节读取] 读取成功 - DB{dbNumber}.DBB{startByte}, 实际长度: {bytes.Length}, 数据: [{string.Join(" ", bytes.Select(b => b.ToString("X2")))}]");
                }
                else
                {
                    _logger.LogWarning($"[字节读取] 读取结果为空 - DB{dbNumber}.DBB{startByte}, 长度: {length}");
                }
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[字节读取异常] 读取字节数组时发生异常 - DB{dbNumber}.DBB{startByte}, 长度: {length}");
                _logger.LogDebug($"[字节读取异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 读取单个Real值
        /// </summary>
        private async Task<float?> ReadRealAsync(Plc plc, int dbNumber, int startByte)
        {
            try
            {
                _logger.LogTrace($"[Real读取] 读取Real值 - DB{dbNumber}.DBD{startByte}");
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 4));
                if (bytes != null && bytes.Length == 4)
                {
                    // S7 PLC使用大端序，需要反转字节数组
                    var reversedBytes = bytes.Reverse().ToArray();
                    var value = BitConverter.ToSingle(reversedBytes, 0);
                    _logger.LogTrace($"[Real读取] 读取成功 - DB{dbNumber}.DBD{startByte}, 值: {value:F6}, 原始字节: [{string.Join(" ", bytes.Select(b => b.ToString("X2")))}]");
                    return value;
                }
                _logger.LogWarning($"[Real读取] 读取失败 - DB{dbNumber}.DBD{startByte}, 字节长度: {bytes?.Length ?? 0}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Real读取异常] 读取Real值时发生异常 - DB{dbNumber}.DBD{startByte}");
                _logger.LogDebug($"[Real读取异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 读取单个Int值
        /// </summary>
        private async Task<short?> ReadIntAsync(Plc plc, int dbNumber, int startByte)
        {
            try
            {
                _logger.LogTrace($"[Int读取] 读取Int值 - DB{dbNumber}.DBW{startByte}");
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 2));
                if (bytes != null && bytes.Length == 2)
                {
                    // S7 PLC使用大端序
                    var value = (short)(bytes[0] << 8 | bytes[1]);
                    _logger.LogTrace($"[Int读取] 读取成功 - DB{dbNumber}.DBW{startByte}, 值: {value}, 原始字节: [{bytes[0]:X2}] [{bytes[1]:X2}]");
                    return value;
                }
                _logger.LogWarning($"[Int读取] 读取失败 - DB{dbNumber}.DBW{startByte}, 字节长度: {bytes?.Length ?? 0}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Int读取异常] 读取Int值时发生异常 - DB{dbNumber}.DBW{startByte}");
                _logger.LogDebug($"[Int读取异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 读取单个Bool值
        /// </summary>
        private async Task<bool?> ReadBoolAsync(Plc plc, int dbNumber, int startByte, int bit = 0)
        {
            try
            {
                _logger.LogTrace($"[Bool读取] 读取Bool值 - DB{dbNumber}.DBX{startByte}.{bit}");
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 1));
                if (bytes != null && bytes.Length == 1)
                {
                    var value = (bytes[0] & (1 << bit)) != 0;
                    _logger.LogTrace($"[Bool读取] 读取成功 - DB{dbNumber}.DBX{startByte}.{bit}, 值: {value}, 原始字节: [{bytes[0]:X2}] (位{bit})");
                    return value;
                }
                _logger.LogWarning($"[Bool读取] 读取失败 - DB{dbNumber}.DBX{startByte}.{bit}, 字节长度: {bytes?.Length ?? 0}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Bool读取异常] 读取Bool值时发生异常 - DB{dbNumber}.DBX{startByte}.{bit}");
                _logger.LogDebug($"[Bool读取异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        private async Task<string?> ReadStringAsync(Plc plc, int dbNumber, int startByte, int length)
        {
            try
            {
                _logger.LogTrace($"[String读取] 读取字符串 - DB{dbNumber}.DBD{startByte}, 长度: {length}");
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, length));
                if (bytes != null && bytes.Length > 0)
                {
                    // 使用GBK编码（双字节中文编码）
                    try
                    {
                        // 注册GBK编码提供程序
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        var gbkEncoding = Encoding.GetEncoding("GBK");
                        
                        // 跳过无效的字节，从第一个有效的中文字符开始
                        var validBytes = SkipInvalidBytes(bytes);
                        var result = gbkEncoding.GetString(validBytes).TrimEnd('\0');
                        
                        if (!string.IsNullOrEmpty(result))
                        {
                            _logger.LogTrace($"[String读取] 读取成功(GBK) - DB{dbNumber}.DBD{startByte}, 值: '{result}', 原始长度: {bytes.Length}, 有效长度: {validBytes.Length}");
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"[String读取] GBK编码解析失败，使用ASCII编码 - DB{dbNumber}.DBD{startByte}");
                        // 如果GBK编码失败，尝试使用ASCII编码
                        var asciiResult = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                        if (!string.IsNullOrEmpty(asciiResult))
                        {
                            _logger.LogTrace($"[String读取] 读取成功(ASCII) - DB{dbNumber}.DBD{startByte}, 值: '{asciiResult}'");
                        }
                        return asciiResult;
                    }
                }
                _logger.LogWarning($"[String读取] 读取失败 - DB{dbNumber}.DBD{startByte}, 字节长度: {bytes?.Length ?? 0}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[String读取异常] 读取字符串时发生异常 - DB{dbNumber}.DBD{startByte}, 长度: {length}");
                _logger.LogDebug($"[String读取异常] 异常详情 - 类型: {ex.GetType().Name}, 消息: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 跳过无效字节，找到第一个有效的中文字符起始位置
        /// </summary>
        private byte[] SkipInvalidBytes(byte[] bytes)
        {
            // 在GBK编码中，中文字符的第一个字节范围通常是0x81-0xFE
            // 英文字符是0x20-0x7E
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                // 检查是否是有效的中文字符起始字节
                if (bytes[i] >= 0x81 && bytes[i] <= 0xFE)
                {
                    // 检查下一个字节是否也是有效的GBK字节
                    if (bytes[i + 1] >= 0x40 && bytes[i + 1] <= 0xFE)
                    {
                        // 找到有效的中文字符起始位置，返回从该位置开始的字节数组
                        var result = new byte[bytes.Length - i];
                        Array.Copy(bytes, i, result, 0, bytes.Length - i);
                        return result;
                    }
                }
            }
            
            // 如果没有找到有效的中文字符，返回原数组
            return bytes;
        }
    }
}

