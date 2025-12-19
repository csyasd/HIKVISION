using System;

namespace YixiaoAdmin.Models.ViewModels
{
    /// <summary>
    /// 作业记录DTO
    /// </summary>
    public class WorkRecordDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所属施工工单id
        /// </summary>
        public string WorkOrderId { get; set; }

        /// <summary>
        /// 所属施工工单
        /// </summary>
        public WorkOrderDto WorkOrder { get; set; }

        /// <summary>
        /// 心率
        /// </summary>
        public string HeartRate { get; set; }

        /// <summary>
        /// 进离场状态 (0-离场, 1-进场)
        /// </summary>
        public string EntryExitStatus { get; set; }

        /// <summary>
        /// 进离场状态描述
        /// </summary>
        public string EntryExitStatusText { get; set; }

        /// <summary>
        /// 工人姓名
        /// </summary>
        public string WorkerName { get; set; }

        /// <summary>
        /// 创建用户名
        /// </summary>
        public string CreateUsername { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改用户名
        /// </summary>
        public string ModificationUsername { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModificationTime { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int? SortCode { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }
    }
}
