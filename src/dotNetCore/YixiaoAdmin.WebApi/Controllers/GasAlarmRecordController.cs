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


        public GasAlarmRecordController(
            IGasAlarmRecordServices GasAlarmRecordServices,
            IDeviceServices deviceServices,
            IWorkOrderServices workOrderServices)
        {
            _GasAlarmRecordServices = GasAlarmRecordServices ?? 
                                       throw new ArgumentNullException(nameof(GasAlarmRecordServices));
            _deviceServices = deviceServices ?? 
                              throw new ArgumentNullException(nameof(deviceServices));
            _workOrderServices = workOrderServices ?? 
                                 throw new ArgumentNullException(nameof(workOrderServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<GasAlarmRecord>> All()
        {
            return await _GasAlarmRecordServices.Query();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<PagesResponse>> Pages(QueryPageModel queryPageModel)
        {
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
                // 1. 获取所有在线设备
                var allDevices = await _deviceServices.Query();
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

                    for (int i = 0; i < gasValues.Length; i++)
                    {
                        var gasValue = gasValues[i];
                        // 只显示有数值的气体（大于0）
                        if (gasValue > 0)
                        {
                            // 根据气体类型格式化数值和单位
                            string formattedValue;
                            if (i == 0) // O2
                            {
                                formattedValue = $"{gasValue:F1}%VOL";
                            }
                            else if (i == 1) // CH4
                            {
                                formattedValue = $"{gasValue:F1}%LEL";
                            }
                            else // 其他气体（ppm）
                            {
                                formattedValue = $"{gasValue:F1}ppm";
                            }

                            // 格式化设备名称：设备名称/设备型号
                            var deviceDisplayName = string.IsNullOrWhiteSpace(device.Model) 
                                ? device.Name 
                                : $"{device.Name}/{device.Model}";
                            
                            result.Add(new GasMonitoringItem
                            {
                                DeviceName = deviceDisplayName,
                                WorkOrderCode = workOrder.Code ?? "",
                                GasName = gasNames[i],
                                GasValue = formattedValue,
                                Status = "在线",
                                CreateTime = latestGasRecord.CreateTime,
                                DeviceId = device.Id,
                                WorkOrderId = workOrder.Id
                            });
                        }
                    }
                }

                // 按设备名称和气体名称排序
                var sortedResult = result
                    .OrderBy(r => r.DeviceName)
                    .ThenBy(r => r.GasName)
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
