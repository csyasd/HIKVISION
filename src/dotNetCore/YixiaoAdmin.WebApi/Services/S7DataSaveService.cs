using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;
using YixiaoAdmin.WebApi.Services;

namespace YixiaoAdmin.WebApi.Services
{
    /// <summary>
    /// S7 PLC数据保存服务（将采集的数据保存到数据库）
    /// </summary>
    public class S7DataSaveService
    {
        private readonly ILogger<S7DataSaveService> _logger;
        private readonly IWorkOrderServices _workOrderService;
        private readonly IDeviceServices _deviceService;
        private readonly IWorkBraceletServices _workBraceletService;
        private readonly IWorkRecordServices _workRecordService;
        private readonly IGasAlarmRecordServices _gasAlarmRecordService;

        public S7DataSaveService(
            ILogger<S7DataSaveService> logger,
            IWorkOrderServices workOrderService,
            IDeviceServices deviceService,
            IWorkBraceletServices workBraceletService,
            IWorkRecordServices workRecordService,
            IGasAlarmRecordServices gasAlarmRecordService)
        {
            _logger = logger;
            _workOrderService = workOrderService;
            _deviceService = deviceService;
            _workBraceletService = workBraceletService;
            _workRecordService = workRecordService;
            _gasAlarmRecordService = gasAlarmRecordService;
        }

        /// <summary>
        /// 保存采集的数据到数据库
        /// </summary>
        public async Task SaveDataToDatabase(S7DataCollectionModel data, Device device)
        {
            if (data == null || !data.IsConnected)
            {
                _logger.LogWarning("[数据保存] 数据为空或设备未连接，跳过保存");
                return;
            }

            try
            {
                _logger.LogDebug($"[数据保存] 开始保存数据到数据库 - DeviceId: {data.DeviceId}, 采集时间: {data.CollectTime:yyyy-MM-dd HH:mm:ss.fff}");

                // 只处理工单索引1的数据
                const int orderIndex = 1;
                var order = data.Construction_Order[orderIndex];

                // 如果工单号为空或为0，跳过处理
                if (order.Construction_Order_No <= 0)
                {
                    _logger.LogDebug($"[数据保存] 工单号为空或为0，跳过处理");
                    return;
                }

                // 1. 处理工单
                var workOrder = await ProcessWorkOrder(order, device, data);
                if (workOrder == null)
                {
                    _logger.LogWarning($"[数据保存] 工单处理失败，跳过后续处理");
                    return;
                }

                // 2. 更新设备数据
                await UpdateDeviceData(device, data);

                // 3. 处理作业手环管理
                await ProcessWorkBracelets(order, workOrder, data);

                // 4. 处理作业记录管理
                await ProcessWorkRecords(order, workOrder, data);

                // 5. 处理气体报警记录
                await ProcessGasAlarmRecords(workOrder, data, device);

                _logger.LogDebug($"[数据保存] 数据保存完成 - DeviceId: {data.DeviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[数据保存异常] 保存数据到数据库时发生异常 - DeviceId: {data.DeviceId}");
            }
        }

        /// <summary>
        /// 检查字符串是否有效（不为null、空字符串或"?"）
        /// </summary>
        private bool IsValidString(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim() != "?";
        }

        /// <summary>
        /// 处理工单（比对、创建或更新）
        /// </summary>
        private async Task<WorkOrder> ProcessWorkOrder(ConstructionOrder order, Device device, S7DataCollectionModel data)
        {
            try
            {
                var orderNo = order.Construction_Order_No.ToString();
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 查询是否存在该工单（根据编号和设备ID）
                var existingOrders = await _workOrderService.Query();
                var existingOrder = existingOrders.FirstOrDefault(o => 
                    o.Code == orderNo && o.DeviceId == device.Id);

                WorkOrder workOrder;
                bool isNewOrder = existingOrder == null;

                if (isNewOrder)
                {
                    // 创建新工单
                    _logger.LogInformation($"[工单处理] 创建新工单 - 编号: {orderNo}, 设备: {device.Name}");
                    workOrder = new WorkOrder
                    {
                        DeviceId = device.Id,
                        Code = orderNo,
                        Content = IsValidString(order.Construction_Order_Content) ? order.Construction_Order_Content : string.Empty,
                        Status = order.Construction_Status,
                        ToxicGasAlarmOnlineStatus = data.Hazardous_Gas_Alarm_Online,
                        StartTime = order.Construction_Status == 1 ? currentTime : null,
                        EndTime = null
                    };

                    await _workOrderService.Add(workOrder);
                    _logger.LogDebug($"[工单处理] 新工单创建成功 - ID: {workOrder.Id}, 状态: {workOrder.Status}");
                }
                else
                {
                    // 更新现有工单
                    workOrder = existingOrder;
                    var oldStatus = workOrder.Status;
                    var newStatus = order.Construction_Status;

                    _logger.LogDebug($"[工单处理] 更新现有工单 - 编号: {orderNo}, 旧状态: {oldStatus}, 新状态: {newStatus}");

                    // 更新工单内容（只有当新内容有效时才更新）
                    if (IsValidString(order.Construction_Order_Content))
                    {
                        workOrder.Content = order.Construction_Order_Content;
                    }
                    workOrder.ToxicGasAlarmOnlineStatus = data.Hazardous_Gas_Alarm_Online;

                    // 处理状态变化
                    if (oldStatus == 0 && newStatus == 1)
                    {
                        // 状态从0变为1，设置开始时间
                        workOrder.StartTime = currentTime;
                        _logger.LogInformation($"[工单处理] 工单状态变化 0→1，设置开始时间: {currentTime}");
                    }
                    else if (oldStatus == 1 && newStatus == 2)
                    {
                        // 状态从1变为2，设置结束时间
                        workOrder.EndTime = currentTime;
                        _logger.LogInformation($"[工单处理] 工单状态变化 1→2，设置结束时间: {currentTime}");
                    }

                    workOrder.Status = newStatus;
                    await _workOrderService.Update(workOrder);
                    _logger.LogDebug($"[工单处理] 工单更新成功 - ID: {workOrder.Id}");
                }

                return workOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[工单处理异常] 处理工单时发生异常 - 工单号: {order.Construction_Order_No}");
                return null;
            }
        }

        /// <summary>
        /// 更新设备数据
        /// </summary>
        private async Task UpdateDeviceData(Device device, S7DataCollectionModel data)
        {
            try
            {
                _logger.LogDebug($"[设备更新] 更新设备数据 - 设备: {device.Name}");

                // 更新GPS在线状态
                device.GpsOnlineStatus = data.GPS_online ? "在线" : "离线";

                // 更新GPS经纬度
                if (data.GPS_lon != 0 || data.GPS_lat != 0)
                {
                    device.GpsLongitude = data.GPS_lon.ToString("F6");
                    device.GpsLatitude = data.GPS_lat.ToString("F6");
                }

                // 更新气体报警状态（如果有报警，更新为"报警"，否则为"正常"）
                bool hasGasAlarm = data.Gas_Alarm.Any(g => g > 0);
                device.ToxicGasAlarmOnlineStatus = hasGasAlarm ? "报警" : (data.Hazardous_Gas_Alarm_Online ? "在线" : "离线");

                // 更新在线状态
                device.OnlineStatus = data.IsConnected ? "在线" : "离线";

                await _deviceService.Update(device);
                _logger.LogDebug($"[设备更新] 设备数据更新成功 - 设备: {device.Name}, 在线状态: {device.OnlineStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[设备更新异常] 更新设备数据时发生异常 - 设备: {device.Name}");
            }
        }

        /// <summary>
        /// 处理作业手环管理（更新或添加）
        /// </summary>
        private async Task ProcessWorkBracelets(ConstructionOrder order, WorkOrder workOrder, S7DataCollectionModel data)
        {
            try
            {
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 获取当前工单的所有手环记录，避免在循环中重复查询
                var braceletQuery = await _workBraceletService.Query(b => b.WorkOrderId == workOrder.Id);
                var currentBracelets = braceletQuery.ToList();

                for (int i = 1; i <= 10; i++)
                {
                    var rawWorkerName = order.Workers_Name[i];
                    // 跳过无效的工人姓名（null、空字符串或"?"）
                    if (!IsValidString(rawWorkerName))
                    {
                        _logger.LogTrace($"[手环处理] 跳过无效工人姓名 - 索引: {i}, 值: '{rawWorkerName}'");
                        continue; // 跳过无效工人
                    }
                    
                    // 去除首尾空格，确保匹配准确
                    var workerName = rawWorkerName.Trim();

                    var workerStatus = order.Workers_Status[i];
                    var heartRate = order.Heart_Rate[i];

                    // 在已加载的列表中查找
                    var existingBracelet = currentBracelets.FirstOrDefault(b => b.WorkerName == workerName);

                    WorkBracelet bracelet;
                    bool isNewBracelet = existingBracelet == null;

                    if (isNewBracelet)
                    {
                        // 创建新手环记录
                        _logger.LogDebug($"[手环处理] 创建新手环记录 - 工人: {workerName}, 工单: {workOrder.Code}, 状态: {workerStatus}");
                        bracelet = new WorkBracelet
                        {
                            WorkerName = workerName,
                            WorkOrderId = workOrder.Id,
                            HeartRate = heartRate > 0 ? heartRate.ToString() : null,
                            EntryExitStatus = workerStatus.ToString(),
                            EntryTime = workerStatus == 2 ? currentTime : null, // 状态2=刷卡成功/进场
                            ExitTime = workerStatus == 5 ? currentTime : null // 状态5=已签出/离场
                        };

                        await _workBraceletService.Add(bracelet);
                        _logger.LogDebug($"[手环处理] 新手环记录创建成功 - 工人: {workerName}, 状态: {workerStatus}");
                    }
                    else
                    {
                        // 更新现有手环记录
                        bracelet = existingBracelet;
                        var oldStatus = int.TryParse(bracelet.EntryExitStatus, out var oldStatusInt) ? oldStatusInt : -1;

                        _logger.LogDebug($"[手环处理] 更新手环记录 - 工人: {workerName}, 旧状态: {oldStatus}, 新状态: {workerStatus}");

                        // 更新心率
                        bracelet.HeartRate = heartRate > 0 ? heartRate.ToString() : bracelet.HeartRate;
                        
                        // 处理进场时间：如果状态变为2（刷卡成功/进场），设置进场时间
                        if (oldStatus != 2 && workerStatus == 2)
                        {
                            bracelet.EntryTime = currentTime;
                            _logger.LogDebug($"[手环处理] 工人进场，设置进场时间: {currentTime}");
                        }

                        // 处理离场时间：如果状态变为5（已签出/离场），设置离场时间
                        if (oldStatus != 5 && workerStatus == 5)
                        {
                            bracelet.ExitTime = currentTime;
                            _logger.LogDebug($"[手环处理] 工人离场，设置离场时间: {currentTime}");
                        }

                        // 更新状态
                        bracelet.EntryExitStatus = workerStatus.ToString();

                        await _workBraceletService.Update(bracelet);
                        _logger.LogDebug($"[手环处理] 手环记录更新成功 - 工人: {workerName}, 状态: {workerStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[手环处理异常] 处理作业手环时发生异常");
            }
        }

        /// <summary>
        /// 处理作业记录管理（每次添加记录）
        /// </summary>
        private async Task ProcessWorkRecords(ConstructionOrder order, WorkOrder workOrder, S7DataCollectionModel data)
        {
            try
            {
                // 如果工单状态是2（工单结束），则所有记录的状态都是5（已签出）
                var recordStatus = workOrder.Status == 2 ? "5" : null;

                for (int i = 1; i <= 10; i++)
                {
                    var workerName = order.Workers_Name[i];
                    // 跳过无效的工人姓名（null、空字符串或"?"）
                    if (!IsValidString(workerName))
                    {
                        continue; // 跳过无效工人
                    }

                    var workerStatus = order.Workers_Status[i];
                    var heartRate = order.Heart_Rate[i];

                    // 每次采集都添加新记录
                    var workRecord = new WorkRecord
                    {
                        WorkOrderId = workOrder.Id,
                        WorkerName = workerName,
                        HeartRate = heartRate > 0 ? heartRate.ToString() : null,
                        EntryExitStatus = recordStatus ?? workerStatus.ToString() // 如果工单结束，状态为5
                    };

                    await _workRecordService.Add(workRecord);
                    _logger.LogTrace($"[记录处理] 添加作业记录 - 工人: {workerName}, 状态: {workRecord.EntryExitStatus}, 心率: {heartRate}");
                }

                _logger.LogDebug($"[记录处理] 作业记录添加完成 - 工单: {workOrder.Code}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[记录处理异常] 处理作业记录时发生异常");
            }
        }

        /// <summary>
        /// 处理气体报警记录（每次添加记录）
        /// </summary>
        private async Task ProcessGasAlarmRecords(WorkOrder workOrder, S7DataCollectionModel data, Device device)
        {
            try
            {
                // 每次采集都添加气体报警记录（即使数据为0）
                var gasRecord = new GasAlarmRecord
                {
                    WorkOrderId = workOrder.Id,
                    Gas1 = data.Gas_Alarm[1],
                    Gas2 = data.Gas_Alarm[2],
                    Gas3 = data.Gas_Alarm[3],
                    Gas4 = data.Gas_Alarm[4],
                    Gas5 = data.Gas_Alarm[5],
                    Gas6 = data.Gas_Alarm[6],
                    Gas7 = data.Gas_Alarm[7],
                    Gas8 = data.Gas_Alarm[8],
                    Gas9 = data.Gas_Alarm[9],
                    Gas10 = data.Gas_Alarm[10]
                };

                await _gasAlarmRecordService.Add(gasRecord);
                _logger.LogDebug($"[气体报警] 气体报警记录添加成功 - 工单: {workOrder.Code}, Gas1-10: [{string.Join(", ", data.Gas_Alarm.Take(10).Select(g => g.ToString("F2")))}]");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[气体报警异常] 处理气体报警记录时发生异常");
            }
        }
    }
}

