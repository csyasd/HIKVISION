using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;

namespace YixiaoAdmin.Repository
{
    /// <summary>
    /// 气体报警记录仓储实现
    /// </summary>
    public class GasAlarmRecordRepository : BaseRepository<GasAlarmRecord>, IGasAlarmRecordRepository
    {
        public GasAlarmRecordRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}

