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
    /// 气体报警记录服务实现
    /// </summary>
    public class GasAlarmRecordServices : BaseServices<GasAlarmRecord>, IGasAlarmRecordServices
    {
        private IGasAlarmRecordRepository _gasAlarmRecordRepository;
        
        public GasAlarmRecordServices(IGasAlarmRecordRepository repository)
        {
            this._gasAlarmRecordRepository = repository;
            base.baseRepository = _gasAlarmRecordRepository;
        }

        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
            //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<GasAlarmRecord, bool>> whereExpression = PredicateBuilder.True<GasAlarmRecord>();
            //判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(GasAlarmRecord).GetProperty(item.QueryField);
                    if (property == null)
                    {
                        continue;
                    }
                    if (item.QueryStr == null || item.QueryStr == "")
                    {
                        continue;
                    }
                    if (item.QueryField == "WorkOrderId")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.WorkOrderId.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "Name")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Name.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.CreateTime.Date == Convert.ToDateTime(item.QueryStr.Trim()));
                    }
                    else if (item.QueryField == "Id")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Id.Contains(item.QueryStr));
                    }
                }
            }
            pagesResponse.Success((await _gasAlarmRecordRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber)).ToList());
            pagesResponse.count = (await _gasAlarmRecordRepository.Query(whereExpression)).Count();
            return pagesResponse;
        }
    }
}

