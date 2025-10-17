using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 作业记录实体类
    /// </summary>
    public class WorkOrder : Entity
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

        /// <summary>
        /// 编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 气体警报
        /// </summary>
        public bool GasAlarm { get; set; }

        /// <summary>
        /// 有毒气体报警在线状态
        /// </summary>
        public bool ToxicGasAlarmOnlineStatus { get; set; }
    }
}
