
/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到YixiaoAdmin.Model/ViewModels文件夹中使用
 * 文件名：CameraRecordManagement.vue
************************************************************************/
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace YixiaoAdmin.Models.ViewModels
{
    

    public class CameraRecordDto : ViewDto
    {
    

    [Description("所属摄像头id")]
    public string CameraId { get; set; }
    

    [Description("所属摄像头")]
    public Camera Camera { get; set; }
    

    [Description("开始时间")]
    public string StartTime { get; set; }
    

    [Description("结束时间")]
    public string EndTime { get; set; }
    


    public string Id { get; set; }
    

    public string Name { get; set; }
    


    public string CreateUsername { get; set; }
    


    public DateTime CreateTime { get; set; }
    


    public string ModificationUsername { get; set; }
    


    public DateTime ModificationTime { get; set; }
    


    public string ParentId { get; set; }
    


    public List<Int32> SortCode { get; set; }
    


    public string Type { get; set; }
    


    public string State { get; set; }
    
    }
}  
