using AutoMapper;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Analytics.Dto;
using Fintex.Investments.Goals;
using Fintex.Investments.Goals.Dto;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Notifications;
using Fintex.Investments.Notifications.Dto;
using Fintex.Investments.PaperTrading.Dto;
using Fintex.Investments.Profiles.Dto;
using Fintex.Investments.Trading.Dto;

namespace Fintex.Investments
{
    /// <summary>
    /// AutoMapper profile for the investment domain DTOs.
    /// </summary>
    public class InvestmentMapProfile : Profile
    {
        public InvestmentMapProfile()
        {
            CreateMap<Trade, TradeDto>();
            CreateMap<MarketDataPoint, MarketDataPointDto>();
            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<TradeAnalysisSnapshot, TradeAnalysisSnapshotDto>();
            CreateMap<ExternalBrokerConnection, ExternalBrokerConnectionDto>();
            CreateMap<PaperTradingAccount, PaperTradingAccountDto>();
            CreateMap<PaperOrder, PaperOrderDto>();
            CreateMap<PaperPosition, PaperPositionDto>();
            CreateMap<PaperTradeFill, PaperTradeFillDto>();
            CreateMap<NotificationItem, NotificationItemDto>();
            CreateMap<NotificationAlertRule, NotificationAlertRuleDto>();
            CreateMap<GoalTarget, GoalTargetDto>();
            CreateMap<GoalEvaluationRun, GoalEvaluationRunDto>();
            CreateMap<GoalExecutionPlan, GoalExecutionPlanDto>();
            CreateMap<GoalExecutionEvent, GoalExecutionEventDto>();
        }
    }
}
