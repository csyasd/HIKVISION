using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 设备实体类
    /// </summary>
    public class Device : Entity
    {
        /// <summary>
        /// 设备型号
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 出厂日期
        /// </summary>
        public string ManufactureDate { get; set; }

        /// <summary>
        /// 所属单位
        /// </summary>
        public string BelongToUnit { get; set; }

        /// <summary>
        /// 手环扫描器-作业区
        /// </summary>
        public string WorkAreaScanner { get; set; }

        /// <summary>
        /// 手环扫描器-人口区
        /// </summary>
        public string EntryAreaScanner { get; set; }

        /// <summary>
        /// GPS在线状态
        /// </summary>
        public string GpsOnlineStatus { get; set; }

        /// <summary>
        /// GPS经度
        /// </summary>
        public string GpsLongitude { get; set; }

        /// <summary>
        /// GPS纬度
        /// </summary>
        public string GpsLatitude { get; set; }

        /// <summary>
        /// 有毒气体报警在线状态
        /// </summary>
        public string ToxicGasAlarmOnlineStatus { get; set; }
    }
}
