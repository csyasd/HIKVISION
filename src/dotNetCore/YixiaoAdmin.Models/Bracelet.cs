using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 手环实体类
    /// </summary>
    public class Bracelet : Entity
    {
        /// <summary>
        /// 手环编号
        /// </summary>
        public string BraceletNumber { get; set; }

        /// <summary>
        /// 手环记录
        /// </summary>
        public string BraceletRecord { get; set; }
    }
}
