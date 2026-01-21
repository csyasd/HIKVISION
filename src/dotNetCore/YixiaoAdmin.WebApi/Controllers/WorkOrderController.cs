
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

//这是 WorkRecord 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderServices _WorkRecordServices;
        private readonly IDeviceServices _DeviceServices;
        private readonly IUserDeviceServices _UserDeviceServices;
        private readonly IUserServices _UserServices;
        private readonly IRoleServices _RoleServices;

        public WorkOrderController(IWorkOrderServices WorkRecordServices, IDeviceServices DeviceServices, IUserDeviceServices UserDeviceServices, IUserServices UserServices, IRoleServices RoleServices)
        {
            _WorkRecordServices = WorkRecordServices ?? 
                                       throw new ArgumentNullException(nameof(WorkRecordServices));
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
        /// 获取在线设备的在线工单
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<WorkOrder>> GetRealtimeWorkOrders()
        {
            // 获取当前用户绑定的设备ID列表
            var userDeviceIds = await GetUserBoundDeviceIds();
            
            // 获取设备列表（管理员获取所有设备，普通用户获取绑定的设备）
            IList<Device> devices;
            if (userDeviceIds == null)
            {
                devices = await _DeviceServices.Query();
            }
            else if (!userDeviceIds.Any())
            {
                return new List<WorkOrder>();
            }
            else
            {
                var allDevices = await _DeviceServices.Query();
                devices = allDevices.Where(d => userDeviceIds.Contains(d.Id)).ToList();
            }
            
            // 筛选在线设备ID
            var onlineDeviceIds = devices
                .Where(d => d.OnlineStatus == "在线")
                .Select(d => d.Id)
                .ToList();

            // 获取实时在线工单
            return await _WorkRecordServices.GetRealtimeWorkOrders(onlineDeviceIds);
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<WorkOrder>> All()
        {
            var deviceIds = await GetUserBoundDeviceIds();
            
            if (deviceIds == null)
            {
                return await _WorkRecordServices.Query();
            }
            
            if (!deviceIds.Any())
            {
                return new List<WorkOrder>();
            }
            
            var allWorkOrders = await _WorkRecordServices.Query();
            return allWorkOrders.Where(wo => wo.DeviceId != null && deviceIds.Contains(wo.DeviceId)).ToList();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> Pages(QueryPageModel queryPageModel)
        {
            var deviceIds = await GetUserBoundDeviceIds();
            
            if (deviceIds == null)
            {
                return Ok(await _WorkRecordServices.QueryPages(queryPageModel));
            }
            
            if (!deviceIds.Any())
            {
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<WorkOrder>(), 0);
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
            
            return Ok(await _WorkRecordServices.QueryPages(queryPageModel));
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<WorkOrder> Get(string Id)
        {
            return await _WorkRecordServices.QueryById(Id);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(WorkOrder viewModel)
        {

            return await _WorkRecordServices.Add(viewModel);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(WorkOrder viewModel)
        {
            return await _WorkRecordServices.Update(viewModel);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _WorkRecordServices.RemoveById(Id);
        }
    }
}
