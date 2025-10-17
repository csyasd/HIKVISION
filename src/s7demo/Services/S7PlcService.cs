using S7.Net;
using S7Demo.Models;

namespace S7Demo.Services
{
    /// <summary>
    /// S7 PLC通信服务
    /// </summary>
    public class S7PlcService
    {
        private readonly Plc _plc;
        private readonly PlcConfig _config;
        private readonly ILogger<S7PlcService> _logger;

        public S7PlcService(PlcConfig config, ILogger<S7PlcService> logger)
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
                    _logger.LogInformation("PLC连接成功");
                    return true;
                }
                else
                {
                    _logger.LogError($"PLC连接失败: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接PLC时发生异常");
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
                await Task.Run(() => _plc.Close());
                _logger.LogInformation("PLC连接已断开");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开PLC连接时发生异常");
            }
        }

        /// <summary>
        /// 检查PLC连接状态
        /// </summary>
        public bool IsConnected => _plc.IsConnected;

        /// <summary>
        /// 读取GPS数据
        /// </summary>
        public async Task<GpsData?> ReadGpsDataAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    _logger.LogWarning("PLC未连接，尝试重新连接");
                    var connected = await ConnectAsync();
                    if (!connected)
                    {
                        return new GpsData
                        {
                            IsConnected = false,
                            ReadTime = DateTime.Now
                        };
                    }
                }

                var gpsData = new GpsData
                {
                    ReadTime = DateTime.Now,
                    IsConnected = true
                };

                // 读取GPS经度 (DB1.DBD0 - Real类型，4字节)
                var longitudeBytes = await Task.Run(() => _plc.ReadBytes(DataType.DataBlock, _config.DataBlockNumber, 0, 4));
                if (longitudeBytes != null && longitudeBytes.Length == 4)
                {
                    gpsData.Longitude = BitConverter.ToSingle(longitudeBytes, 0);
                }

                // 读取GPS纬度 (DB1.DBD4 - Real类型，4字节)
                var latitudeBytes = await Task.Run(() => _plc.ReadBytes(DataType.DataBlock, _config.DataBlockNumber, 4, 4));
                if (latitudeBytes != null && latitudeBytes.Length == 4)
                {
                    gpsData.Latitude = BitConverter.ToSingle(latitudeBytes, 0);
                }

                // 读取设备ID (DB1.DBW8 - Int类型，2字节)
                var deviceIdBytes = await Task.Run(() => _plc.ReadBytes(DataType.DataBlock, _config.DataBlockNumber, 8, 2));
                if (deviceIdBytes != null && deviceIdBytes.Length == 2)
                {
                    gpsData.DeviceId = BitConverter.ToInt16(deviceIdBytes, 0);
                }

                _logger.LogInformation($"成功读取GPS数据: 经度={gpsData.Longitude}, 纬度={gpsData.Latitude}, 设备ID={gpsData.DeviceId}");
                return gpsData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取GPS数据时发生异常");
                return new GpsData
                {
                    IsConnected = false,
                    ReadTime = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 读取单个Real值
        /// </summary>
        public async Task<float?> ReadRealAsync(int dbNumber, int startByte)
        {
            try
            {
                if (!IsConnected)
                {
                    await ConnectAsync();
                }

                var bytes = await Task.Run(() => _plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 4));
                if (bytes != null && bytes.Length == 4)
                {
                    return BitConverter.ToSingle(bytes, 0);
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
        public async Task<short?> ReadIntAsync(int dbNumber, int startByte)
        {
            try
            {
                if (!IsConnected)
                {
                    await ConnectAsync();
                }

                var bytes = await Task.Run(() => _plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, 2));
                if (bytes != null && bytes.Length == 2)
                {
                    return BitConverter.ToInt16(bytes, 0);
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
        /// 写入Real值
        /// </summary>
        public async Task<bool> WriteRealAsync(int dbNumber, int startByte, float value)
        {
            try
            {
                if (!IsConnected)
                {
                    await ConnectAsync();
                }

                var bytes = BitConverter.GetBytes(value);
                await Task.Run(() => _plc.WriteBytes(DataType.DataBlock, dbNumber, startByte, bytes));
                var result = ErrorCode.NoError;
                
                if (result == ErrorCode.NoError)
                {
                    _logger.LogInformation($"成功写入Real值: DB{dbNumber}.DBD{startByte} = {value}");
                    return true;
                }
                else
                {
                    _logger.LogError($"写入Real值失败: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"写入Real值时发生异常: DB{dbNumber}.DBD{startByte}");
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (IsConnected)
                {
                    _plc.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放S7PlcService资源时发生异常");
            }
        }
    }
}
