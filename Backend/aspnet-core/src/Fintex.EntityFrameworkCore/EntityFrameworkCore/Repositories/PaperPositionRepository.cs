using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core repository for querying paper positions.
    /// </summary>
    public class PaperPositionRepository : FintexRepositoryBase<PaperPosition, long>, IPaperPositionRepository
    {
        public PaperPositionRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<PaperPosition>> GetOpenPositionsAsync(long accountId)
        {
            return await GetAll()
                .Where(x => x.AccountId == accountId && x.Status == PaperPositionStatus.Open)
                .OrderByDescending(x => x.LastUpdatedAt)
                .ToListAsync();
        }

        public async Task<PaperPosition> GetOpenBySymbolAsync(long accountId, string symbol, MarketDataProvider provider)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x =>
                    x.AccountId == accountId &&
                    x.Status == PaperPositionStatus.Open &&
                    x.Symbol == normalized &&
                    x.Provider == provider)
                .OrderByDescending(x => x.LastUpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PaperPosition>> GetOpenByMarketAsync(string symbol, MarketDataProvider provider)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x =>
                    x.Status == PaperPositionStatus.Open &&
                    x.Symbol == normalized &&
                    x.Provider == provider)
                .OrderByDescending(x => x.LastUpdatedAt)
                .ToListAsync();
        }
    }
}
