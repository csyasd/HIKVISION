using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 异常配置仓储实现
    /// </summary>
    public class AbnormalConfigRepository : BaseRepository<AbnormalConfig>, IAbnormalConfigRepository
    {
        public AbnormalConfigRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}

