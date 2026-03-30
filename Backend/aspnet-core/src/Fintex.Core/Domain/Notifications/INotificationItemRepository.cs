using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Repository contract for user-notification queries and dedupe checks.
    /// </summary>
    public interface INotificationItemRepository : IRepository<NotificationItem, long>
    {
        Task<List<NotificationItem>> GetUserNotificationsAsync(long userId, int take, bool unreadOnly);

        Task<int> GetUnreadCountAsync(long userId);

        Task<bool> ExistsRecentAsync(long userId, string triggerKey, DateTime since);
    }
}
