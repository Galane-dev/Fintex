using Abp.Events.Bus;
using System;

namespace Fintex.Investments.Events
{
    /// <summary>
    /// Raised when a new market data point has been persisted and analyzed.
    /// </summary>
    public class MarketDataUpdatedEventData : EventData
    {
        public int? TenantId { get; set; }

        public long MarketDataPointId { get; set; }

        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public MarketDataProvider Provider { get; set; }

        public decimal Price { get; set; }

        public decimal? Bid { get; set; }

        public decimal? Ask { get; set; }

        public decimal? Volume { get; set; }

        public decimal? Sma { get; set; }

        public decimal? Ema { get; set; }

        public decimal? Rsi { get; set; }

        public decimal? StdDev { get; set; }

        public decimal? Macd { get; set; }

        public decimal? MacdSignal { get; set; }

        public decimal? MacdHistogram { get; set; }

        public decimal? Momentum { get; set; }

        public decimal? RateOfChange { get; set; }

        public decimal? BollingerUpper { get; set; }

        public decimal? BollingerLower { get; set; }

        public decimal? TrendScore { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public MarketVerdict Verdict { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
