namespace S7Demo.Models
{
    /// <summary>
    /// GPS坐标数据模型
    /// </summary>
    public class GpsData
    {
        /// <summary>
        /// GPS经度
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// GPS纬度
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 读取时间
        /// </summary>
        public DateTime ReadTime { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected { get; set; }
    }

    /// <summary>
    /// PLC连接配置
    /// </summary>
    public class PlcConfig
    {
        /// <summary>
        /// PLC IP地址
        /// </summary>
        public string IpAddress { get; set; } = "192.168.0.1";

        /// <summary>
        /// 机架号
        /// </summary>
        public int Rack { get; set; } = 0;

        /// <summary>
        /// 槽位号
        /// </summary>
        public int Slot { get; set; } = 1;

        /// <summary>
        /// 数据块编号
        /// </summary>
        public int DataBlockNumber { get; set; } = 1;

        /// <summary>
        /// 连接超时时间（毫秒）
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;
    }

    /// <summary>
    /// API响应模型
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }
    }
}
