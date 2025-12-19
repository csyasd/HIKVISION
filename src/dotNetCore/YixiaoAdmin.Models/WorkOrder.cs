using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 施工工单类
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
        /// 工单状态 (0-未开始, 1-工单开始, 2-工单结束)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 有毒气体报警在线状态
        /// </summary>
        public bool ToxicGasAlarmOnlineStatus { get; set; }
    }
}
