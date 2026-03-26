namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// Summary of directional confirmation from a specific timeframe.
    /// </summary>
    public class MarketVerdictTimeframeDto
    {
        public string Timeframe { get; set; }

        public decimal? BiasScore { get; set; }

        public IndicatorSignal Signal { get; set; }
    }
}
