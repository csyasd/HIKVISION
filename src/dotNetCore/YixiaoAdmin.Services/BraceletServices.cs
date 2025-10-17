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
    /// 手环服务实现
    /// </summary>
    public class BraceletServices : BaseServices<Bracelet>, IBraceletServices
    {
        private IBraceletRepository _braceletRepository;
        
        public BraceletServices(IBraceletRepository repository)
        {
            this._braceletRepository = repository;
            base.baseRepository = _braceletRepository;
        }

        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
            //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<Bracelet, bool>> whereExpression = PredicateBuilder.True<Bracelet>();
            //判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(Bracelet).GetProperty(item.QueryField);
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
                        whereExpression = PredicateBuilder.And<Bracelet>(whereExpression, (x) => x.Name.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "BraceletNumber")
                    {
                        whereExpression = PredicateBuilder.And<Bracelet>(whereExpression, (x) => x.BraceletNumber.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "BraceletRecord")
                    {
                        whereExpression = PredicateBuilder.And<Bracelet>(whereExpression, (x) => x.BraceletRecord.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And<Bracelet>(whereExpression, (x) => x.CreateTime.Date == Convert.ToDateTime(item.QueryStr.Trim()));
                    }
                    else if (item.QueryField == "Id")
                    {
                        whereExpression = PredicateBuilder.And<Bracelet>(whereExpression, (x) => x.Id.Contains(item.QueryStr));
                    }
                }
            }
            pagesResponse.Success((await _braceletRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber)).ToList());
            pagesResponse.count = (await _braceletRepository.Query(whereExpression)).Count();
            return pagesResponse;
        }
    }
}
