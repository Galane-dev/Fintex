using Abp.Dependency;
using Abp.Runtime.Session;
using Fintex.Investments.Brokers;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.PaperTrading;
using Fintex.Investments.PaperTrading.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals.Services
{
    public class GoalExecutionService : IGoalExecutionService, ITransientDependency
    {
        private readonly IPaperTradingAppService _paperTradingAppService;
        private readonly IExternalBrokerTradingAppService _externalBrokerTradingAppService;
        private readonly IAbpSession _abpSession;

        public GoalExecutionService(
            IPaperTradingAppService paperTradingAppService,
            IExternalBrokerTradingAppService externalBrokerTradingAppService,
            IAbpSession abpSession)
        {
            _paperTradingAppService = paperTradingAppService;
            _externalBrokerTradingAppService = externalBrokerTradingAppService;
            _abpSession = abpSession;
        }

        public async Task<GoalExecutionResult> ExecuteAsync(GoalTarget goal, GoalPlanDraft plan, CancellationToken cancellationToken)
        {
            if (!plan.ShouldExecute || !plan.SuggestedDirection.HasValue || !plan.SuggestedQuantity.HasValue || plan.SuggestedQuantity.Value <= 0m)
            {
                return new GoalExecutionResult
                {
                    WasExecuted = false,
                    Summary = plan.NextAction ?? "The goal plan is still waiting for a cleaner setup."
                };
            }

            using (_abpSession.Use(goal.TenantId, goal.UserId))
            {
                if (goal.AccountType == GoalAccountType.PaperTrading)
                {
                    var result = await _paperTradingAppService.PlaceMarketOrderAsync(new PlacePaperOrderInput
                    {
                        Symbol = plan.ExecutionSymbol ?? goal.MarketSymbol,
                        AssetClass = AssetClass.Crypto,
                        Provider = MarketDataProvider.Binance,
                        Direction = plan.SuggestedDirection.Value,
                        Quantity = plan.SuggestedQuantity.Value,
                        StopLoss = plan.SuggestedStopLoss,
                        TakeProfit = plan.SuggestedTakeProfit,
                        Notes = $"Goal autopilot: {goal.Name}"
                    });

                    return result.WasExecuted
                        ? new GoalExecutionResult
                        {
                            WasExecuted = true,
                            TradeId = result.Order?.Id,
                            Summary = $"Fintex auto-executed a paper {result.Order?.Direction.ToString().ToLowerInvariant()} for goal '{goal.Name}'."
                        }
                        : new GoalExecutionResult
                        {
                            WasExecuted = false,
                            Error = result.Assessment?.Summary ?? result.Assessment?.Headline,
                            Summary = result.Assessment?.Headline ?? "The goal paper trade was blocked."
                        };
                }

                var execution = await _externalBrokerTradingAppService.PlaceMarketOrderAsync(new PlaceExternalBrokerMarketOrderInput
                {
                    ConnectionId = goal.ExternalConnectionId ?? 0,
                    Symbol = plan.ExecutionSymbol ?? "BTCUSD",
                    AssetClass = AssetClass.Crypto,
                    Provider = MarketDataProvider.Binance,
                    Direction = plan.SuggestedDirection.Value,
                    Quantity = plan.SuggestedQuantity.Value,
                    StopLoss = plan.SuggestedStopLoss,
                    TakeProfit = plan.SuggestedTakeProfit,
                    Notes = $"Goal autopilot: {goal.Name}"
                });

                return new GoalExecutionResult
                {
                    WasExecuted = true,
                    TradeId = execution.Trade?.Id,
                    Summary = execution.Summary
                };
            }
        }
    }
}
