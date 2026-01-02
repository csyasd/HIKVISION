using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 手环异常记录实体类
    /// </summary>
    public class BraceletAbnormal : Entity
    {
        /// <summary>
        /// 所属施工工单id
        /// </summary>
        public string WorkOrderId { get; set; }

        /// <summary>
        /// 所属施工工单
        /// </summary>
        public WorkOrder WorkOrder { get; set; }

        /// <summary>
        /// 心率
        /// </summary>
        public string HeartRate { get; set; }

        /// <summary>
        /// 进离场状态 (0-未进入, 1-申请进入, 2-刷卡成功, 3-进入, 4-申请签出, 5-已经签出)
        /// </summary>
        public string EntryExitStatus { get; set; }

        /// <summary>
        /// 工人姓名
        /// </summary>
        public string WorkerName { get; set; }
    }
}



