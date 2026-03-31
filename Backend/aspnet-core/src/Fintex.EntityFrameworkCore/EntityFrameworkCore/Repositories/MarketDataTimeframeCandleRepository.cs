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
    /// EF Core repository for derived timeframe candles.
    /// </summary>
    public class MarketDataTimeframeCandleRepository : FintexRepositoryBase<MarketDataTimeframeCandle, long>, IMarketDataTimeframeCandleRepository
    {
        private readonly IDbContextProvider<FintexDbContext> _dbContextProvider;

        public MarketDataTimeframeCandleRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<MarketDataTimeframeCandle> GetLatestAsync(string symbol, MarketDataProvider provider, MarketDataTimeframe timeframe)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == provider && x.Timeframe == timeframe)
                .OrderByDescending(x => x.OpenTime)
                .FirstOrDefaultAsync();
        }

        public async Task<MarketDataTimeframeCandle> GetByOpenTimeAsync(string symbol, MarketDataProvider provider, MarketDataTimeframe timeframe, DateTime openTime)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x =>
                    x.Symbol == normalized &&
                    x.Provider == provider &&
                    x.Timeframe == timeframe &&
                    x.OpenTime == openTime)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MarketDataTimeframeCandle>> GetRecentAsync(string symbol, MarketDataProvider provider, MarketDataTimeframe timeframe, int take)
        {
            var normalized = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();

            return await GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == provider && x.Timeframe == timeframe)
                .OrderByDescending(x => x.OpenTime)
                .Take(take)
                .ToListAsync();
        }

        public async Task UpsertAsync(
            int? tenantId,
            MarketDataProvider provider,
            AssetClass assetClass,
            string symbol,
            MarketDataTimeframe timeframe,
            DateTime openTime,
            decimal price,
            DateTime priceTimestamp)
        {
            var context = _dbContextProvider.GetDbContext();
            var normalizedSymbol = symbol == null ? string.Empty : symbol.Trim().ToUpperInvariant();
            var normalizedOpenTime = openTime.Kind == DateTimeKind.Utc ? openTime : openTime.ToUniversalTime();
            var normalizedPriceTimestamp = priceTimestamp.Kind == DateTimeKind.Utc ? priceTimestamp : priceTimestamp.ToUniversalTime();
            var createdAt = DateTime.UtcNow;

            await context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO ""AppMarketDataTimeframeCandles""
    (""TenantId"", ""Provider"", ""AssetClass"", ""Symbol"", ""Timeframe"", ""OpenTime"", ""Open"", ""High"", ""Low"", ""Close"", ""LastPriceTimestamp"", ""CreationTime"", ""CreatorUserId"")
VALUES
    ({tenantId}, {provider.ToString()}, {assetClass.ToString()}, {normalizedSymbol}, {timeframe.ToString()}, {normalizedOpenTime}, {price}, {price}, {price}, {price}, {normalizedPriceTimestamp}, {createdAt}, {null})
ON CONFLICT (""Provider"", ""Symbol"", ""Timeframe"", ""OpenTime"")
DO UPDATE
SET
    ""High"" = GREATEST(""AppMarketDataTimeframeCandles"".""High"", EXCLUDED.""High""),
    ""Low"" = LEAST(""AppMarketDataTimeframeCandles"".""Low"", EXCLUDED.""Low""),
    ""Close"" = CASE
        WHEN EXCLUDED.""LastPriceTimestamp"" >= ""AppMarketDataTimeframeCandles"".""LastPriceTimestamp"" THEN EXCLUDED.""Close""
        ELSE ""AppMarketDataTimeframeCandles"".""Close""
    END,
    ""LastPriceTimestamp"" = GREATEST(""AppMarketDataTimeframeCandles"".""LastPriceTimestamp"", EXCLUDED.""LastPriceTimestamp"");");
        }
    }
}
