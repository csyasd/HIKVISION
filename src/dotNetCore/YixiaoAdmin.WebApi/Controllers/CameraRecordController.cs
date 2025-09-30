
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

//这是 CameraRecord 控制器

namespace YixiaoAdmin.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CameraRecordController : ControllerBase
    {
        private readonly ICameraRecordServices _CameraRecordServices;


        public CameraRecordController(ICameraRecordServices CameraRecordServices)
        {
            _CameraRecordServices = CameraRecordServices ?? 
                                       throw new ArgumentNullException(nameof(CameraRecordServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<CameraRecord>> All()
        {
            return await _CameraRecordServices.Query();
        }
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<CameraRecord>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _CameraRecordServices.QueryPages(queryPageModel));
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<CameraRecord> Get(string Id)
        {
            return await _CameraRecordServices.QueryById(Id);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(CameraRecord viewModel)
        {

            return await _CameraRecordServices.Add(viewModel);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(CameraRecord viewModel)
        {
            return await _CameraRecordServices.Update(viewModel);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _CameraRecordServices.RemoveById(Id);
        }
    }
}
