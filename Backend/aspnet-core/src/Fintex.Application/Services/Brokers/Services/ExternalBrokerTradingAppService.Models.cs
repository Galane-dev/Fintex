using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;

namespace Fintex.Investments.Brokers
{
    public partial class ExternalBrokerTradingAppService
    {
        private sealed class LiveTradeMarketContext
        {
            public MarketDataPoint LatestPoint { get; set; }

            public MarketVerdictDto Verdict { get; set; }

            public decimal? Spread { get; set; }

            public decimal? SpreadPercent { get; set; }
        }

        private sealed class BrokerExposureAllocation
        {
            public string Symbol { get; set; }

            public TradeDirection Direction { get; set; }

            public decimal RemainingQuantity { get; set; }

            public decimal? CurrentPrice { get; set; }

            public decimal? AverageEntryPrice { get; set; }
        }
    }
}
