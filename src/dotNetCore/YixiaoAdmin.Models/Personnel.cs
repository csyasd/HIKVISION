using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 人员实体类
    /// </summary>
    public class Personnel : Entity
    {
        /// <summary>
        /// 工号
        /// </summary>
        public string EmployeeNumber { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string FullName { get; set; }
    }
}
