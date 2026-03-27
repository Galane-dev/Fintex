using System;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// Represents RSI for a specific chart timeframe.
    /// </summary>
    public class MarketTimeframeRsiDto
    {
        public string Timeframe { get; set; }

        public decimal? Value { get; set; }

        public DateTime? CandleTimestamp { get; set; }
    }
}
