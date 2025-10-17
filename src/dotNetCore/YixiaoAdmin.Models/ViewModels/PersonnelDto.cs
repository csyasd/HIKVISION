using System;

namespace YixiaoAdmin.Models.ViewModels
{
    /// <summary>
    /// 人员DTO
    /// </summary>
    public class PersonnelDto
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
        /// 工号
        /// </summary>
        public string EmployeeNumber { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string FullName { get; set; }

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
