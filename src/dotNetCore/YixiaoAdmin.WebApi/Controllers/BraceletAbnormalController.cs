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
    /// 手环异常记录控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BraceletAbnormalController : ControllerBase
    {
        private readonly IBraceletAbnormalServices _braceletAbnormalServices;

        public BraceletAbnormalController(IBraceletAbnormalServices braceletAbnormalServices)
        {
            _braceletAbnormalServices = braceletAbnormalServices ?? 
                                               throw new ArgumentNullException(nameof(braceletAbnormalServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<BraceletAbnormal>> All()
        {
            return await _braceletAbnormalServices.Query();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<BraceletAbnormal>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _braceletAbnormalServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<BraceletAbnormal> Get(string Id)
        {
            return await _braceletAbnormalServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(BraceletAbnormal viewModel)
        {
            return await _braceletAbnormalServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(BraceletAbnormal viewModel)
        {
            return await _braceletAbnormalServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _braceletAbnormalServices.RemoveById(Id);
        }
    }
}



