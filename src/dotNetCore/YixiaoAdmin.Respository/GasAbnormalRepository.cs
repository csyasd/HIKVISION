using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 气体异常记录仓储实现
    /// </summary>
    public class GasAbnormalRepository : BaseRepository<GasAbnormal>, IGasAbnormalRepository
    {
        public GasAbnormalRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}

