using System;

namespace Fintex.Investments.MarketData
{
    /// <summary>
    /// Normalized inbound tick produced by market data connectors.
    /// </summary>
    public class MarketStreamTick
    {
        public int? TenantId { get; set; }

        public MarketDataProvider Provider { get; set; }

        public AssetClass AssetClass { get; set; }

        public string Symbol { get; set; }

        public decimal Price { get; set; }

        public decimal? Bid { get; set; }

        public decimal? Ask { get; set; }

        public decimal? Volume { get; set; }

        public decimal? Open24Hours { get; set; }

        public decimal? High24Hours { get; set; }

        public decimal? Low24Hours { get; set; }

        public long? Sequence { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
