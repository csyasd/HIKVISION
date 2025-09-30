using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 作业记录实体类
    /// </summary>
    public class WorkRecord : Entity
    {
        /// <summary>
        /// 所属设备id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 所属设备
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
    }
}
