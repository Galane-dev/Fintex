using Abp.Application.Services.Dto;
using System;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// DTO for exposing market data history and latest quotes.
    /// </summary>
    public class MarketDataPointDto : CreationAuditedEntityDto<long>
    {
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

        public decimal? Sma { get; set; }

        public decimal? Ema { get; set; }

        public decimal? Rsi { get; set; }

        public decimal? StdDev { get; set; }
    }
}
