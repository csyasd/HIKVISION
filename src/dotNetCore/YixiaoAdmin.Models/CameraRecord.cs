using System;
using System.ComponentModel;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 摄像头记录实体类
    /// </summary>
    public class CameraRecord : Entity
    {
        /// <summary>
        /// 所属摄像头id
        /// </summary>
        public string CameraId { get; set; }

        /// <summary>
        /// 所属摄像头
        /// </summary>
        public Camera Camera { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
    }
}
