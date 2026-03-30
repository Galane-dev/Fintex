using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.Investments.Automation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core queries for auto-execution rules.
    /// </summary>
    public class TradeAutomationRuleRepository : FintexRepositoryBase<TradeAutomationRule, long>, ITradeAutomationRuleRepository
    {
        public TradeAutomationRuleRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<TradeAutomationRule>> GetActiveRulesAsync(string symbol, MarketDataProvider provider)
        {
            var normalizedSymbol = NormalizeSymbol(symbol);
            return await GetAll()
                .Where(x => x.IsActive && x.Symbol == normalizedSymbol && x.Provider == provider)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<List<TradeAutomationRule>> GetUserRulesAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<TradeAutomationRule> GetUserRuleAsync(long userId, long ruleId)
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
