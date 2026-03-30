using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Goals.Dto;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Notifications.Dto;
using Fintex.Investments.PaperTrading.Dto;
using Fintex.Investments.Trading.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.Assistant
{
    public partial class AssistantAppService
    {
        private async Task<AssistantContextSnapshot> LoadContextSnapshotAsync()
        {
            var marketInput = new GetMarketDataHistoryInput
            {
                Symbol = "BTCUSDT",
                Provider = MarketDataProvider.Binance,
                Take = 80
            };

            // These reads share the same request-scoped unit of work, so they must stay
            // sequential to avoid EF Core's "second operation started" DbContext error.
            var verdict = await _marketDataAppService.GetRealtimeVerdictAsync(marketInput);
            var paperSnapshot = await _paperTradingAppService.GetMySnapshotAsync();
            var recommendation = await _paperTradingAppService.GetRecommendationAsync(new GetPaperTradeRecommendationInput
            {
                Symbol = "BTCUSDT",
                Provider = MarketDataProvider.Binance,
                AssetClass = AssetClass.Crypto
            });
            var notifications = await _notificationAppService.GetMyInboxAsync(new GetMyNotificationsInput
            {
                MaxResultCount = 8,
                UnreadOnly = false
            });
            var profile = await _userProfileAppService.GetMyProfileAsync();
            var trades = await _tradeAppService.GetMyTradesAsync();
            var connections = await _externalBrokerAppService.GetMyConnectionsAsync();
            var goals = await _goalAutomationAppService.GetMyGoalsAsync();

            return new AssistantContextSnapshot
            {
                Verdict = verdict,
                PaperSnapshot = paperSnapshot,
                Recommendation = recommendation,
                Notifications = notifications,
                Profile = profile,
                Trades = trades.Items?.ToList() ?? new List<TradeDto>(),
                Connections = connections.Items?.ToList() ?? new List<ExternalBrokerConnectionDto>(),
                Goals = goals.Items?.ToList() ?? new List<GoalTargetDto>()
            };
        }
    }
}
