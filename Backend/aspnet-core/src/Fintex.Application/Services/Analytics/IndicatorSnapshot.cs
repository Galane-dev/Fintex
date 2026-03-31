using System.Collections.Generic;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Represents the calculated market indicators for a symbol window.
    /// </summary>
    public class IndicatorSnapshot
    {
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

        public IReadOnlyList<IndicatorScore> Scores { get; set; } = new List<IndicatorScore>();
    }
}
