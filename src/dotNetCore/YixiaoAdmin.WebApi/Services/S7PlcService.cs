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
            try
            {
                _logger.LogInformation($"正在连接到PLC: {_config.IpAddress}");
                
                await Task.Run(() => _plc.Open());
                var result = _plc.IsConnected ? ErrorCode.NoError : ErrorCode.ConnectionError;
                
                if (result == ErrorCode.NoError)
                {
                    _logger.LogInformation($"PLC连接成功: {_config.IpAddress}");
                    return true;
                }
                else
                {
                    _logger.LogError($"PLC连接失败: {_config.IpAddress}, 错误码: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"连接PLC时发生异常: {_config.IpAddress}");
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
        /// </summary>
        public async Task<S7DataCollectionModel> ReadAllDataAsync(S7PlcConnectionService connectionService, string deviceId)
        {
            var data = new S7DataCollectionModel
            {
                DeviceId = deviceId,
                DeviceIP = connectionService.GetConfig().IpAddress,
                CollectTime = DateTime.Now,
                IsConnected = connectionService.IsConnected
            };

            if (!connectionService.IsConnected)
            {
                _logger.LogWarning($"PLC未连接，无法读取数据: {deviceId}");
                return data;
            }

            var plc = connectionService.GetPlc();
            var dbNumber = connectionService.GetConfig().DataBlockNumber;

            try
            {
                // ============================================
                // 重要提示：以下地址偏移量需要根据实际PLC配置调整！
                // 这里提供的是示例地址，实际使用时需要根据PLC程序中的实际数据块地址修改
                // 请根据PLC程序中的实际数据块布局调整以下偏移量
                // ============================================

                // 读取Device_ID (Int类型，2字节)
                // 地址：DB1.DBW0（示例，需要根据实际PLC配置调整）
                data.Device_ID = await ReadIntAsync(plc, dbNumber, 0) ?? 0;

                // 读取Construction_Order_No (Int类型，2字节)
                // 地址：DB1.DBW2（示例，需要根据实际PLC配置调整）
                data.Construction_Order_No = await ReadIntAsync(plc, dbNumber, 2) ?? 0;

                // 读取Construction_Order_Content (String类型，256字节)
                // 地址：DB1.DBD4（示例，需要根据实际PLC配置调整）
                data.Construction_Order_Content = await ReadStringAsync(plc, dbNumber, 4, 256) ?? string.Empty;

                // 读取Construction_Status (Int类型，2字节)
                // 地址：DB1.DBW260（示例，需要根据实际PLC配置调整）
                data.Construction_Status = await ReadIntAsync(plc, dbNumber, 260) ?? 0;

                // 读取Worker相关数组数据（0-10）
                // 起始偏移量，需要根据实际PLC配置调整
                // 假设每个工人数据占用50字节（包括：姓名20字节 + 手环号2字节 + 状态2字节 + 按钮2字节 + 心率4字节 + 紧急呼叫1字节 + 气体报警4字节等）
                int baseOffset = 262; // 需要根据实际PLC配置调整
                
                for (int i = 0; i <= 10; i++)
                {
                    int offset = baseOffset + i * 50; // 假设每个工人数据占用50字节

                    // 读取Workers_Name (String，每个20字节)
                    data.Workers_Name[i] = await ReadStringAsync(plc, dbNumber, offset, 20) ?? string.Empty;

                    // 读取SmartBand_No (Int，2字节)
                    data.SmartBand_No[i] = await ReadIntAsync(plc, dbNumber, offset + 20) ?? 0;

                    // 读取Workers_Status (Int，2字节)
                    data.Workers_Status[i] = await ReadIntAsync(plc, dbNumber, offset + 22) ?? 0;

                    // 读取Button_In (Bool，1字节)
                    data.Button_In[i] = await ReadBoolAsync(plc, dbNumber, offset + 24) ?? false;

                    // 读取Button_Out (Bool，1字节)
                    data.Button_Out[i] = await ReadBoolAsync(plc, dbNumber, offset + 25) ?? false;

                    // 读取Maximum_HeartRate (Int，2字节)
                    data.Maximum_HeartRate[i] = await ReadIntAsync(plc, dbNumber, offset + 26) ?? 0;

                    // 读取MInimum_HeartRate (Int，2字节)
                    data.MInimum_HeartRate[i] = await ReadIntAsync(plc, dbNumber, offset + 28) ?? 0;

                    // 读取Emergency_Call (Bool，1字节)
                    data.Emergency_Call[i] = await ReadBoolAsync(plc, dbNumber, offset + 30) ?? false;

                    // 读取Gas_Alarm (Real，4字节)
                    data.Gas_Alarm[i] = await ReadRealAsync(plc, dbNumber, offset + 32) ?? 0f;
                }

                // 读取在线状态标志（Bool类型）
                // 地址：工人数据后的偏移量（需要根据实际PLC配置调整）
                int statusOffset = baseOffset + 11 * 50; // 需要根据实际PLC配置调整
                data.Hazardous_Gas_Alarm_Online = await ReadBoolAsync(plc, dbNumber, statusOffset) ?? false;
                data.Bracelet_Scanner_WorkingArea = await ReadBoolAsync(plc, dbNumber, statusOffset + 1) ?? false;
                data.Bracelet_Scanner_Gate = await ReadBoolAsync(plc, dbNumber, statusOffset + 2) ?? false;
                data.GPS_online = await ReadBoolAsync(plc, dbNumber, statusOffset + 3) ?? false;

                // 读取GPS数据（Real类型，4字节）
                // 地址：状态标志后的偏移量（需要根据实际PLC配置调整）
                data.GPS_lon = await ReadRealAsync(plc, dbNumber, statusOffset + 4) ?? 0f;
                data.GPS_lat = await ReadRealAsync(plc, dbNumber, statusOffset + 8) ?? 0f;

                _logger.LogInformation($"成功读取S7数据: DeviceId={deviceId}, IP={data.DeviceIP}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取S7数据时发生异常: DeviceId={deviceId}");
                data.IsConnected = false;
            }

            return data;
        }

        /// <summary>
        /// 读取单个Real值
        /// </summary>
        private async Task<float?> ReadRealAsync(Plc plc, int dbNumber, int startByte)
        {
            try
            {
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 4));
                if (bytes != null && bytes.Length == 4)
                {
                    // S7 PLC使用大端序，需要反转字节数组
                    var reversedBytes = bytes.Reverse().ToArray();
                    return BitConverter.ToSingle(reversedBytes, 0);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取Real值时发生异常: DB{dbNumber}.DBD{startByte}");
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
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 2));
                if (bytes != null && bytes.Length == 2)
                {
                    // S7 PLC使用大端序
                    return (short)(bytes[0] << 8 | bytes[1]);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取Int值时发生异常: DB{dbNumber}.DBW{startByte}");
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
                var bytes = await Task.Run(() => plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 1));
                if (bytes != null && bytes.Length == 1)
                {
                    return (bytes[0] & (1 << bit)) != 0;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取Bool值时发生异常: DB{dbNumber}.DBX{startByte}.{bit}");
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
                        
                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"GBK编码解析失败，使用ASCII编码");
                        // 如果GBK编码失败，尝试使用ASCII编码
                        return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取字符串时发生异常: DB{dbNumber}.DBD{startByte}, 长度={length}");
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

