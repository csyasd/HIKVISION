using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 手环仓储实现
    /// </summary>
    public class BraceletRepository : BaseRepository<Bracelet>, IBraceletRepository
    {
        public BraceletRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}
