using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core repository for trade aggregate-specific queries.
    /// </summary>
    public class TradeRepository : FintexRepositoryBase<Trade, long>, ITradeRepository
    {
        public TradeRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<Trade>> GetOpenTradesBySymbolAsync(string symbol)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized && x.Status == TradeStatus.Open)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetUserTradesAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetUserOpenTradesAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId && x.Status == TradeStatus.Open)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<Trade> GetByExternalOrderIdAsync(long userId, string externalOrderId)
        {
            var normalized = externalOrderId == null ? string.Empty : externalOrderId.Trim();

            return await GetAll()
                .Where(x => x.UserId == userId && x.ExternalOrderId == normalized)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();
        }
    }
}
