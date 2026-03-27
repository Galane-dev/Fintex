using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Repository contract for notification alert-rule queries.
    /// </summary>
    public interface INotificationAlertRuleRepository : IRepository<NotificationAlertRule, long>
    {
        Task<List<NotificationAlertRule>> GetActivePriceAlertsAsync(string symbol, MarketDataProvider provider);

        Task<List<NotificationAlertRule>> GetUserRulesAsync(long userId);

        Task<NotificationAlertRule> GetUserRuleAsync(long userId, long ruleId);
    }
}
