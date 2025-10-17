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
    /// 人员控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class PersonnelController : ControllerBase
    {
        private readonly IPersonnelServices _personnelServices;

        public PersonnelController(IPersonnelServices personnelServices)
        {
            _personnelServices = personnelServices ?? 
                                       throw new ArgumentNullException(nameof(personnelServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<Personnel>> All()
        {
            return await _personnelServices.Query();
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="queryPageModel">查询模型</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<IEnumerable<Personnel>>> Pages(QueryPageModel queryPageModel)
        {
            return Ok(await _personnelServices.QueryPages(queryPageModel));
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Personnel> Get(string Id)
        {
            return await _personnelServices.QueryById(Id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(Personnel viewModel)
        {
            return await _personnelServices.Add(viewModel);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(Personnel viewModel)
        {
            return await _personnelServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _personnelServices.RemoveById(Id);
        }
    }
}
