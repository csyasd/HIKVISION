
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
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

//这是 Device 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceServices _DeviceServices;
        private readonly IUserDeviceServices _UserDeviceServices;
        private readonly IUserServices _UserServices;
        private readonly IRoleServices _RoleServices;

        public DeviceController(IDeviceServices DeviceServices, IUserDeviceServices UserDeviceServices, IUserServices UserServices, IRoleServices RoleServices)
        {
            _DeviceServices = DeviceServices ?? 
                                       throw new ArgumentNullException(nameof(DeviceServices));
            _UserDeviceServices = UserDeviceServices ?? 
                                       throw new ArgumentNullException(nameof(UserDeviceServices));
            _UserServices = UserServices ?? 
                                       throw new ArgumentNullException(nameof(UserServices));
            _RoleServices = RoleServices ?? 
                                       throw new ArgumentNullException(nameof(RoleServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<Device>> All()
        {
            // 获取当前用户绑定的设备ID列表
            var deviceIds = await GetUserBoundDeviceIds();
            
            if (deviceIds == null)
            {
                // 管理员，返回所有设备
                return await _DeviceServices.Query();
            }
            
            if (!deviceIds.Any())
            {
                // 普通用户但没有绑定设备，返回空列表
                return new List<Device>();
            }
            
            // 普通用户，只返回绑定的设备
            var allDevices = await _DeviceServices.Query();
            return allDevices.Where(d => deviceIds.Contains(d.Id)).ToList();
        }
        
        /// <summary>
        /// 获取当前用户绑定的设备ID列表（如果是管理员返回null，表示返回所有设备）
        /// </summary>
        /// <returns>管理员返回null，普通用户返回设备ID列表</returns>
        private async Task<List<string>> GetUserBoundDeviceIds()
        {
            // 获取当前用户ID（从JWT token的Jti claim中获取）
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return new List<string>(); // 没有用户ID，返回空列表
            }
            
            try
            {
                // 查询用户信息以获取角色信息
                var userQuery = await _UserServices.Query(u => u.Id == userId);
                var user = userQuery.FirstOrDefault();
                
                if (user != null)
                {
                    // 如果Role为null，尝试通过RoleId查询Role
                    if (user.Role == null && !string.IsNullOrEmpty(user.RoleId))
                    {
                        var role = await _RoleServices.QueryById(user.RoleId);
                        if (role != null)
                        {
                            user.Role = role;
                        }
                    }
                    
                    if (user.Role != null)
                    {
                        // 检查是否是管理员
                        bool isAdmin = string.Equals(user.Role.Code, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(user.Role.Name, "管理员", StringComparison.OrdinalIgnoreCase) ||
                                      (user.Role.Name?.Contains("管理") == true) ||
                                      (user.Role.Name?.ToLower().Contains("admin") == true);
                        
                        if (isAdmin)
                        {
                            return null; // 管理员，返回null表示返回所有设备
                        }
                        
                        // 普通用户，获取绑定的设备ID列表
                        return await _UserDeviceServices.GetDeviceIdsByUserId(userId);
                    }
                }
            }
            catch
            {
                // 查询失败，返回空列表（安全策略）
            }
            
            return new List<string>(); // 默认返回空列表
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<Device>>> Pages(QueryPageModel queryPageModel)
        {
            // 获取当前用户ID（从JWT token的Jti claim中获取）
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            
            // 判断是否是管理员
            bool isAdmin = false;
            List<string> deviceIds = null;

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // 查询用户信息以获取角色信息（使用Query方法，需要手动Include Role）
                    // 注意：Query方法返回的是IQueryable，需要手动Include
                    var userQuery = await _UserServices.Query(u => u.Id == userId);
                    var user = userQuery.FirstOrDefault();
                    
                    // 如果用户存在但Role为null，尝试通过RoleId查询Role
                    if (user != null && user.Role == null && !string.IsNullOrEmpty(user.RoleId))
                    {
                        var role = await _RoleServices.QueryById(user.RoleId);
                        if (role != null)
                        {
                            user.Role = role;
                        }
                    }
                    
                    if (user != null && user.Role != null)
                    {
                        // 检查角色Code是否为"Admin"（不区分大小写）
                        // 或者角色名称包含"管理"或"admin"
                        isAdmin = string.Equals(user.Role.Code, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(user.Role.Name, "管理员", StringComparison.OrdinalIgnoreCase) ||
                                  (user.Role.Name?.Contains("管理") == true) ||
                                  (user.Role.Name?.ToLower().Contains("admin") == true);
                        
                        // 如果不是管理员，获取用户绑定的设备ID列表
                        if (!isAdmin)
                        {
                            deviceIds = await _UserDeviceServices.GetDeviceIdsByUserId(userId);
                        }
                    }
                    else
                    {
                        // 如果用户或角色为空，返回空列表（安全策略）
                        isAdmin = false;
                        deviceIds = new List<string>();
                    }
                }
                catch (Exception ex)
                {
                    // 如果查询用户失败，记录错误并返回空列表（安全策略）
                    System.Diagnostics.Debug.WriteLine($"[DeviceController] Error querying user: {ex.Message}");
                    isAdmin = false;
                    deviceIds = new List<string>();
                }
            }
            else
            {
                // 如果没有用户ID，返回空列表（安全起见）
                isAdmin = false;
                deviceIds = new List<string>();
            }

            // 如果是管理员，返回所有设备（不添加设备ID过滤）
            if (isAdmin)
            {
                var result = await _DeviceServices.QueryPagesExpand(queryPageModel);
                return Ok(result);
            }

            // 如果是普通用户，只返回绑定的设备
            if (string.IsNullOrEmpty(userId))
            {
                // 如果没有用户ID，返回空列表
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<Device>(), 0);
                return Ok(emptyResponse);
            }

            if (deviceIds == null || !deviceIds.Any())
            {
                // 如果没有绑定的设备，返回空列表
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<Device>(), 0);
                return Ok(emptyResponse);
            }

            // 在查询条件中添加设备ID过滤（在数据库查询时过滤，而不是在内存中）
            var queryList = queryPageModel.Query?.ToList() ?? new List<QueryFieldModel>();

            // 添加设备ID过滤条件（使用逗号分隔的ID列表）
            queryList.Add(new QueryFieldModel 
            { 
                QueryField = "Id", 
                QueryStr = string.Join(",", deviceIds) 
            });

            queryPageModel.Query = queryList.ToArray();

            // 调用查询方法，设备ID过滤会在数据库查询时生效
            return Ok(await _DeviceServices.QueryPagesExpand(queryPageModel));
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Device> Get(string Id)
        {
            return await _DeviceServices.QueryById(Id);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(Device viewModel)
        {

            return await _DeviceServices.Add(viewModel);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(Device viewModel)
        {
            return await _DeviceServices.Update(viewModel);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            // 删除设备时，同时删除用户设备关联
            await _UserDeviceServices.RemoveByDeviceId(Id);
            return await _DeviceServices.RemoveById(Id);
        }

        /// <summary>
        /// 根据设备ID获取绑定的用户ID列表
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult<List<string>>> GetUserIdsByDeviceId(string deviceId)
        {
            return Ok(await _UserDeviceServices.GetUserIdsByDeviceId(deviceId));
        }

        /// <summary>
        /// 绑定设备到用户
        /// </summary>
        /// <param name="request">请求模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<bool>> BindUsers([FromBody] BindUsersRequest request)
        {
            return Ok(await _UserDeviceServices.AddUserDevices(request.DeviceId, request.UserIds));
        }
    }

    public class BindUsersRequest
    {
        public string DeviceId { get; set; }
        public List<string> UserIds { get; set; }
    }
}
