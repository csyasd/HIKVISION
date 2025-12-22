using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 工人进出状态记录仓储实现
    /// </summary>
    public class WorkerStatusRecordRepository : BaseRepository<WorkerStatusRecord>, IWorkerStatusRecordRepository
    {
        public WorkerStatusRecordRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}
