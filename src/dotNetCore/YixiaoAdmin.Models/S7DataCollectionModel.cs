using System;

namespace YixiaoAdmin.Models
{
    /// <summary>
    /// 施工工单数据结构
    /// </summary>
    public class ConstructionOrder
    {
        /// <summary>
        /// 工人姓名数组（0-10），每个String占256字节
        /// </summary>
        public string[] Workers_Name { get; set; } = new string[11];

        /// <summary>
        /// 施工工单编号
        /// </summary>
        public int Construction_Order_No { get; set; }

        /// <summary>
        /// 智能手环编号数组（0-10）
        /// </summary>
        public int[] SmartBand_No { get; set; } = new int[11];

        /// <summary>
        /// 施工工单内容（String占256字节）
        /// </summary>
        public string Construction_Order_Content { get; set; }

        /// <summary>
        /// 工人状态数组（0-10）
        /// 0=未进入 1=申请进入 2=啥卡成功 3=进入 4=申请签出 5=已经签出
        /// </summary>
        public int[] Workers_Status { get; set; } = new int[11];

        /// <summary>
        /// 施工状态
        /// 0=未开始 1=工单开始 2=工单结束（所有人全部签出）
        /// </summary>
        public int Construction_Status { get; set; }

        /// <summary>
        /// 申请进入按钮数组（0-10）
        /// </summary>
        public bool[] Button_In { get; set; } = new bool[11];

        /// <summary>
        /// 申请签出按钮数组（0-10）
        /// </summary>
        public bool[] Button_Out { get; set; } = new bool[11];

        /// <summary>
        /// 最大心率数组（0-10）
        /// </summary>
        public int[] Maximum_HeartRate { get; set; } = new int[11];

        /// <summary>
        /// 最小心率数组（0-10）
        /// </summary>
        public int[] MInimum_HeartRate { get; set; } = new int[11];

        /// <summary>
        /// 实际心率数组（0-10）
        /// </summary>
        public int[] Heart_Rate { get; set; } = new int[11];
    }

    /// <summary>
    /// S7 PLC数据采集模型（施工工单数据）
    /// </summary>
    public class S7DataCollectionModel
    {
        /// <summary>
        /// 设备ID（根据IP对应）
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string DeviceIP { get; set; }

        /// <summary>
        /// 数据采集时间
        /// </summary>
        public DateTime CollectTime { get; set; }

        /// <summary>
        /// 是否连接成功
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 施工工单数组（0-10），共11个工单
        /// 偏移量：Constructoin_Order[0] = 0, Constructoin_Order[1] = 3190, ...
        /// </summary>
        public ConstructionOrder[] Construction_Order { get; set; }

        /// <summary>
        /// 构造函数，初始化工单数组
        /// </summary>
        public S7DataCollectionModel()
        {
            Construction_Order = new ConstructionOrder[11];
            for (int i = 0; i < 11; i++)
            {
                Construction_Order[i] = new ConstructionOrder();
            }
        }

        /// <summary>
        /// 工单开始按钮
        /// 偏移量：35090
        /// </summary>
        public bool ConstructionOrder_Start_PB { get; set; }

        /// <summary>
        /// 工单结束按钮
        /// 偏移量：35090.1
        /// </summary>
        public bool ConstructionOrder_Stop_PB { get; set; }

        /// <summary>
        /// 空工单（Construction_Order_Null）
        /// 偏移量：35092
        /// </summary>
        public ConstructionOrder Construction_Order_Null { get; set; } = new ConstructionOrder();

        /// <summary>
        /// 有害气体报警器在线
        /// 偏移量：38282
        /// </summary>
        public bool Hazardous_Gas_Alarm_Online { get; set; }

        /// <summary>
        /// GPS设备是否在线
        /// 偏移量：38282.1
        /// </summary>
        public bool GPS_online { get; set; }

        /// <summary>
        /// 气体报警数组（0-10），Real类型（float）
        /// 偏移量：Gas_Alarm[0] = 38284, Gas_Alarm[1] = 38288, ...
        /// </summary>
        public float[] Gas_Alarm { get; set; } = new float[11];

        /// <summary>
        /// GPS经度
        /// 偏移量：38328
        /// </summary>
        public float GPS_lon { get; set; }

        /// <summary>
        /// GPS纬度
        /// 偏移量：38332
        /// </summary>
        public float GPS_lat { get; set; }

        /// <summary>
        /// 设备ID（PLC中的Device_ID字段）
        /// </summary>
        public int Device_ID { get; set; }

        // ========== 以下字段保留用于向后兼容，但建议使用 Construction_Order 数组 ==========

        /// <summary>
        /// 工人姓名数组（0-10）- 已废弃，请使用 Construction_Order[].Workers_Name
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Workers_Name")]
        public string[] Workers_Name { get; set; } = new string[11];

        /// <summary>
        /// 施工工单编号 - 已废弃，请使用 Construction_Order[].Construction_Order_No
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Construction_Order_No")]
        public int Construction_Order_No { get; set; }

        /// <summary>
        /// 智能手环编号数组（0-10）- 已废弃，请使用 Construction_Order[].SmartBand_No
        /// </summary>
        [Obsolete("请使用 Construction_Order[].SmartBand_No")]
        public int[] SmartBand_No { get; set; } = new int[11];

        /// <summary>
        /// 施工工单内容 - 已废弃，请使用 Construction_Order[].Construction_Order_Content
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Construction_Order_Content")]
        public string Construction_Order_Content { get; set; }

        /// <summary>
        /// 工人状态数组（0-10）- 已废弃，请使用 Construction_Order[].Workers_Status
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Workers_Status")]
        public int[] Workers_Status { get; set; } = new int[11];

        /// <summary>
        /// 施工状态 - 已废弃，请使用 Construction_Order[].Construction_Status
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Construction_Status")]
        public int Construction_Status { get; set; }

        /// <summary>
        /// 申请进入按钮数组（0-10）- 已废弃，请使用 Construction_Order[].Button_In
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Button_In")]
        public bool[] Button_In { get; set; } = new bool[11];

        /// <summary>
        /// 申请签出按钮数组（0-10）- 已废弃，请使用 Construction_Order[].Button_Out
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Button_Out")]
        public bool[] Button_Out { get; set; } = new bool[11];

        /// <summary>
        /// 最大心率数组（0-10）- 已废弃，请使用 Construction_Order[].Maximum_HeartRate
        /// </summary>
        [Obsolete("请使用 Construction_Order[].Maximum_HeartRate")]
        public int[] Maximum_HeartRate { get; set; } = new int[11];

        /// <summary>
        /// 最小心率数组（0-10）- 已废弃，请使用 Construction_Order[].MInimum_HeartRate
        /// </summary>
        [Obsolete("请使用 Construction_Order[].MInimum_HeartRate")]
        public int[] MInimum_HeartRate { get; set; } = new int[11];

        /// <summary>
        /// 紧急呼叫数组（0-10）- 已废弃
        /// </summary>
        [Obsolete("此字段已废弃")]
        public bool[] Emergency_Call { get; set; } = new bool[11];

        /// <summary>
        /// 手环扫描器在线工作区域是否在线 - 已废弃
        /// </summary>
        [Obsolete("此字段已废弃")]
        public bool Bracelet_Scanner_WorkingArea { get; set; }

        /// <summary>
        /// 手环扫描器在线入口区域是否在线 - 已废弃
        /// </summary>
        [Obsolete("此字段已废弃")]
        public bool Bracelet_Scanner_Gate { get; set; }
    }

    /// <summary>
    /// S7 PLC连接配置
    /// </summary>
    public class S7PlcConfig
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
        /// 采集间隔（秒）
        /// </summary>
        public int CollectInterval { get; set; } = 5;
    }
}


