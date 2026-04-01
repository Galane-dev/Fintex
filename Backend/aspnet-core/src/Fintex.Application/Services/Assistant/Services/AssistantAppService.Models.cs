using Fintex.Investments.Automation.Dto;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.EconomicCalendar.Dto;
using Fintex.Investments.Goals.Dto;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Notifications.Dto;
using Fintex.Investments.PaperTrading.Dto;
using Fintex.Investments.Profiles.Dto;
using Fintex.Investments.Strategies.Dto;
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

            public List<GoalTargetDto> Goals { get; set; } = new List<GoalTargetDto>();

            public List<TradeAutomationRuleDto> AutomationRules { get; set; } = new List<TradeAutomationRuleDto>();

            public List<StrategyValidationResultDto> StrategyValidations { get; set; } = new List<StrategyValidationResultDto>();

            public MacroCalendarInsightDto MacroInsight { get; set; }
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

            public long? RuleId { get; set; }

            public long? NotificationId { get; set; }

            public long? PositionId { get; set; }

            public long? GoalId { get; set; }

            public string GoalName { get; set; }

            public string BaseCurrency { get; set; }

            public decimal? StartingBalance { get; set; }

            public string AccountType { get; set; }

            public string TargetType { get; set; }

            public string TriggerType { get; set; }

            public string Destination { get; set; }

            public string TargetVerdict { get; set; }

            public decimal? TriggerValue { get; set; }

            public decimal? MinimumConfidenceScore { get; set; }

            public decimal? TargetPercent { get; set; }

            public decimal? TargetAmount { get; set; }

            public System.DateTime? DeadlineUtc { get; set; }

            public decimal? MaxAcceptableRisk { get; set; }

            public decimal? MaxDrawdownPercent { get; set; }

            public decimal? MaxPositionSizePercent { get; set; }

            public string TradingSession { get; set; }

            public bool? AllowOvernightPositions { get; set; }

            public bool? NotifyEmail { get; set; }

            public bool? NotifyInApp { get; set; }

            public string StrategyName { get; set; }

            public string StrategyText { get; set; }

            public string Timeframe { get; set; }

            public string DirectionPreference { get; set; }

            public string Notes { get; set; }
        }
    }
}
