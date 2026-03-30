using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core repository for market data snapshots.
    /// </summary>
    public class MarketDataPointRepository : FintexRepositoryBase<MarketDataPoint, long>, IMarketDataPointRepository
    {
        public MarketDataPointRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<MarketDataPoint> GetLatestAsync(string symbol, MarketDataProvider provider)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == provider)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<MarketDataPoint> GetLatestBySymbolAsync(string symbol)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MarketDataPoint>> GetRecentAsync(string symbol, MarketDataProvider provider, int take)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == provider)
                .OrderByDescending(x => x.Timestamp)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<MarketDataPoint>> GetSinceAsync(string symbol, MarketDataProvider provider, DateTime startTimeUtc)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == provider && x.Timestamp >= startTimeUtc)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();
        }
    }
}
