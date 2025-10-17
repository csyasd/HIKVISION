using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.Repository;

namespace YixiaoAdmin.Respository
{
    /// <summary>
    /// 人员仓储实现
    /// </summary>
    public class PersonnelRepository : BaseRepository<Personnel>, IPersonnelRepository
    {
        public PersonnelRepository(YixiaoAdminContext context) : base(context)
        {
        }
    }
}
