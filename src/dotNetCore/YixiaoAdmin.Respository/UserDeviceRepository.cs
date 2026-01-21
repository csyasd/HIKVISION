using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YixiaoAdmin.EntityFrameworkCore;
using YixiaoAdmin.IRepository;
using YixiaoAdmin.Models;
using YixiaoAdmin.Common;

namespace YixiaoAdmin.Repository
{
    public class UserDeviceRepository : BaseRepository<UserDevice>, IUserDeviceRepository
    {
        public UserDeviceRepository(YixiaoAdminContext yixiaoAdminContext) : base(yixiaoAdminContext)
        {
        }

        public async Task<List<string>> GetDeviceIdsByUserId(string userId)
        {
            var query = await base.Query(x => x.UserId == userId);
            return await query.Select(x => x.DeviceId).ToListAsync();
        }

        public async Task<List<string>> GetUserIdsByDeviceId(string deviceId)
        {
            var query = await base.Query(x => x.DeviceId == deviceId);
            return await query.Select(x => x.UserId).ToListAsync();
        }

        public async Task<bool> RemoveByDeviceId(string deviceId)
        {
            var userDevices = await base.Query(x => x.DeviceId == deviceId);
            var list = await userDevices.ToListAsync();
            if (list.Any())
            {
                db.Set<UserDevice>().RemoveRange(list);
                var result = await db.SaveChangesAsync();
                return result > 0;
            }
            return true;
        }

        public async Task<bool> AddUserDevices(string deviceId, List<string> userIds)
        {
            // 先删除该设备的所有现有关联（无论userIds是否为空）
            await RemoveByDeviceId(deviceId);

            // 如果userIds为空或null，只删除关联，不添加新的
            if (userIds == null || !userIds.Any())
            {
                return true;
            }

            // 添加新的关联
            var userDevices = userIds.Select(userId => new UserDevice
            {
                Id = Guid.NewGuid().ToString(),
                DeviceId = deviceId,
                UserId = userId,
                Name = $"UserDevice_{deviceId}_{userId}",
                CreateTime = DateTime.Now,
                CreateUsername = "system"
            }).ToList();

            db.Set<UserDevice>().AddRange(userDevices);
            var result = await db.SaveChangesAsync();
            return result > 0;
        }
    }
}

