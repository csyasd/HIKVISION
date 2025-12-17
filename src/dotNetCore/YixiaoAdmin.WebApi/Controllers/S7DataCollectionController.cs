using Microsoft.AspNetCore.Mvc;
using System.Linq;
using YixiaoAdmin.Models;
using YixiaoAdmin.WebApi.Services;

namespace YixiaoAdmin.WebApi.Controllers
{
    /// <summary>
    /// S7 PLC数据采集控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class S7DataCollectionController : ControllerBase
    {
        private readonly S7DataCollectionService _dataCollectionService;

        public S7DataCollectionController(S7DataCollectionService dataCollectionService)
        {
            _dataCollectionService = dataCollectionService;
        }

        /// <summary>
        /// 获取指定设备的最新采集数据
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet("device/{deviceId}")]
        public IActionResult GetDeviceData(string deviceId)
        {
            var data = _dataCollectionService.GetDeviceData(deviceId);
            if (data == null)
            {
                return NotFound($"未找到设备 {deviceId} 的采集数据");
            }
            return Ok(data);
        }

        /// <summary>
        /// 获取指定设备的指定工单数据
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="orderIndex">工单索引（0-10）</param>
        /// <returns></returns>
        [HttpGet("device/{deviceId}/order/{orderIndex}")]
        public IActionResult GetDeviceOrderData(string deviceId, int orderIndex)
        {
            if (orderIndex < 0 || orderIndex > 10)
            {
                return BadRequest("工单索引必须在0-10之间");
            }

            var data = _dataCollectionService.GetDeviceData(deviceId);
            if (data == null)
            {
                return NotFound($"未找到设备 {deviceId} 的采集数据");
            }

            if (data.Construction_Order == null || orderIndex >= data.Construction_Order.Length)
            {
                return NotFound($"未找到工单索引 {orderIndex} 的数据");
            }

            return Ok(new
            {
                DeviceId = data.DeviceId,
                DeviceIP = data.DeviceIP,
                CollectTime = data.CollectTime,
                OrderIndex = orderIndex,
                OrderData = data.Construction_Order[orderIndex]
            });
        }

        /// <summary>
        /// 获取指定设备的所有工单数据
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet("device/{deviceId}/orders")]
        public IActionResult GetDeviceOrdersData(string deviceId)
        {
            var data = _dataCollectionService.GetDeviceData(deviceId);
            if (data == null)
            {
                return NotFound($"未找到设备 {deviceId} 的采集数据");
            }

            return Ok(new
            {
                DeviceId = data.DeviceId,
                DeviceIP = data.DeviceIP,
                CollectTime = data.CollectTime,
                Orders = data.Construction_Order,
                StartButton = data.ConstructionOrder_Start_PB,
                StopButton = data.ConstructionOrder_Stop_PB,
                NullOrder = data.Construction_Order_Null
            });
        }

        /// <summary>
        /// 获取指定设备的传感器数据（气体报警、GPS等）
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet("device/{deviceId}/sensors")]
        public IActionResult GetDeviceSensorData(string deviceId)
        {
            var data = _dataCollectionService.GetDeviceData(deviceId);
            if (data == null)
            {
                return NotFound($"未找到设备 {deviceId} 的采集数据");
            }

            return Ok(new
            {
                DeviceId = data.DeviceId,
                DeviceIP = data.DeviceIP,
                CollectTime = data.CollectTime,
                Hazardous_Gas_Alarm_Online = data.Hazardous_Gas_Alarm_Online,
                GPS_online = data.GPS_online,
                Gas_Alarm = data.Gas_Alarm,
                GPS_lon = data.GPS_lon,
                GPS_lat = data.GPS_lat
            });
        }

        /// <summary>
        /// 获取所有设备的最新采集数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public IActionResult GetAllDeviceData()
        {
            var data = _dataCollectionService.GetAllDeviceData();
            return Ok(data);
        }

        /// <summary>
        /// 获取设备连接状态
        /// </summary>
        /// <returns></returns>
        [HttpGet("status")]
        public IActionResult GetDeviceConnectionStatus()
        {
            var status = _dataCollectionService.GetDeviceConnectionStatus();
            return Ok(status);
        }

        /// <summary>
        /// 获取指定设备的数据读取详情（包含所有读取到的数据内容）
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet("device/{deviceId}/details")]
        public IActionResult GetDeviceDataDetails(string deviceId)
        {
            var data = _dataCollectionService.GetDeviceData(deviceId);
            if (data == null)
            {
                return NotFound($"未找到设备 {deviceId} 的采集数据");
            }

            // 构建详细的数据响应
            var details = new
            {
                // 基本信息
                DeviceId = data.DeviceId,
                DeviceIP = data.DeviceIP,
                CollectTime = data.CollectTime,
                IsConnected = data.IsConnected,
                
                // 全局控制
                ConstructionOrder_Start_PB = data.ConstructionOrder_Start_PB,
                ConstructionOrder_Stop_PB = data.ConstructionOrder_Stop_PB,
                
                // 工单数据（只返回有数据的工单）
                Orders = data.Construction_Order?.Select((order, index) => new
                {
                    OrderIndex = index,
                    Construction_Order_No = order.Construction_Order_No,
                    Construction_Order_Content = order.Construction_Order_Content,
                    Construction_Status = order.Construction_Status,
                    Workers = order.Workers_Name?.Select((name, i) => new
                    {
                        Index = i,
                        Name = name,
                        SmartBand_No = order.SmartBand_No?[i] ?? 0,
                        Status = order.Workers_Status?[i] ?? 0,
                        Button_In = order.Button_In?[i] ?? false,
                        Button_Out = order.Button_Out?[i] ?? false,
                        Maximum_HeartRate = order.Maximum_HeartRate?[i] ?? 0,
                        MInimum_HeartRate = order.MInimum_HeartRate?[i] ?? 0,
                        Heart_Rate = order.Heart_Rate?[i] ?? 0
                    }).Where(w => !string.IsNullOrEmpty(w.Name)).ToList(),
                    HasData = order.Construction_Order_No > 0 || 
                             order.Workers_Name?.Any(n => !string.IsNullOrEmpty(n)) == true
                }).Where(o => o.HasData).ToList(),
                
                // 空工单
                NullOrder = data.Construction_Order_Null != null ? new
                {
                    Construction_Order_No = data.Construction_Order_Null.Construction_Order_No,
                    Construction_Order_Content = data.Construction_Order_Null.Construction_Order_Content,
                    Construction_Status = data.Construction_Order_Null.Construction_Status,
                    Workers = data.Construction_Order_Null.Workers_Name?.Select((name, i) => new
                    {
                        Index = i,
                        Name = name,
                        SmartBand_No = data.Construction_Order_Null.SmartBand_No?[i] ?? 0,
                        Status = data.Construction_Order_Null.Workers_Status?[i] ?? 0
                    }).Where(w => !string.IsNullOrEmpty(w.Name)).ToList()
                } : null,
                
                // 传感器数据
                Sensors = new
                {
                    Hazardous_Gas_Alarm_Online = data.Hazardous_Gas_Alarm_Online,
                    GPS_online = data.GPS_online,
                    Gas_Alarm = data.Gas_Alarm?.Select((value, index) => new { Index = index, Value = value })
                        .Where(g => g.Value > 0).ToList(),
                    GPS_lon = data.GPS_lon,
                    GPS_lat = data.GPS_lat
                },
                
                // 数据统计
                Statistics = new
                {
                    TotalOrders = data.Construction_Order?.Length ?? 0,
                    ActiveOrders = data.Construction_Order?.Count(o => o.Construction_Order_No > 0) ?? 0,
                    TotalWorkers = data.Construction_Order?.Sum(o => o.Workers_Name?.Count(n => !string.IsNullOrEmpty(n)) ?? 0) ?? 0,
                    ActiveWorkers = data.Construction_Order?.Sum(o => o.Workers_Status?.Count(s => s > 0) ?? 0) ?? 0,
                    GasAlarmCount = data.Gas_Alarm?.Count(g => g > 0) ?? 0,
                    HasGPSData = (data.GPS_lon != 0 || data.GPS_lat != 0)
                }
            };

            return Ok(details);
        }
    }
}



