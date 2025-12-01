using Microsoft.AspNetCore.Mvc;
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


