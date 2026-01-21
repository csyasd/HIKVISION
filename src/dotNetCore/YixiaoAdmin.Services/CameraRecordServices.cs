
/****************************************************
 * 本文件由T4模板生成，请将本文件复制到YixiaoAdmin.Services类库中使用
 * 文件名：CameraRecordServices.cs

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

    public partial class  CameraRecordServices:BaseServices<CameraRecord>, ICameraRecordServices
    {
       public async Task<PagesResponse> QueryPagesExpand(QueryPageModel queryPageModel)
        {
             //自定义分页Response
            PagesResponse pagesResponse = new PagesResponse();
            //初始化查询表达式
            Expression<Func<CameraRecord, bool>> whereExpression = PredicateBuilder.True<CameraRecord>();

            // 判断是否存在查询条件
            if (queryPageModel.Query != null)
            {
                foreach (QueryFieldModel item in queryPageModel.Query)
                {
                    //根据属性名获取属性
                    var property = typeof(CameraRecord).GetProperty(item.QueryField);
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
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.Name == item.QueryStr);
                    }
                    else if (item.QueryField == "CreateTime")
                    {
                        whereExpression = PredicateBuilder.And(whereExpression, (x) => x.CreateTime.Date == Convert.ToDateTime(item.QueryStr.Trim())); 
                    }
                    else if (item.QueryField == "CameraId")
                    {
                        // 支持多个摄像头ID查询（用逗号分隔），用于设备权限过滤
                        if (item.QueryStr.Contains(","))
                        {
                            var cameraIds = item.QueryStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (cameraIds.Length > 0)
                            {
                                whereExpression = PredicateBuilder.And(whereExpression, (x) => cameraIds.Contains(x.CameraId));
                            }
                        }
                        else
                        {
                            whereExpression = PredicateBuilder.And(whereExpression, (x) => x.CameraId == item.QueryStr);
                        }
                    }
                }
            }
            //获取查询语句
            var query = await _CameraRecordRepository.Query(whereExpression, queryPageModel.Orderby, queryPageModel.CurrentPage, queryPageModel.PageNumber);

            //添加联合查询条件
            //query = query.Include(x => x.SubOrder).ThenInclude(x=>x.Order);

            pagesResponse.Success(query.ToList());
            pagesResponse.count = (await _CameraRecordRepository.Query(whereExpression)).Count();
            return pagesResponse;
           
        }
    }
}
