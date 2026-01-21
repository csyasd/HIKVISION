using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;
using YixiaoAdmin.Common;

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserDeviceController : ControllerBase
    {
        private readonly IUserDeviceServices _UserDeviceServices;

        public UserDeviceController(IUserDeviceServices UserDeviceServices)
        {
            _UserDeviceServices = UserDeviceServices ?? 
                                       throw new ArgumentNullException(nameof(UserDeviceServices));
        }

        /// <summary>
        /// 根据用户ID获取设备ID列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult<List<string>>> GetDeviceIdsByUserId(string userId)
        {
            return Ok(await _UserDeviceServices.GetDeviceIdsByUserId(userId));
        }

        /// <summary>
        /// 根据设备ID获取用户ID列表
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult<List<string>>> GetUserIdsByDeviceId(string deviceId)
        {
            return Ok(await _UserDeviceServices.GetUserIdsByDeviceId(deviceId));
        }

        /// <summary>
        /// 批量添加用户设备关联
        /// </summary>
        /// <param name="request">请求模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<bool>> AddUserDevices([FromBody] AddUserDevicesRequest request)
        {
            return Ok(await _UserDeviceServices.AddUserDevices(request.DeviceId, request.UserIds));
        }
    }

    public class AddUserDevicesRequest
    {
        public string DeviceId { get; set; }
        public List<string> UserIds { get; set; }
    }
}

