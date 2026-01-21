using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YixiaoAdmin.IRepository;
using YixiaoAdmin.IServices;
using YixiaoAdmin.Models;

namespace YixiaoAdmin.Services
{
    public class UserDeviceServices : BaseServices<UserDevice>, IUserDeviceServices
    {
        private readonly IUserDeviceRepository _userDeviceRepository;

        public UserDeviceServices(IUserDeviceRepository userDeviceRepository)
        {
            _userDeviceRepository = userDeviceRepository;
            baseRepository = userDeviceRepository;
        }

        public async Task<List<string>> GetDeviceIdsByUserId(string userId)
        {
            return await _userDeviceRepository.GetDeviceIdsByUserId(userId);
        }

        public async Task<List<string>> GetUserIdsByDeviceId(string deviceId)
        {
            return await _userDeviceRepository.GetUserIdsByDeviceId(deviceId);
        }

        public async Task<bool> RemoveByDeviceId(string deviceId)
        {
            return await _userDeviceRepository.RemoveByDeviceId(deviceId);
        }

        public async Task<bool> AddUserDevices(string deviceId, List<string> userIds)
        {
            return await _userDeviceRepository.AddUserDevices(deviceId, userIds);
        }
    }
}

