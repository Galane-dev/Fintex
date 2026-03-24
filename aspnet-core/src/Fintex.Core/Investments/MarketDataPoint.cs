using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Aggregate root that captures a single persisted market tick with analytics.
    /// </summary>
    public class MarketDataPoint : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSymbolLength = 32;

        protected MarketDataPoint()
        {
        }

        public MarketDataPoint(
            int? tenantId,
            MarketDataProvider provider,
            AssetClass assetClass,
            string symbol,
            decimal price,
            decimal? bid,
            decimal? ask,
            decimal? volume,
            decimal? open24Hours,
            decimal? high24Hours,
            decimal? low24Hours,
            long? sequence,
            DateTime timestamp)
        {
            TenantId = tenantId;
            Provider = provider;
            AssetClass = assetClass;
            Symbol = NormalizeSymbol(symbol);
            Price = EnsurePositive(price, nameof(price));
            Bid = bid;
            Ask = ask;
            Volume = volume;
            Open24Hours = open24Hours;
            High24Hours = high24Hours;
            Low24Hours = low24Hours;
            Sequence = sequence;
            Timestamp = timestamp;
        }

        public int? TenantId { get; set; }

        public MarketDataProvider Provider { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public string Symbol { get; protected set; }

        public decimal Price { get; protected set; }

        public decimal? Bid { get; protected set; }

        public decimal? Ask { get; protected set; }

        public decimal? Volume { get; protected set; }

        public decimal? Open24Hours { get; protected set; }

        public decimal? High24Hours { get; protected set; }

        public decimal? Low24Hours { get; protected set; }

        public long? Sequence { get; protected set; }

        public DateTime Timestamp { get; protected set; }

        public decimal? Sma { get; protected set; }

        public decimal? Ema { get; protected set; }

        public decimal? Rsi { get; protected set; }

        public decimal? StdDev { get; protected set; }

        /// <summary>
        /// Adds the calculated indicator values to the market point.
        /// </summary>
        public void ApplyIndicators(decimal? sma, decimal? ema, decimal? rsi, decimal? stdDev)
        {
            Sma = sma;
            Ema = ema;
            Rsi = rsi;
            StdDev = stdDev;
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
            if (normalized.Length > MaxSymbolLength)
            {
                throw new ArgumentException("Symbol is too long.", nameof(symbol));
            }

            return normalized;
        }
    }
}
