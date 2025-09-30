using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 摄像头实体类
    /// </summary>
    public class Camera : Entity
    {
        /// <summary>
        /// 摄像头型号
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 所属设备id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 所属设备
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }
    }
}
