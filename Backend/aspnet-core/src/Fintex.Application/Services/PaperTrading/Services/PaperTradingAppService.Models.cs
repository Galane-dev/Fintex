using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        private sealed class PaperTradeMarketContext
        {
            public MarketDataPoint LatestPoint { get; set; }

            public MarketVerdictDto RealtimeVerdict { get; set; }

            public decimal? Spread { get; set; }

            public decimal? SpreadPercent { get; set; }
        }

        private sealed class SuggestedTradePlan
        {
            public decimal StopLoss { get; set; }

            public decimal TakeProfit { get; set; }

            public decimal RewardRiskRatio { get; set; }
        }
    }
}
