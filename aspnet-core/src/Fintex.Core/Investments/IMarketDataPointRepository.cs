using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for market data history and snapshot queries.
    /// </summary>
    public interface IMarketDataPointRepository : IRepository<MarketDataPoint, long>
    {
        Task<MarketDataPoint> GetLatestAsync(string symbol, MarketDataProvider provider);

        Task<MarketDataPoint> GetLatestBySymbolAsync(string symbol);

        Task<List<MarketDataPoint>> GetRecentAsync(string symbol, MarketDataProvider provider, int take);

        Task<List<MarketDataPoint>> GetSinceAsync(string symbol, MarketDataProvider provider, DateTime startTimeUtc);
    }
}
