namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 作业记录实体类
    /// </summary>
    public class WorkRecord : Entity
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
        /// 所属作业手环id
        /// </summary>
        public string WorkBraceletId { get; set; }

        /// <summary>
        /// 所属作业手环
        /// </summary>
        public WorkBracelet WorkBracelet { get; set; }

        /// <summary>
        /// 心率
        /// </summary>
        public string HeartRate { get; set; }

        /// <summary>
        /// 进离场状态 (0-离场, 1-进场)
        /// </summary>
        public string EntryExitStatus { get; set; }

        /// <summary>
        /// 紧急呼叫状态 (0-正常, 1-紧急呼叫)
        /// </summary>
        public string EmergencyCallStatus { get; set; }
    }
}
