using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.Investments.Notifications;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core queries for notification alert rules.
    /// </summary>
    public class NotificationAlertRuleRepository : FintexRepositoryBase<NotificationAlertRule, long>, INotificationAlertRuleRepository
    {
        public NotificationAlertRuleRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<NotificationAlertRule>> GetActivePriceAlertsAsync(string symbol, MarketDataProvider provider)
        {
            var normalizedSymbol = NormalizeSymbol(symbol);

            return await GetAll()
                .Where(x => x.IsActive && x.Symbol == normalizedSymbol && x.Provider == provider)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<List<NotificationAlertRule>> GetUserRulesAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<NotificationAlertRule> GetUserRuleAsync(long userId, long ruleId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId && x.Id == ruleId)
                .FirstOrDefaultAsync();
        }

        private static string NormalizeSymbol(string symbol)
        {
            return (symbol ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
