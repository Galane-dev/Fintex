using System;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// Short-horizon price projection built from moving-average drift.
    /// </summary>
    public class MarketPriceProjectionDto
    {
        public string Horizon { get; set; }

        public int MinutesAhead { get; set; }

        public DateTime TargetTimestamp { get; set; }

        public string ModelName { get; set; }

        public decimal? ConsensusPrice { get; set; }

        public decimal? SmaPrice { get; set; }

        public decimal? EmaPrice { get; set; }

        public decimal? SmmaPrice { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public MarketProjectionMaturity Maturity { get; set; }

        public int BarsUsed { get; set; }

        public int EffectivePeriod { get; set; }
    }
}
