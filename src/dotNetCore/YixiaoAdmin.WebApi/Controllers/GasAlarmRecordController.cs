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
    [Route("[controller]")]
    [ApiController]
    public class GasAlarmRecordController : ControllerBase
    {
        private readonly IGasAlarmRecordServices _GasAlarmRecordServices;


        public GasAlarmRecordController(IGasAlarmRecordServices GasAlarmRecordServices)
        {
            _GasAlarmRecordServices = GasAlarmRecordServices ?? 
                                       throw new ArgumentNullException(nameof(GasAlarmRecordServices));
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
    }
}

