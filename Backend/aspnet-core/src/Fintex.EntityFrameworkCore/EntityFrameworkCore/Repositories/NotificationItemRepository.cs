using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.Investments.Notifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core queries for the user notification inbox.
    /// </summary>
    public class NotificationItemRepository : FintexRepositoryBase<NotificationItem, long>, INotificationItemRepository
    {
        public NotificationItemRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<NotificationItem>> GetUserNotificationsAsync(long userId, int take, bool unreadOnly)
        {
            var query = GetAll().Where(x => x.UserId == userId);
            if (unreadOnly)
            {
                query = query.Where(x => !x.IsRead);
            }

            return await query
                .OrderByDescending(x => x.OccurredAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId && !x.IsRead)
                .CountAsync();
        }

        public async Task<bool> ExistsRecentAsync(long userId, string triggerKey, DateTime since)
        {
            var normalizedKey = (triggerKey ?? string.Empty).Trim();

            return await GetAll()
                .Where(x => x.UserId == userId && x.TriggerKey == normalizedKey && x.OccurredAt >= since)
                .AnyAsync();
        }
    }
}
