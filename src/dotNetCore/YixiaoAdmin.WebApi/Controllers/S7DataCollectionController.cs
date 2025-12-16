using Microsoft.AspNetCore.Mvc;
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
    }
}



