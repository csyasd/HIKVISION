using YixiaoAdmin.IRepository;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;

namespace YixiaoAdmin.Services
{
    /// <summary>
    /// 异常配置服务实现
    /// </summary>
    public class AbnormalConfigServices : BaseServices<AbnormalConfig>, IAbnormalConfigServices
    {
        private IAbnormalConfigRepository _abnormalConfigRepository;
        
        public AbnormalConfigServices(IAbnormalConfigRepository repository)
        {
            this._abnormalConfigRepository = repository;
            base.baseRepository = _abnormalConfigRepository;
        }
    }
}



