using System;

namespace YixiaoAdmin.Models.ViewModels
{
    /// <summary>
    /// 作业手环DTO
    /// </summary>
    public class WorkBraceletDto
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
        /// 工人姓名
        /// </summary>
        public string WorkerName { get; set; }

        /// <summary>
        /// 进离场状态 (0-离场, 1-进场)
        /// </summary>
        public string EntryExitStatus { get; set; }

        /// <summary>
        /// 进离场状态描述
        /// </summary>
        public string EntryExitStatusText { get; set; }

        /// <summary>
        /// 紧急呼叫状态 (0-正常, 1-紧急呼叫)
        /// </summary>
        public string EmergencyCallStatus { get; set; }

        /// <summary>
        /// 紧急呼叫状态描述
        /// </summary>
        public string EmergencyCallStatusText { get; set; }

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
        public BraceletDto Bracelet { get; set; }

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
