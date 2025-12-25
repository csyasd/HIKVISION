using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 异常检测配置实体类
    /// </summary>
    public class AbnormalConfig : Entity
    {
        /// <summary>
        /// 配置类型：HeartRate（心率）或Gas（气体）
        /// </summary>
        public string ConfigType { get; set; }

        /// <summary>
        /// 配置名称：HeartRate, Gas1, Gas2, Gas3, Gas4
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public float MaxValue { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}

