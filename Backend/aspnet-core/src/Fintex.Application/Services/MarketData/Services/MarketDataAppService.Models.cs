using System;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
        private sealed class TimeframeClosePoint
        {
            public DateTime OpenTime { get; set; }

            public decimal Close { get; set; }
        }

        private sealed class TimeframeDirectionPoint
        {
            public MarketDataTimeframe Timeframe { get; set; }

            public decimal BiasScore { get; set; }

            public IndicatorSignal Signal { get; set; }
        }

        private sealed class WeightedSignalPoint
        {
            public WeightedSignalPoint(decimal weight, decimal normalized)
            {
                Weight = weight;
                Normalized = normalized;
            }

            public decimal Weight { get; }

            public decimal Normalized { get; }
        }
    }
}
