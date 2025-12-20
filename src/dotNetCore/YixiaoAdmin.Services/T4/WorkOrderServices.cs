
/****************************************************
 * 本文件由T4模板生成，重新生成T4模板后会导致代码丢失
 * 如需修改请使用partial关键词
 * 文件名：WorkRecordServices.cs
****************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using YixiaoAdmin.Models;
using YixiaoAdmin.IServices;
using YixiaoAdmin.IRepository;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;
using YixiaoAdmin.Common;

namespace YixiaoAdmin.Services
{

    public partial class  WorkOrderServices:BaseServices<WorkOrder>, IWorkOrderServices
    {
        private IWorkOrderRepository _WorkRecordRepository;
        public WorkOrderServices(IWorkOrderRepository WorkRecordRepository)
        {
            this._WorkRecordRepository = WorkRecordRepository;
            base.baseRepository = _WorkRecordRepository;
        }
        public async Task<PagesResponse> QueryPages(QueryPageModel queryPageModel)
        {
             //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<WorkOrder, bool>> whereExpression = PredicateBuilder.True<WorkOrder>();
            //判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(WorkOrder).GetProperty(item.QueryField);
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

                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Name .Contains( item.QueryStr));
                    }

                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.CreateTime.Date==Convert.ToDateTime(item.QueryStr.Trim()));
                    }

                    else if (item.QueryField == "Id")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Id.Contains(item.QueryStr));
                    }

                    else if (item.QueryField == "DeviceId")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.DeviceId == item.QueryStr);
                    }

                    else if (item.QueryField == "Code")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Code != null && x.Code.Contains(item.QueryStr));
                    }

                    else if (item.QueryField == "Content")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Content != null && x.Content.Contains(item.QueryStr));
                    }

                }
            }
            pagesResponse.Success((await _WorkRecordRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber)).ToList());
            pagesResponse.count = (await _WorkRecordRepository.Query(whereExpression)).Count();
            return pagesResponse;
           
        }
    }
}
