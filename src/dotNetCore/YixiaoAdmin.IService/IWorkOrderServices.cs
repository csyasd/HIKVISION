
/****************************************************
 * 本文件由T4模板生成，请将本文件复制到YixiaoAdmin.IServices类库中使用
 * 文件名：IWorkRecordServices.cs
****************************************************/

using YixiaoAdmin.Models;
using YixiaoAdmin.Common;
using YixiaoAdmin.Models.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace YixiaoAdmin.IServices
{

    public partial interface  IWorkOrderServices:IBaseServices<WorkOrder>
    {
        /// <summary>
        /// 分页查询拓展
        /// </summary>
        /// <param name="queryPageModel">分页搜索模型</param>
        /// <returns></returns>
        Task<PagesResponse> QueryPagesExpand(QueryPageModel queryPageModel);

        /// <summary>
        /// 获取在线设备的在线工单
        /// </summary>
        /// <param name="onlineDeviceIds">在线设备ID列表</param>
        /// <returns></returns>
        Task<List<WorkOrder>> GetRealtimeWorkOrders(List<string> onlineDeviceIds);
    }
}
