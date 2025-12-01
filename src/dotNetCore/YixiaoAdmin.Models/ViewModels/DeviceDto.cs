
/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到YixiaoAdmin.Model/ViewModels文件夹中使用
 * 文件名：DeviceManagement.vue
************************************************************************/
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace YixiaoAdmin.Models.ViewModels
{
    

    public class DeviceDto : ViewDto
    {
    

    [Description("设备型号")]
    public string Model { get; set; }
    

    [Description("出厂日期")]
    public string ManufactureDate { get; set; }
    

    [Description("所属单位")]
    public string BelongToUnit { get; set; }
    

    [Description("手环扫描器-作业区")]
    public string WorkAreaScanner { get; set; }
    

    [Description("手环扫描器-人口区")]
    public string EntryAreaScanner { get; set; }
    

    [Description("GPS在线状态")]
    public string GpsOnlineStatus { get; set; }
    

    [Description("GPS经度")]
    public string GpsLongitude { get; set; }
    

    [Description("GPS纬度")]
    public string GpsLatitude { get; set; }
    

    [Description("有毒气体报警在线状态")]
    public string ToxicGasAlarmOnlineStatus { get; set; }
    

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
