using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for timeframe candle queries.
    /// </summary>
    public interface IMarketDataTimeframeCandleRepository : IRepository<MarketDataTimeframeCandle, long>
    {
        Task<MarketDataTimeframeCandle> GetLatestAsync(string symbol, MarketDataProvider provider, MarketDataTimeframe timeframe);

        Task<MarketDataTimeframeCandle> GetByOpenTimeAsync(string symbol, MarketDataProvider provider, MarketDataTimeframe timeframe, DateTime openTime);

        Task<List<MarketDataTimeframeCandle>> GetRecentAsync(string symbol, MarketDataProvider provider, MarketDataTimeframe timeframe, int take);

        Task UpsertAsync(
            int? tenantId,
            MarketDataProvider provider,
            AssetClass assetClass,
            string symbol,
            MarketDataTimeframe timeframe,
            DateTime openTime,
            decimal price,
            DateTime priceTimestamp);
    }
}
