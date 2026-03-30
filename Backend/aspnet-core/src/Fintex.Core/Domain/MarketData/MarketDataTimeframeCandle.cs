using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Aggregated OHLC candle derived from persisted market ticks for a specific timeframe.
    /// </summary>
    public class MarketDataTimeframeCandle : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        protected MarketDataTimeframeCandle()
        {
        }

        public MarketDataTimeframeCandle(
            int? tenantId,
            MarketDataProvider provider,
            AssetClass assetClass,
            string symbol,
            MarketDataTimeframe timeframe,
            DateTime openTime,
            decimal price,
            DateTime lastPriceTimestamp)
        {
            TenantId = tenantId;
            Provider = provider;
            AssetClass = assetClass;
            Symbol = NormalizeSymbol(symbol);
            Timeframe = timeframe;
            OpenTime = openTime.Kind == DateTimeKind.Utc ? openTime : openTime.ToUniversalTime();
            Open = EnsurePositive(price, nameof(price));
            High = price;
            Low = price;
            Close = price;
            LastPriceTimestamp = lastPriceTimestamp.Kind == DateTimeKind.Utc
                ? lastPriceTimestamp
                : lastPriceTimestamp.ToUniversalTime();
        }

        public int? TenantId { get; set; }

        public MarketDataProvider Provider { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public string Symbol { get; protected set; }

        public MarketDataTimeframe Timeframe { get; protected set; }

        public DateTime OpenTime { get; protected set; }

        public decimal Open { get; protected set; }

        public decimal High { get; protected set; }

        public decimal Low { get; protected set; }

        public decimal Close { get; protected set; }

        public DateTime LastPriceTimestamp { get; protected set; }

        public void ApplyTick(decimal price, DateTime timestamp)
        {
            var normalizedTimestamp = timestamp.Kind == DateTimeKind.Utc
                ? timestamp
                : timestamp.ToUniversalTime();

            High = Math.Max(High, price);
            Low = Math.Min(Low, price);
            Close = price;
            LastPriceTimestamp = normalizedTimestamp;
        }

        private static decimal EnsurePositive(decimal value, string name)
        {
            if (value <= 0m)
            {
                throw new ArgumentOutOfRangeException(name, "Value must be greater than zero.");
            }

            return value;
        }

        private static string NormalizeSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Symbol is required.", nameof(symbol));
            }

            var normalized = symbol.Trim().ToUpperInvariant();
            if (normalized.Length > MarketDataPoint.MaxSymbolLength)
            {
                throw new ArgumentException("Symbol is too long.", nameof(symbol));
            }

            return normalized;
        }
    }
}
