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

namespace YixiaoAdmin.WebApi.Controllers
{
    /// <summary>
    /// 作业记录控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class WorkRecordController : ControllerBase
    {
        private readonly IWorkRecordServices _workRecordServices;
        private readonly IUserDeviceServices _UserDeviceServices;
        private readonly IUserServices _UserServices;
        private readonly IRoleServices _RoleServices;
        private readonly IWorkOrderServices _WorkOrderServices;

        public WorkRecordController(IWorkRecordServices workRecordServices, IUserDeviceServices UserDeviceServices, IUserServices UserServices, IRoleServices RoleServices, IWorkOrderServices WorkOrderServices)
        {
            _workRecordServices = workRecordServices ?? 
                                       throw new ArgumentNullException(nameof(workRecordServices));
            _UserDeviceServices = UserDeviceServices ?? 
                                       throw new ArgumentNullException(nameof(UserDeviceServices));
            _UserServices = UserServices ?? 
                                       throw new ArgumentNullException(nameof(UserServices));
            _RoleServices = RoleServices ?? 
                                       throw new ArgumentNullException(nameof(RoleServices));
            _WorkOrderServices = WorkOrderServices ?? 
                                       throw new ArgumentNullException(nameof(WorkOrderServices));
        }
        
        /// <summary>
        /// 获取当前用户绑定的工单ID列表（如果是管理员返回null，表示返回所有工单）
        /// </summary>
        /// <returns>管理员返回null，普通用户返回工单ID列表</returns>
        private async Task<List<string>> GetUserBoundWorkOrderIds()
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
                    
                    // 获取这些设备对应的工单ID列表
                    var allWorkOrders = await _WorkOrderServices.Query();
                    return allWorkOrders
                        .Where(wo => wo.DeviceId != null && deviceIds.Contains(wo.DeviceId))
                        .Select(wo => wo.Id)
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
        public async Task<IList<WorkRecord>> All()
        {
            var workOrderIds = await GetUserBoundWorkOrderIds();
            
            if (workOrderIds == null)
            {
                return await _workRecordServices.Query();
            }
            
            if (!workOrderIds.Any())
            {
                return new List<WorkRecord>();
            }
            
            var allRecords = await _workRecordServices.Query();
            return allRecords.Where(wr => wr.WorkOrderId != null && workOrderIds.Contains(wr.WorkOrderId)).ToList();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<WorkRecord>>> Pages(QueryPageModel queryPageModel)
        {
            var workOrderIds = await GetUserBoundWorkOrderIds();
            
            if (workOrderIds == null)
            {
                return Ok(await _workRecordServices.QueryPages(queryPageModel));
            }
            
            if (!workOrderIds.Any())
            {
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<WorkRecord>(), 0);
                return Ok(emptyResponse);
            }
            
            // 检查前端是否已经提供了 WorkOrderId 查询条件
            var queryList = queryPageModel.Query?.ToList() ?? new List<QueryFieldModel>();
            var existingWorkOrderQuery = queryList.FirstOrDefault(q => q.QueryField == "WorkOrderId");
            
            if (existingWorkOrderQuery != null)
            {
                // 前端已提供 WorkOrderId 条件,需要与用户权限取交集
                var requestedWorkOrderIds = existingWorkOrderQuery.QueryStr
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .ToList();
                
                // 取交集:只返回用户有权限且前端请求的工单
                var allowedWorkOrderIds = requestedWorkOrderIds
                    .Where(id => workOrderIds.Contains(id))
                    .ToList();
                
                if (allowedWorkOrderIds.Any())
                {
                    existingWorkOrderQuery.QueryStr = string.Join(",", allowedWorkOrderIds);
                }
                else
                {
                    // 没有交集,返回空结果
                    existingWorkOrderQuery.QueryStr = "__NO_MATCH__";
                }
            }
            else
            {
                // 前端未提供 WorkOrderId 条件,添加用户权限过滤
                queryList.Add(new QueryFieldModel 
                { 
                    QueryField = "WorkOrderId", 
                    QueryStr = string.Join(",", workOrderIds) 
                });
            }
            
            queryPageModel.Query = queryList.ToArray();
            
            return Ok(await _workRecordServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<WorkRecord> Get(string Id)
        {
            return await _workRecordServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(WorkRecord viewModel)
        {
            return await _workRecordServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(WorkRecord viewModel)
        {
            return await _workRecordServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _workRecordServices.RemoveById(Id);
        }
    }
}
