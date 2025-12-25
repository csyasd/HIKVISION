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
    /// 异常配置控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AbnormalConfigController : ControllerBase
    {
        private readonly IAbnormalConfigServices _abnormalConfigServices;

        public AbnormalConfigController(IAbnormalConfigServices abnormalConfigServices)
        {
            _abnormalConfigServices = abnormalConfigServices ?? 
                                               throw new ArgumentNullException(nameof(abnormalConfigServices));
        }

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<AbnormalConfig>> All()
        {
            return await _abnormalConfigServices.Query();
        }

        /// <summary>
        /// 根据配置名称查询
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<AbnormalConfig> GetByConfigName(string configName)
        {
            var configs = await _abnormalConfigServices.Query(c => c.ConfigName == configName && c.IsEnabled);
            return configs.FirstOrDefault();
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<AbnormalConfig> Get(string Id)
        {
            return await _abnormalConfigServices.QueryById(Id);
        }

        /// <summary>
        /// 添加或更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> Post(AbnormalConfig viewModel)
        {
            // 检查是否已存在相同配置名称的配置
            var existingConfigs = await _abnormalConfigServices.Query(c => c.ConfigName == viewModel.ConfigName);
            var existingConfig = existingConfigs.FirstOrDefault();
            
            if (existingConfig != null)
            {
                // 更新现有配置，确保IsEnabled为true
                existingConfig.MinValue = viewModel.MinValue;
                existingConfig.MaxValue = viewModel.MaxValue;
                existingConfig.IsEnabled = true; // 强制启用检测
                existingConfig.ModificationTime = DateTime.Now;
                return await _abnormalConfigServices.Update(existingConfig);
            }
            else
            {
                // 添加新配置，确保IsEnabled默认为true
                viewModel.IsEnabled = true;
                return await _abnormalConfigServices.Add(viewModel);
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> Put(AbnormalConfig viewModel)
        {
            return await _abnormalConfigServices.Update(viewModel);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<bool> Delete(string Id)
        {
            return await _abnormalConfigServices.RemoveById(Id);
        }
    }
}

