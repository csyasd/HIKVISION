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

//这是 GasAlarmRecord 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    /// <summary>
    /// 气体监测数据项
    /// </summary>
    public class GasMonitoringItem
    {
        public string DeviceName { get; set; }
        public string WorkOrderCode { get; set; }
        public string GasName { get; set; }
        public string GasValue { get; set; }
        public string Status { get; set; }
        public DateTime CreateTime { get; set; }
        public string DeviceId { get; set; }
        public string WorkOrderId { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    public class GasAlarmRecordController : ControllerBase
    {
        private readonly IGasAlarmRecordServices _GasAlarmRecordServices;
        private readonly IDeviceServices _deviceServices;
        private readonly IWorkOrderServices _workOrderServices;
        private readonly IUserDeviceServices _UserDeviceServices;
        private readonly IUserServices _UserServices;
        private readonly IRoleServices _RoleServices;

        public GasAlarmRecordController(
            IGasAlarmRecordServices GasAlarmRecordServices,
            IDeviceServices deviceServices,
            IWorkOrderServices workOrderServices,
            IUserDeviceServices UserDeviceServices,
            IUserServices UserServices,
            IRoleServices RoleServices)
        {
            _GasAlarmRecordServices = GasAlarmRecordServices ?? 
                                       throw new ArgumentNullException(nameof(GasAlarmRecordServices));
            _deviceServices = deviceServices ?? 
                              throw new ArgumentNullException(nameof(deviceServices));
            _workOrderServices = workOrderServices ?? 
                                 throw new ArgumentNullException(nameof(workOrderServices));
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
        /// 获取当前用户绑定的工单ID列表（如果是管理员返回null，表示返回所有工单）
        /// </summary>
        /// <returns>管理员返回null，普通用户返回工单ID列表</returns>
        private async Task<List<string>> GetUserBoundWorkOrderIds()
        {
            var deviceIds = await GetUserBoundDeviceIds();
            
            if (deviceIds == null)
            {
                return null;
            }
            
            if (!deviceIds.Any())
            {
                return new List<string>();
            }
            
            var allWorkOrders = await _workOrderServices.Query();
            return allWorkOrders
                .Where(wo => wo.DeviceId != null && deviceIds.Contains(wo.DeviceId))
                .Select(wo => wo.Id)
                .ToList();
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<GasAlarmRecord>> All()
        {
            var workOrderIds = await GetUserBoundWorkOrderIds();
            
            if (workOrderIds == null)
            {
                return await _GasAlarmRecordServices.Query();
            }
            
            if (!workOrderIds.Any())
            {
                return new List<GasAlarmRecord>();
            }
            
            var allRecords = await _GasAlarmRecordServices.Query();
            return allRecords.Where(gar => gar.WorkOrderId != null && workOrderIds.Contains(gar.WorkOrderId)).ToList();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<PagesResponse>> Pages(QueryPageModel queryPageModel)
        {
            var workOrderIds = await GetUserBoundWorkOrderIds();
            
            if (workOrderIds == null)
            {
                return Ok(await _GasAlarmRecordServices.QueryPages(queryPageModel));
            }
            
            if (!workOrderIds.Any())
            {
                var emptyResponse = new PagesResponse();
                emptyResponse.Success(new List<GasAlarmRecord>(), 0);
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
            
            return Ok(await _GasAlarmRecordServices.QueryPages(queryPageModel));
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<GasAlarmRecord> Get(string Id)
        {
            return await _GasAlarmRecordServices.QueryById(Id);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(GasAlarmRecord viewModel)
        {

            return await _GasAlarmRecordServices.Add(viewModel);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(GasAlarmRecord viewModel)
        {
            return await _GasAlarmRecordServices.Update(viewModel);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _GasAlarmRecordServices.RemoveById(Id);
        }

        /// <summary>
        /// 获取在线设备的最新气体监测实时数据（仅显示工单开始状态的设备）
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> GetRealtimeGasData()
        {
            try
            {
                // 获取当前用户绑定的设备ID列表
                var userDeviceIds = await GetUserBoundDeviceIds();
                
                // 1. 获取设备列表（管理员获取所有设备，普通用户获取绑定的设备）
                IList<Device> allDevices;
                if (userDeviceIds == null)
                {
                    allDevices = await _deviceServices.Query();
                }
                else if (!userDeviceIds.Any())
                {
                    return Ok(new List<GasMonitoringItem>());
                }
                else
                {
                    var allDevicesList = await _deviceServices.Query();
                    allDevices = allDevicesList.Where(d => userDeviceIds.Contains(d.Id)).ToList();
                }
                
                var onlineDevices = allDevices.Where(d => d.OnlineStatus == "在线").ToList();

                if (!onlineDevices.Any())
                {
                    return Ok(new List<GasMonitoringItem>());
                }

                // 2. 获取所有工单状态为1（工单开始）的工单
                var allWorkOrders = await _workOrderServices.Query();
                var activeWorkOrders = allWorkOrders
                    .Where(wo => wo.Status == 1 && onlineDevices.Any(d => d.Id == wo.DeviceId))
                    .ToList();

                if (!activeWorkOrders.Any())
                {
                    return Ok(new List<GasMonitoringItem>());
                }

                // 3. 获取所有气体报警记录
                var allGasRecords = await _GasAlarmRecordServices.Query();
                
                // 4. 构建返回数据
                var result = new List<GasMonitoringItem>();
                
                // 气体名称映射（Gas1-Gas10对应的气体名称）
                var gasNames = new[] { "O2", "CH4", "H2S", "CO", "CO2", "NH3", "H2", "Cl2", "SO2", "NO2" };
                
                foreach (var workOrder in activeWorkOrders)
                {
                    var device = onlineDevices.FirstOrDefault(d => d.Id == workOrder.DeviceId);
                    if (device == null) continue;

                    // 获取该工单的最新气体报警记录（按创建时间降序）
                    var latestGasRecord = allGasRecords
                        .Where(gr => gr.WorkOrderId == workOrder.Id)
                        .OrderByDescending(gr => gr.CreateTime)
                        .FirstOrDefault();

                    if (latestGasRecord == null) continue;

                    // 将10种气体数据展开为多行
                    var gasValues = new[]
                    {
                        latestGasRecord.Gas1,
                        latestGasRecord.Gas2,
                        latestGasRecord.Gas3,
                        latestGasRecord.Gas4,
                        latestGasRecord.Gas5,
                        latestGasRecord.Gas6,
                        latestGasRecord.Gas7,
                        latestGasRecord.Gas8,
                        latestGasRecord.Gas9,
                        latestGasRecord.Gas10
                    };

                    // 定义要显示的4种特定气体及其映射
                    var displayConfigs = new[]
                    {
                        new { Index = 3, Name = "一氧化碳 (PPM)" },
                        new { Index = 2, Name = "硫化氢 (PPM)" },
                        new { Index = 1, Name = "甲烷 (%LEL)" },
                        new { Index = 8, Name = "二氧化硫 (PPM)" }
                    };

                    foreach (var config in displayConfigs)
                    {
                        var gasValue = gasValues[config.Index];
                        string formattedValue = $"{gasValue:F1}";

                        // 格式化设备名称：设备名称/设备型号
                        var deviceDisplayName = string.IsNullOrWhiteSpace(device.Model) 
                            ? device.Name 
                            : $"{device.Name}/{device.Model}";
                        
                        result.Add(new GasMonitoringItem
                        {
                            DeviceName = deviceDisplayName,
                            WorkOrderCode = workOrder.Code ?? "",
                            GasName = config.Name,
                            GasValue = formattedValue,
                            Status = "在线",
                            CreateTime = latestGasRecord.CreateTime,
                            DeviceId = device.Id,
                            WorkOrderId = workOrder.Id
                        });
                    }
                }

                // 按设备名称和气体名称排序
                var sortedResult = result
                    .OrderBy(r => r.DeviceName)
                    .ToList();

                return Ok(sortedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "获取气体监测数据失败", message = ex.Message });
            }
        }
    }
}
