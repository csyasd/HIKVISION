
/****************************************************
 * 本文件由T4模板生成，请将本文件复制到YixiaoAdmin.Services类库中使用
 * 文件名：DeviceServices.cs

****************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YixiaoAdmin.IRepository;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;
using YixiaoAdmin.Models.ViewModels;
using YixiaoAdmin.Common;

namespace YixiaoAdmin.Services
{

    public partial class DeviceServices : BaseServices<Device>, IDeviceServices
    {
        public async Task<PagesResponse> QueryPagesExpand(QueryPageModel queryPageModel)
        {
            //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<Device, bool>> whereExpression = PredicateBuilder.True<Device>();

            // 判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(Device).GetProperty(item.QueryField);
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
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Name.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "Model")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Model.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "OnlineStatus")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.OnlineStatus == item.QueryStr);
                    }
                    else if (item.QueryField == "BelongToUnit")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.BelongToUnit.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "IP")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.IP.Contains(item.QueryStr));
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.CreateTime.Date == Convert.ToDateTime(item.QueryStr.Trim()));
                    }
                    else if (item.QueryField == "Id")
                    {
                        // 支持多个ID查询（用逗号分隔），用于设备权限过滤
                        if (item.QueryStr.Contains(","))
                        {
                            var ids = item.QueryStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (ids.Length > 0)
                            {
                                whereExpression = PredicateBuilder.And(whereExpression, (x) => ids.Contains(x.Id));
                            }
                        }
                        else
                        {
                            whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Id == item.QueryStr);
                        }
                    }
                }
            }
            
            //获取查询语句
            var query = await _DeviceRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber);

            //添加联合查询条件
            //query = query.Include(x => x.SubOrder).ThenInclude(x=>x.Order);

            pagesResponse.Success(query.ToList());
            pagesResponse.count = (await _DeviceRepository.Query(whereExpression)).Count();
            return pagesResponse;
        }
    }
}
