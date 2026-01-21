
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

//这是 CameraRecord 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CameraRecordController : ControllerBase
    {
        private readonly ICameraRecordServices _CameraRecordServices;
        private readonly IUserDeviceServices _UserDeviceServices;
        private readonly IUserServices _UserServices;
        private readonly IRoleServices _RoleServices;
        private readonly ICameraServices _CameraServices;

        public CameraRecordController(ICameraRecordServices CameraRecordServices, IUserDeviceServices UserDeviceServices, IUserServices UserServices, IRoleServices RoleServices, ICameraServices CameraServices)
        {
            _CameraRecordServices = CameraRecordServices ?? 
                                       throw new ArgumentNullException(nameof(CameraRecordServices));
            _UserDeviceServices = UserDeviceServices ?? 
                                       throw new ArgumentNullException(nameof(UserDeviceServices));
            _UserServices = UserServices ?? 
                                       throw new ArgumentNullException(nameof(UserServices));
            _RoleServices = RoleServices ?? 
                                       throw new ArgumentNullException(nameof(RoleServices));
            _CameraServices = CameraServices ?? 
                                       throw new ArgumentNullException(nameof(CameraServices));
        }
        
        /// <summary>
        /// 获取当前用户绑定的摄像头ID列表（如果是管理员返回null，表示返回所有摄像头）
        /// </summary>
        /// <returns>管理员返回null，普通用户返回摄像头ID列表</returns>
        private async Task<List<string>> GetUserBoundCameraIds()
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
                    
                    // 获取用户绑定的设备ID列表
                    var deviceIds = await _UserDeviceServices.GetDeviceIdsByUserId(userId);
                    if (!deviceIds.Any()) return new List<string>();
                    
                    // 获取这些设备对应的摄像头ID列表
                    var allCameras = await _CameraServices.Query();
                    return allCameras
                        .Where(c => c.DeviceId != null && deviceIds.Contains(c.DeviceId))
                        .Select(c => c.Id)
                        .ToList();
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
        public async Task<IList<CameraRecord>> All()
        {
            var cameraIds = await GetUserBoundCameraIds();
            
            if (cameraIds == null)
            {
                return await _CameraRecordServices.Query();
            }
            
            if (!cameraIds.Any())
            {
                return new List<CameraRecord>();
            }
            
            var allRecords = await _CameraRecordServices.Query();
            return allRecords.Where(cr => cr.CameraId != null && cameraIds.Contains(cr.CameraId)).ToList();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<CameraRecord>>> Pages(QueryPageModel queryPageModel)
        {
            var cameraIds = await GetUserBoundCameraIds();
            
            if (cameraIds == null)
            {
                return Ok(await _CameraRecordServices.QueryPages(queryPageModel));
            }
            
            if (!cameraIds.Any())
            {
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<CameraRecord>(), 0);
                return Ok(emptyResponse);
            }
            
            var queryList = queryPageModel.Query?.ToList() ?? new List<QueryFieldModel>();
            queryList.Add(new QueryFieldModel 
            { 
                QueryField = "CameraId", 
                QueryStr = string.Join(",", cameraIds) 
            });
            queryPageModel.Query = queryList.ToArray();
            
            return Ok(await _CameraRecordServices.QueryPages(queryPageModel));
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<CameraRecord> Get(string Id)
        {
            return await _CameraRecordServices.QueryById(Id);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(CameraRecord viewModel)
        {

            return await _CameraRecordServices.Add(viewModel);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(CameraRecord viewModel)
        {
            return await _CameraRecordServices.Update(viewModel);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _CameraRecordServices.RemoveById(Id);
        }
    }
}
