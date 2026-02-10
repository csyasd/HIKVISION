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

        public WorkBraceletController(IWorkBraceletServices workBraceletServices)
        {
            _workBraceletServices = workBraceletServices ?? 
                                       throw new ArgumentNullException(nameof(workBraceletServices));
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
    }
}
