
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

//这是 WorkRecord 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderServices _WorkRecordServices;
        private readonly IDeviceServices _DeviceServices;


        public WorkOrderController(IWorkOrderServices WorkRecordServices, IDeviceServices DeviceServices)
        {
            _WorkRecordServices = WorkRecordServices ?? 
                                       throw new ArgumentNullException(nameof(WorkRecordServices));
            _DeviceServices = DeviceServices ?? 
                                       throw new ArgumentNullException(nameof(DeviceServices));
        }

        /// <summary>
        /// 获取在线设备的在线工单
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<WorkOrder>> GetRealtimeWorkOrders()
        {
            // 获取所有设备
            var allDevices = await _DeviceServices.Query();
            // 筛选在线设备ID
            var onlineDeviceIds = allDevices
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
            return await _WorkRecordServices.Query();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> Pages(QueryPageModel queryPageModel)
        {
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
