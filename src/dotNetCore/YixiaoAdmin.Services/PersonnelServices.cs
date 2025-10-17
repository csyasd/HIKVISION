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
    /// 人员服务实现
    /// </summary>
    public class PersonnelServices : BaseServices<Personnel>, IPersonnelServices
    {
        private IPersonnelRepository _personnelRepository;
        
        public PersonnelServices(IPersonnelRepository repository)
        {
            this._personnelRepository = repository;
            base.baseRepository = _personnelRepository;
        }

        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
            //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<Personnel, bool>> whereExpression = PredicateBuilder.True<Personnel>();
            //判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(Personnel).GetProperty(item.QueryField);
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
                        whereExpression = PredicateBuilder.And<Personnel>(whereExpression, (x) => x.Name.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "EmployeeNumber")
                    {
                        whereExpression = PredicateBuilder.And<Personnel>(whereExpression, (x) => x.EmployeeNumber.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "FullName")
                    {
                        whereExpression = PredicateBuilder.And<Personnel>(whereExpression, (x) => x.FullName.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And<Personnel>(whereExpression, (x) => x.CreateTime.Date == Convert.ToDateTime(item.QueryStr.Trim()));
                    }
                    else if (item.QueryField == "Id")
                    {
                        whereExpression = PredicateBuilder.And<Personnel>(whereExpression, (x) => x.Id.Contains(item.QueryStr));
                    }
                }
            }
            pagesResponse.Success((await _personnelRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber)).ToList());
            pagesResponse.count = (await _personnelRepository.Query(whereExpression)).Count();
            return pagesResponse;
        }
    }
}
