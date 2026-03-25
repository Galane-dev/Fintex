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

        public decimal? Sma { get; set; }

        public decimal? Ema { get; set; }

        public decimal? Rsi { get; set; }

        public decimal? Macd { get; set; }

        public decimal? MacdSignal { get; set; }

        public decimal? MacdHistogram { get; set; }

        public decimal? Momentum { get; set; }

        public decimal? RateOfChange { get; set; }

        public decimal? Atr { get; set; }

        public decimal? AtrPercent { get; set; }

        public decimal? Adx { get; set; }

        public decimal? StructureScore { get; set; }

        public string StructureLabel { get; set; }

        public decimal? TimeframeAlignmentScore { get; set; }

        public IReadOnlyList<IndicatorScoreDto> IndicatorScores { get; set; }

        public IReadOnlyList<MarketVerdictTimeframeDto> TimeframeSignals { get; set; }
    }
}
