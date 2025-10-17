using Microsoft.AspNetCore.Mvc;
using S7Demo.Models;
using S7Demo.Services;

namespace S7Demo.Controllers
{
    /// <summary>
    /// S7 PLC数据API控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PlcController : ControllerBase
    {
        private readonly S7PlcService _plcService;
        private readonly ILogger<PlcController> _logger;

        public PlcController(S7PlcService plcService, ILogger<PlcController> logger)
        {
            _plcService = plcService;
            _logger = logger;
        }

        /// <summary>
        /// 获取GPS数据
        /// </summary>
        /// <returns>GPS坐标和设备信息</returns>
        [HttpGet("gps")]
        public async Task<ActionResult<ApiResponse<GpsData>>> GetGpsData()
        {
            try
            {
                _logger.LogInformation("开始读取GPS数据");
                
                var gpsData = await _plcService.ReadGpsDataAsync();
                
                if (gpsData != null)
                {
                    return Ok(new ApiResponse<GpsData>
                    {
                        Success = true,
                        Message = "GPS数据读取成功",
                        Data = gpsData
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<GpsData>
                    {
                        Success = false,
                        Message = "GPS数据读取失败",
                        Error = "无法从PLC读取数据"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取GPS数据时发生异常");
                return StatusCode(500, new ApiResponse<GpsData>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 检查PLC连接状态
        /// </summary>
        /// <returns>连接状态</returns>
        [HttpGet("status")]
        public ActionResult<ApiResponse<bool>> GetConnectionStatus()
        {
            try
            {
                var isConnected = _plcService.IsConnected;
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = isConnected ? "PLC已连接" : "PLC未连接",
                    Data = isConnected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查连接状态时发生异常");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 连接到PLC
        /// </summary>
        /// <returns>连接结果</returns>
        [HttpPost("connect")]
        public async Task<ActionResult<ApiResponse<bool>>> Connect()
        {
            try
            {
                _logger.LogInformation("尝试连接PLC");
                
                var connected = await _plcService.ConnectAsync();
                
                return Ok(new ApiResponse<bool>
                {
                    Success = connected,
                    Message = connected ? "PLC连接成功" : "PLC连接失败",
                    Data = connected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接PLC时发生异常");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        /// <returns>断开结果</returns>
        [HttpPost("disconnect")]
        public async Task<ActionResult<ApiResponse<bool>>> Disconnect()
        {
            try
            {
                _logger.LogInformation("断开PLC连接");
                
                await _plcService.DisconnectAsync();
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "PLC连接已断开",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开PLC连接时发生异常");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 读取Real值
        /// </summary>
        /// <param name="dbNumber">数据块编号</param>
        /// <param name="startByte">起始字节</param>
        /// <returns>Real值</returns>
        [HttpGet("read/real/{dbNumber}/{startByte}")]
        public async Task<ActionResult<ApiResponse<float?>>> ReadReal(int dbNumber, int startByte)
        {
            try
            {
                _logger.LogInformation($"读取Real值: DB{dbNumber}.DBD{startByte}");
                
                var value = await _plcService.ReadRealAsync(dbNumber, startByte);
                
                return Ok(new ApiResponse<float?>
                {
                    Success = value.HasValue,
                    Message = value.HasValue ? "读取成功" : "读取失败",
                    Data = value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取Real值时发生异常: DB{dbNumber}.DBD{startByte}");
                return StatusCode(500, new ApiResponse<float?>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 读取Int值
        /// </summary>
        /// <param name="dbNumber">数据块编号</param>
        /// <param name="startByte">起始字节</param>
        /// <returns>Int值</returns>
        [HttpGet("read/int/{dbNumber}/{startByte}")]
        public async Task<ActionResult<ApiResponse<short?>>> ReadInt(int dbNumber, int startByte)
        {
            try
            {
                _logger.LogInformation($"读取Int值: DB{dbNumber}.DBW{startByte}");
                
                var value = await _plcService.ReadIntAsync(dbNumber, startByte);
                
                return Ok(new ApiResponse<short?>
                {
                    Success = value.HasValue,
                    Message = value.HasValue ? "读取成功" : "读取失败",
                    Data = value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取Int值时发生异常: DB{dbNumber}.DBW{startByte}");
                return StatusCode(500, new ApiResponse<short?>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// 写入Real值
        /// </summary>
        /// <param name="dbNumber">数据块编号</param>
        /// <param name="startByte">起始字节</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        [HttpPost("write/real/{dbNumber}/{startByte}")]
        public async Task<ActionResult<ApiResponse<bool>>> WriteReal(int dbNumber, int startByte, [FromBody] float value)
        {
            try
            {
                _logger.LogInformation($"写入Real值: DB{dbNumber}.DBD{startByte} = {value}");
                
                var success = await _plcService.WriteRealAsync(dbNumber, startByte, value);
                
                return Ok(new ApiResponse<bool>
                {
                    Success = success,
                    Message = success ? "写入成功" : "写入失败",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"写入Real值时发生异常: DB{dbNumber}.DBD{startByte}");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "服务器内部错误",
                    Error = ex.Message
                });
            }
        }
    }
}
