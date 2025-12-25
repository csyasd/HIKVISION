using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 手环异常记录仓储实现
    /// </summary>
    public class BraceletAbnormalRepository : BaseRepository<BraceletAbnormal>, IBraceletAbnormalRepository
    {
        public BraceletAbnormalRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}

