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
    /// 工人进出状态记录服务实现
    /// </summary>
    public class WorkerStatusRecordServices : BaseServices<WorkerStatusRecord>, IWorkerStatusRecordServices
    {
        private IWorkerStatusRecordRepository _workerStatusRecordRepository;
        
        public WorkerStatusRecordServices(IWorkerStatusRecordRepository repository)
        {
            this._workerStatusRecordRepository = repository;
            base.baseRepository = _workerStatusRecordRepository;
        }

        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
            PagesResponse pagesResponse = new PagesResponse();
            Expression<Func<WorkerStatusRecord, bool>> whereExpression = PredicateBuilder.True<WorkerStatusRecord>();
            
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    if (string.IsNullOrEmpty(item.QueryStr))
                    {
                        continue;
                    }

                    if (item.QueryField == "WorkOrderId")
                    {
                        if (item.QueryStr.Contains(","))
                        {
                            var ids = item.QueryStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (ids.Length > 0)
                            {
                                if (ids[0] == "__NO_MATCH__") whereExpression = PredicateBuilder.And(whereExpression, (x) => false);
                                else whereExpression = PredicateBuilder.And(whereExpression, (x) => ids.Contains(x.WorkOrderId));
                            }
                        }
                        else
                        {
                            if (item.QueryStr == "__NO_MATCH__") whereExpression = PredicateBuilder.And(whereExpression, (x) => false);
                            else whereExpression = PredicateBuilder.And(whereExpression, (x) => x.WorkOrderId == item.QueryStr);
                        }
                    }
                    else if (item.QueryField == "WorkerName")
                    {
                        whereExpression = PredicateBuilder.And<WorkerStatusRecord>(whereExpression, (x) => x.WorkerName.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "HeartRate")
                    {
                        whereExpression = PredicateBuilder.And<WorkerStatusRecord>(whereExpression, (x) => x.HeartRate.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "EntryExitStatus")
                    {
                        whereExpression = PredicateBuilder.And<WorkerStatusRecord>(whereExpression, (x) => x.EntryExitStatus == item.QueryStr);
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        if (DateTime.TryParse(item.QueryStr.Trim(), out DateTime dt))
                        {
                            whereExpression = PredicateBuilder.And<WorkerStatusRecord>(whereExpression, (x) => x.CreateTime.Date == dt.Date);
                        }
                    }
                }
            }
            
            var list = await _workerStatusRecordRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber);
            var count = await _workerStatusRecordRepository.Query(whereExpression);
            
            pagesResponse.Success(list.ToList(), count.Count());
            return pagesResponse;
        }
    }
}
