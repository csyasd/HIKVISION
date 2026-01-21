
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

//这是 Camera 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly ICameraServices _CameraServices;
        private readonly IUserDeviceServices _UserDeviceServices;
        private readonly IUserServices _UserServices;
        private readonly IRoleServices _RoleServices;

        public CameraController(ICameraServices CameraServices, IUserDeviceServices UserDeviceServices, IUserServices UserServices, IRoleServices RoleServices)
        {
            _CameraServices = CameraServices ?? 
                                       throw new ArgumentNullException(nameof(CameraServices));
            _UserDeviceServices = UserDeviceServices ?? 
                                       throw new ArgumentNullException(nameof(UserDeviceServices));
            _UserServices = UserServices ?? 
                                       throw new ArgumentNullException(nameof(UserServices));
            _RoleServices = RoleServices ?? 
                                       throw new ArgumentNullException(nameof(RoleServices));
        }
        
        /// <summary>
        /// 获取当前用户绑定的设备ID列表（如果是管理员返回null，表示返回所有设备）
        /// </summary>
        /// <returns>管理员返回null，普通用户返回设备ID列表</returns>
        private async Task<List<string>> GetUserBoundDeviceIds()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return new List<string>();
            }
            
            try
            {
                var userQuery = await _UserServices.Query(u => u.Id == userId);
                var user = userQuery.FirstOrDefault();
                
                if (user != null && user.Role == null && !string.IsNullOrEmpty(user.RoleId))
                {
                    var role = await _RoleServices.QueryById(user.RoleId);
                    if (role != null) user.Role = role;
                }
                
                if (user != null && user.Role != null)
                {
                    bool isAdmin = string.Equals(user.Role.Code, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(user.Role.Name, "管理员", StringComparison.OrdinalIgnoreCase) ||
                                  (user.Role.Name?.Contains("管理") == true) ||
                                  (user.Role.Name?.ToLower().Contains("admin") == true);
                    
                    if (isAdmin) return null;
                    return await _UserDeviceServices.GetDeviceIdsByUserId(userId);
                }
            }
            catch { }
            
            return new List<string>();
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<Camera>> All()
        {
            var deviceIds = await GetUserBoundDeviceIds();
            
            if (deviceIds == null)
            {
                return await _CameraServices.Query();
            }
            
            if (!deviceIds.Any())
            {
                return new List<Camera>();
            }
            
            var allCameras = await _CameraServices.Query();
            return allCameras.Where(c => c.DeviceId != null && deviceIds.Contains(c.DeviceId)).ToList();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<Camera>>> Pages(QueryPageModel queryPageModel)
        {
            var deviceIds = await GetUserBoundDeviceIds();
            
            if (deviceIds == null)
            {
                return Ok(await _CameraServices.QueryPagesExpand(queryPageModel));
            }
            
            if (!deviceIds.Any())
            {
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<Camera>(), 0);
                return Ok(emptyResponse);
            }
            
            // 添加设备ID过滤条件
            var queryList = queryPageModel.Query?.ToList() ?? new List<QueryFieldModel>();
            queryList.Add(new QueryFieldModel 
            { 
                QueryField = "DeviceId", 
                QueryStr = string.Join(",", deviceIds) 
            });
            queryPageModel.Query = queryList.ToArray();
            
            return Ok(await _CameraServices.QueryPagesExpand(queryPageModel));
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Camera> Get(string Id)
        {
            return await _CameraServices.QueryById(Id);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(Camera viewModel)
        {

            return await _CameraServices.Add(viewModel);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(Camera viewModel)
        {
            return await _CameraServices.Update(viewModel);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _CameraServices.RemoveById(Id);
        }
    }
}
