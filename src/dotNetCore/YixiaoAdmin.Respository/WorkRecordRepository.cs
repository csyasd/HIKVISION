using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 作业记录仓储实现
    /// </summary>
    public class WorkRecordRepository : BaseRepository<WorkRecord>, IWorkRecordRepository
    {
        public WorkRecordRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}
