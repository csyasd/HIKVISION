using YixiaoAdmin.Models;
using YixiaoAdmin.Common;
using System.Threading.Tasks;

namespace YixiaoAdmin.IServices
{
    /// <summary>
    /// 气体报警记录服务接口
    /// </summary>
    public interface IGasAlarmRecordServices : IBaseServices<GasAlarmRecord>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="queryPageModel">分页搜索模型</param>
        /// <returns></returns>
        Task<PagesResponse> QueryPages(QueryPageModel queryPageModel);
    }
}

