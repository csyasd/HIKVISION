using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YixiaoAdmin.Models;

namespace YixiaoAdmin.IServices
{
    public interface IUserDeviceServices : IBaseServices<UserDevice>
    {
        /// <summary>
        /// 根据用户ID获取设备ID列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<List<string>> GetDeviceIdsByUserId(string userId);

        /// <summary>
        /// 根据设备ID获取用户ID列表
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        Task<List<string>> GetUserIdsByDeviceId(string deviceId);

        /// <summary>
        /// 删除设备的所有用户关联
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        Task<bool> RemoveByDeviceId(string deviceId);

        /// <summary>
        /// 批量添加用户设备关联
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="userIds">用户ID列表</param>
        /// <returns></returns>
        Task<bool> AddUserDevices(string deviceId, List<string> userIds);
    }
}

