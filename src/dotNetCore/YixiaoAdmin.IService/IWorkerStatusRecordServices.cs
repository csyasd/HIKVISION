using System.Threading.Tasks;
using YixiaoAdmin.Models;
using YixiaoAdmin.Common;

namespace YixiaoAdmin.IServices
{
    /// <summary>
    /// 工人进出状态记录服务接口
    /// </summary>
    public interface IWorkerStatusRecordServices : IBaseServices<WorkerStatusRecord>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="queryPageModel">分页搜索模型</param>
        /// <returns></returns>
        Task<PagesResponse> QueryPages(QueryPageModel queryPageModel);
    }
}
