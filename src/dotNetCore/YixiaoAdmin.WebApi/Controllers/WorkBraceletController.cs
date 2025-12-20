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
    /// <summary>
    /// 作业手环控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class WorkBraceletController : ControllerBase
    {
        private readonly IWorkBraceletServices _workBraceletServices;
        private readonly IDeviceServices _deviceServices;
        private readonly IWorkOrderServices _workOrderServices;

        public WorkBraceletController(
            IWorkBraceletServices workBraceletServices,
            IDeviceServices deviceServices,
            IWorkOrderServices workOrderServices)
        {
            _workBraceletServices = workBraceletServices ?? 
                                       throw new ArgumentNullException(nameof(workBraceletServices));
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
        public async Task<IList<WorkBracelet>> All()
        {
            return await _workBraceletServices.Query();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<WorkBracelet>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _workBraceletServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<WorkBracelet> Get(string Id)
        {
            return await _workBraceletServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(WorkBracelet viewModel)
        {
            return await _workBraceletServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(WorkBracelet viewModel)
        {
            return await _workBraceletServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _workBraceletServices.RemoveById(Id);
        }

        /// <summary>
        /// 获取实时手环信息（在线设备的工单开始状态的作业手环）
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> GetRealtimeBraceletInfo()
        {
            try
            {
                // 1. 获取所有在线设备
                var allDevices = await _deviceServices.Query();
                var onlineDevices = allDevices.Where(d => d.OnlineStatus == "在线").ToList();

                if (!onlineDevices.Any())
                {
                    return Ok(new List<BraceletInfoItem>());
                }

                // 2. 获取所有工单状态为1（工单开始）的工单
                var allWorkOrders = await _workOrderServices.Query();
                var activeWorkOrders = allWorkOrders
                    .Where(wo => wo.Status == 1 && onlineDevices.Any(d => d.Id == wo.DeviceId))
                    .ToList();

                if (!activeWorkOrders.Any())
                {
                    return Ok(new List<BraceletInfoItem>());
                }

                // 3. 获取所有作业手环
                var allBracelets = await _workBraceletServices.Query();

                // 4. 构建返回数据
                var result = new List<BraceletInfoItem>();

                foreach (var workOrder in activeWorkOrders)
                {
                    // 获取该工单对应的设备
                    var device = onlineDevices.FirstOrDefault(d => d.Id == workOrder.DeviceId);
                    if (device == null) continue;

                    // 格式化设备显示名称：设备名称/设备型号
                    var deviceDisplayName = string.IsNullOrWhiteSpace(device.Model) 
                        ? device.Name 
                        : $"{device.Name}/{device.Model}";

                    // 获取该工单的所有作业手环
                    var bracelets = allBracelets
                        .Where(b => b.WorkOrderId == workOrder.Id)
                        .ToList();

                    foreach (var bracelet in bracelets)
                    {
                        // 根据存储的状态值显示对应的状态文本
                        // 状态码对应关系：
                        // 0 => "未进入"
                        // 1 => "申请进入"
                        // 2 => "刷卡成功"
                        // 3 => "进入"
                        // 4 => "申请签出"
                        // 5 => "已签出"
                        string entryExitStatusDisplay = "未知";
                        if (!string.IsNullOrEmpty(bracelet.EntryExitStatus))
                        {
                            entryExitStatusDisplay = bracelet.EntryExitStatus switch
                            {
                                "0" => "未进入",
                                "1" => "申请进入",
                                "2" => "刷卡成功",
                                "3" => "进入",
                                "4" => "申请签出",
                                "5" => "已签出",
                                _ => $"未知({bracelet.EntryExitStatus})"
                            };
                        }

                        result.Add(new BraceletInfoItem
                        {
                            DeviceName = deviceDisplayName,
                            WorkOrderCode = workOrder.Code ?? "",
                            WorkerName = bracelet.WorkerName ?? "",
                            HeartRate = bracelet.HeartRate ?? "",
                            EntryExitStatus = entryExitStatusDisplay,
                            EntryTime = bracelet.EntryTime ?? "",
                            ExitTime = bracelet.ExitTime ?? "",
                            WorkOrderId = workOrder.Id,
                            BraceletId = bracelet.Id
                        });
                    }
                }

                // 按工单编号和工人姓名排序
                var sortedResult = result
                    .OrderBy(r => r.WorkOrderCode)
                    .ThenBy(r => r.WorkerName)
                    .ToList();

                return Ok(sortedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "获取手环信息失败", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// 手环信息项
    /// </summary>
    public class BraceletInfoItem
    {
        public string DeviceName { get; set; }
        public string WorkOrderCode { get; set; }
        public string WorkerName { get; set; }
        public string HeartRate { get; set; }
        public string EntryExitStatus { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
        public string WorkOrderId { get; set; }
        public string BraceletId { get; set; }
    }
}
