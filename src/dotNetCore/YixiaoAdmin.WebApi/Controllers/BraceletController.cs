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
    /// 手环控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BraceletController : ControllerBase
    {
        private readonly IBraceletServices _braceletServices;

        public BraceletController(IBraceletServices braceletServices)
        {
            _braceletServices = braceletServices ?? 
                                       throw new ArgumentNullException(nameof(braceletServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<Bracelet>> All()
        {
            return await _braceletServices.Query();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<Bracelet>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _braceletServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Bracelet> Get(string Id)
        {
            return await _braceletServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(Bracelet viewModel)
        {
            return await _braceletServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(Bracelet viewModel)
        {
            return await _braceletServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _braceletServices.RemoveById(Id);
        }
    }
}
