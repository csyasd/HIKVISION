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

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 在线状态 (在线/离线)
        /// </summary>
        public string OnlineStatus { get; set; }
    }
}
