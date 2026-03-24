using Abp.Domain.Repositories;
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

        Task<List<MarketDataPoint>> GetRecentAsync(string symbol, int take);
    }
}
