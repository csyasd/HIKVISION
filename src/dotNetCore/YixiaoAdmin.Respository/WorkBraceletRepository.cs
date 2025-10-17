using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 作业手环仓储实现
    /// </summary>
    public class WorkBraceletRepository : BaseRepository<WorkBracelet>, IWorkBraceletRepository
    {
        public WorkBraceletRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}
