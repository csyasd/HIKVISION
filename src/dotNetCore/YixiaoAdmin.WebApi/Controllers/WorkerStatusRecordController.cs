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
    /// 工人进出状态记录控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class WorkerStatusRecordController : ControllerBase
    {
        private readonly IWorkerStatusRecordServices _workerStatusRecordServices;

        public WorkerStatusRecordController(IWorkerStatusRecordServices workerStatusRecordServices)
        {
            _workerStatusRecordServices = workerStatusRecordServices ?? 
                                               throw new ArgumentNullException(nameof(workerStatusRecordServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<WorkerStatusRecord>> All()
        {
            return await _workerStatusRecordServices.Query();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<WorkerStatusRecord>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _workerStatusRecordServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<WorkerStatusRecord> Get(string Id)
        {
            return await _workerStatusRecordServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(WorkerStatusRecord viewModel)
        {
            return await _workerStatusRecordServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(WorkerStatusRecord viewModel)
        {
            return await _workerStatusRecordServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _workerStatusRecordServices.RemoveById(Id);
        }
    }
}
