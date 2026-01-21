using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 用户设备关联实体类
    /// </summary>
    public class UserDevice : Entity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        public Device Device { get; set; }
    }
}

