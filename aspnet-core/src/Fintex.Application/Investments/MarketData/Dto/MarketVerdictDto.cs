using System;
using System.Collections.Generic;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// DTO for exposing the latest market-only estimate and verdict.
    /// </summary>
    public class MarketVerdictDto
    {
        public long MarketDataPointId { get; set; }

        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public decimal Price { get; set; }

        public decimal? TrendScore { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public MarketVerdict Verdict { get; set; }

        public DateTime Timestamp { get; set; }

        public IReadOnlyList<IndicatorScoreDto> IndicatorScores { get; set; }
    }
}
