using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 设备实体类
    /// </summary>
    public class Device : Entity
    {
        /// <summary>
        /// 设备型号
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 出厂日期
        /// </summary>
        public string ManufactureDate { get; set; }

        /// <summary>
        /// 所属单位
        /// </summary>
        public string BelongToUnit { get; set; }
    }
}
