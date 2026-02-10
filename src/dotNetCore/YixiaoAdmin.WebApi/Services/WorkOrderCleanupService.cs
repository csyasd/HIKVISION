using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;

namespace YixiaoAdmin.WebApi.Services
{
    /// <summary>
    /// 后台工单清理服务：确保每个设备只保留最新的一份活动工单
    /// </summary>
    public class WorkOrderCleanupService : BackgroundService
    {
        private readonly ILogger<WorkOrderCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WorkOrderCleanupService(
            ILogger<WorkOrderCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("工单自动清理服务启动...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoCleanup(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行工单清理时出错");
                }

                // 每分钟执行一次
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task DoCleanup(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var workOrderService = scope.ServiceProvider.GetRequiredService<IWorkOrderServices>();
                
                // 获取所有未结束的工单 (Status != 2)
                var activeWorkOrders = await workOrderService.Query(wo => wo.Status != 2);
                var workOrderList = activeWorkOrders.ToList();

                if (workOrderList.Count == 0) return;

                // 按设备分组
                var groups = workOrderList.GroupBy(wo => wo.DeviceId);

                foreach (var group in groups)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    // 如果该设备有超过一个活动工单
                    if (group.Count() > 1)
                    {
                        // 按创建时间降序排列，保留最新的一个
                        var sortedOrders = group.OrderByDescending(wo => wo.CreateTime).ToList();
                        var latestOrder = sortedOrders.First();
                        var ordersToClose = sortedOrders.Skip(1);

                        foreach (var oldOrder in ordersToClose)
                        {
                            _logger.LogInformation($"[自动清理] 设备 {oldOrder.DeviceId} 发现旧工单 {oldOrder.Code}，正在将其关闭...");
                            oldOrder.Status = 2; // 设置为结束状态
                            await workOrderService.Update(oldOrder);
                        }
                    }
                }
            }
        }
    }
}
