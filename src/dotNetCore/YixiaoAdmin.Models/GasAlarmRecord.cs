using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 气体报警记录实体类
    /// </summary>
    public class GasAlarmRecord : Entity
    {
        /// <summary>
        /// 所属工单id
        /// </summary>
        public string WorkOrderId { get; set; }

        /// <summary>
        /// 所属工单
        /// </summary>
        public WorkOrder WorkOrder { get; set; }

        /// <summary>
        /// 第一种气体含量
        /// </summary>
        public float Gas1 { get; set; }

        /// <summary>
        /// 第二种气体含量
        /// </summary>
        public float Gas2 { get; set; }

        /// <summary>
        /// 第三种气体含量
        /// </summary>
        public float Gas3 { get; set; }

        /// <summary>
        /// 第四种气体含量
        /// </summary>
        public float Gas4 { get; set; }

        /// <summary>
        /// 第五种气体含量
        /// </summary>
        public float Gas5 { get; set; }

        /// <summary>
        /// 第六种气体含量
        /// </summary>
        public float Gas6 { get; set; }

        /// <summary>
        /// 第七种气体含量
        /// </summary>
        public float Gas7 { get; set; }

        /// <summary>
        /// 第八种气体含量
        /// </summary>
        public float Gas8 { get; set; }

        /// <summary>
        /// 第九种气体含量
        /// </summary>
        public float Gas9 { get; set; }

        /// <summary>
        /// 第十种气体含量
        /// </summary>
        public float Gas10 { get; set; }
    }
}

