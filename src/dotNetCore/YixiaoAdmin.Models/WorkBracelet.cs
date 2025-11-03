using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 作业手环实体类
    /// </summary>
    public class WorkBracelet : Entity
    {
        /// <summary>
        /// 工人姓名
        /// </summary>
        public string WorkerName { get; set; }

        /// <summary>
        /// 进离场状态 (0-离场, 1-进场)
        /// </summary>
        public string EntryExitStatus { get; set; }

        /// <summary>
        /// 紧急呼叫状态 (0-正常, 1-紧急呼叫)
        /// </summary>
        public string EmergencyCallStatus { get; set; }

        /// <summary>
        /// 进场时间
        /// </summary>
        public string EntryTime { get; set; }

        /// <summary>
        /// 离场时间
        /// </summary>
        public string ExitTime { get; set; }

        /// <summary>
        /// 心率
        /// </summary>
        public string HeartRate { get; set; }

        /// <summary>
        /// 所属手环id
        /// </summary>
        public string BraceletId { get; set; }

        /// <summary>
        /// 所属手环
        /// </summary>
        public Bracelet Bracelet { get; set; }
    }
}
