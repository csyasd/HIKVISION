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
    /// 气体异常记录控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class GasAbnormalController : ControllerBase
    {
        private readonly IGasAbnormalServices _gasAbnormalServices;

        public GasAbnormalController(IGasAbnormalServices gasAbnormalServices)
        {
            _gasAbnormalServices = gasAbnormalServices ?? 
                                               throw new ArgumentNullException(nameof(gasAbnormalServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<GasAbnormal>> All()
        {
            return await _gasAbnormalServices.Query();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<GasAbnormal>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _gasAbnormalServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<GasAbnormal> Get(string Id)
        {
            return await _gasAbnormalServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(GasAbnormal viewModel)
        {
            return await _gasAbnormalServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(GasAbnormal viewModel)
        {
            return await _gasAbnormalServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _gasAbnormalServices.RemoveById(Id);
        }
    }
}


