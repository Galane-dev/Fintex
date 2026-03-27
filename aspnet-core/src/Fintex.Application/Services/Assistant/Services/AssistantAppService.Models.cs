using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Notifications.Dto;
using Fintex.Investments.PaperTrading.Dto;
using Fintex.Investments.Profiles.Dto;
using Fintex.Investments.Trading.Dto;
using System.Collections.Generic;

namespace Fintex.Investments.Assistant
{
    public partial class AssistantAppService
    {
        private sealed class AssistantContextSnapshot
        {
            public MarketVerdictDto Verdict { get; set; }

            public PaperTradingSnapshotDto PaperSnapshot { get; set; }

            public PaperTradeRecommendationDto Recommendation { get; set; }

            public NotificationInboxDto Notifications { get; set; }

            public UserProfileDto Profile { get; set; }

            public List<TradeDto> Trades { get; set; } = new List<TradeDto>();

            public List<ExternalBrokerConnectionDto> Connections { get; set; } = new List<ExternalBrokerConnectionDto>();
        }

        private sealed class AssistantPlan
        {
            public string Reply { get; set; }

            public string VoiceReply { get; set; }

            public List<string> SuggestedPrompts { get; set; } = new List<string>();

            public List<AssistantPlannedAction> Actions { get; set; } = new List<AssistantPlannedAction>();
        }

        private sealed class AssistantPlannedAction
        {
            public string Type { get; set; }

            public string Symbol { get; set; }

            public string Direction { get; set; }

            public decimal? Quantity { get; set; }

            public decimal? TargetPrice { get; set; }

            public decimal? StopLoss { get; set; }

            public decimal? TakeProfit { get; set; }

            public long? ConnectionId { get; set; }

            public bool? NotifyEmail { get; set; }

            public bool? NotifyInApp { get; set; }

            public string Notes { get; set; }
        }
    }
}
