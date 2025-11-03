using System;

namespace YixiaoAdmin.Models.ViewModels
{
    /// <summary>
    /// 手环DTO
    /// </summary>
    public class BraceletDto
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
        /// 手环编号
        /// </summary>
        public string BraceletNumber { get; set; }

        /// <summary>
        /// 手环记录
        /// </summary>
        public string BraceletRecord { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNumber { get; set; }

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
