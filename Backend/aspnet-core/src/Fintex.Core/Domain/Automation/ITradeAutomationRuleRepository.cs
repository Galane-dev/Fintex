using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments.Automation
{
    /// <summary>
    /// Repository contract for auto-execution rule queries.
    /// </summary>
    public interface ITradeAutomationRuleRepository : IRepository<TradeAutomationRule, long>
    {
        Task<List<TradeAutomationRule>> GetActiveRulesAsync(string symbol, MarketDataProvider provider);

        Task<List<TradeAutomationRule>> GetUserRulesAsync(long userId);

        Task<TradeAutomationRule> GetUserRuleAsync(long userId, long ruleId);
    }
}
