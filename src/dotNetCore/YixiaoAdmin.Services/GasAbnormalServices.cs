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
    /// 气体异常记录服务实现
    /// </summary>
    public class GasAbnormalServices : BaseServices<GasAbnormal>, IGasAbnormalServices
    {
        private IGasAbnormalRepository _gasAbnormalRepository;
        
        public GasAbnormalServices(IGasAbnormalRepository repository)
        {
            this._gasAbnormalRepository = repository;
            base.baseRepository = _gasAbnormalRepository;
        }

        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
            //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<GasAbnormal, bool>> whereExpression = PredicateBuilder.True<GasAbnormal>();
            //判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(GasAbnormal).GetProperty(item.QueryField);
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
            pagesResponse.Success((await _gasAbnormalRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber)).ToList());
            pagesResponse.count = (await _gasAbnormalRepository.Query(whereExpression)).Count();
            return pagesResponse;
        }
    }
}





