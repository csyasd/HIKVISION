using System;
using System.Threading.Tasks;
using YixiaoAdmin.IRepository;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;
using YixiaoAdmin.Common;
using System.Linq.Expressions;
using System.Linq;

namespace YixiaoAdmin.Services
{
    /// <summary>
    /// 作业记录服务实现
    /// </summary>
    public class WorkRecordServices : BaseServices<WorkRecord>, IWorkRecordServices
    {
        private IWorkRecordRepository _workRecordRepository;
        
        public WorkRecordServices(IWorkRecordRepository repository)
        {
            this._workRecordRepository = repository;
            base.baseRepository = _workRecordRepository;
        }

        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
            //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<WorkRecord, bool>> whereExpression = PredicateBuilder.True<WorkRecord>();
            //判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(WorkRecord).GetProperty(item.QueryField);
                    if (property == null)
                    {
                        continue;
                    }
                    if (item.QueryStr == null || item.QueryStr == "")
                    {
                        continue;
                    }
                    if (item.QueryField == "Name")
                    {
                        whereExpression = PredicateBuilder.And<WorkRecord>(whereExpression, (x) => x.Name.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "WorkOrderId")
                    {
                        // 支持多个工单ID查询（用逗号分隔）
                        if (item.QueryStr.Contains(","))
                        {
                            var workOrderIds = item.QueryStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (workOrderIds.Length > 0)
                            {
                                // 如果包含特殊标记，表示无匹配结果
                                if (workOrderIds[0] == "__NO_MATCH__")
                                {
                                    whereExpression = PredicateBuilder.And(whereExpression, (x) => false);
                                }
                                else
                                {
                                    whereExpression = PredicateBuilder.And(whereExpression, (x) => workOrderIds.Contains(x.WorkOrderId));
                                }
                            }
                        }
                        else
                        {
                            // 单个工单ID查询
                            if (item.QueryStr == "__NO_MATCH__")
                            {
                                whereExpression = PredicateBuilder.And(whereExpression, (x) => false);
                            }
                            else
                            {
                                whereExpression = PredicateBuilder.And(whereExpression, (x) => x.WorkOrderId == item.QueryStr);
                            }
                        }
                    }
                    else if (item.QueryField == "WorkerName")
                    {
                        whereExpression = PredicateBuilder.And<WorkRecord>(whereExpression, (x) => x.WorkerName.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "HeartRate")
                    {
                        whereExpression = PredicateBuilder.And<WorkRecord>(whereExpression, (x) => x.HeartRate.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "EntryExitStatus")
                    {
                        whereExpression = PredicateBuilder.And<WorkRecord>(whereExpression, (x) => x.EntryExitStatus.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And<WorkRecord>(whereExpression, (x) => x.CreateTime.Date == Convert.ToDateTime(item.QueryStr.Trim()));
                    }
                    else if (item.QueryField == "Id")
                    {
                        whereExpression = PredicateBuilder.And<WorkRecord>(whereExpression, (x) => x.Id.Contains(item.QueryStr));
                    }
                }
            }
            pagesResponse.Success((await _workRecordRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber)).ToList());
            pagesResponse.count = (await _workRecordRepository.Query(whereExpression)).Count();
            return pagesResponse;
        }
    }
}
