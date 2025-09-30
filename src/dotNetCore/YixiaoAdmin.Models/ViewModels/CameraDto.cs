
/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到YixiaoAdmin.Model/ViewModels文件夹中使用
 * 文件名：CameraManagement.vue
************************************************************************/
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace YixiaoAdmin.Models.ViewModels
{
    

    public class CameraDto : ViewDto
    {
    

    [Description("摄像头型号")]
    public string Model { get; set; }
    

    [Description("所属设备id")]
    public string DeviceId { get; set; }
    

    [Description("所属设备")]
    public Device Device { get; set; }
    

    [Description("设备编码")]
    public string DeviceCode { get; set; }
    

    [Description("IP地址")]
    public string IP { get; set; }
    


    public string Id { get; set; }
    

    [Description("名称")]
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
