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
    }
}
