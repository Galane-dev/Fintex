using Abp.Dependency;
using Fintex.Investments.PaperTrading;
using Fintex.Investments.PaperTrading.Dto;
using System;

namespace Fintex.Investments.Goals.Services
{
    public class GoalPlannerService : IGoalPlannerService, ITransientDependency
    {
        public GoalPlanDraft BuildPlan(
            GoalTarget goal,
            GoalProgressSnapshot progress,
            PaperTradeRecommendationDto recommendation,
            bool hasOpenExposure)
        {
            var nowUtc = DateTime.UtcNow;
            if (!IsWithinSession(goal, nowUtc))
            {
                return BuildWaitingPlan("Trading session filter is active, so Fintex is waiting for the configured window before considering a BTC entry.");
            }

            if (hasOpenExposure)
            {
                return BuildWaitingPlan("A BTC position is already open for this goal account, so Fintex is waiting before adding more exposure.");
            }

            if (recommendation == null || recommendation.RecommendedAction == MarketVerdict.Hold)
            {
                return BuildWaitingPlan("The market recommendation is still neutral, so the goal stays on watch.");
            }

            if (recommendation.RiskLevel == PaperTradeRiskLevel.High || recommendation.RiskScore > goal.MaxAcceptableRisk)
            {
                return BuildWaitingPlan($"The BTC setup is currently too risky for this goal. Recommendation risk is {recommendation.RiskScore:0.##} against a goal limit of {goal.MaxAcceptableRisk:0.##}.");
            }

            if (recommendation.EconomicCalendarRiskScore.HasValue && recommendation.EconomicCalendarRiskScore.Value >= 70m)
            {
                return BuildWaitingPlan("A macro-event blackout is active, so Fintex is waiting for economic-calendar risk to cool off.");
            }

            if (recommendation.ReferencePrice <= 0m)
            {
                return BuildWaitingPlan("The latest BTC reference price is unavailable, so the goal cannot size a trade yet.");
            }

            var positionBudget = progress.CurrentEquity * (goal.MaxPositionSizePercent / 100m);
            var quantity = positionBudget <= 0m
                ? 0m
                : decimal.Round(positionBudget / recommendation.ReferencePrice, 8, MidpointRounding.AwayFromZero);
            if (quantity <= 0m)
            {
                return BuildWaitingPlan("The position budget for this goal is too small to size a BTC trade cleanly.");
            }

            var suggestedDirection = recommendation.RecommendedAction == MarketVerdict.Sell
                ? TradeDirection.Sell
                : TradeDirection.Buy;

            return new GoalPlanDraft
            {
                ExecutionSymbol = goal.AccountType == GoalAccountType.ExternalBroker ? "BTCUSD" : goal.MarketSymbol,
                SuggestedDirection = suggestedDirection,
                SuggestedQuantity = quantity,
                SuggestedStopLoss = recommendation.SuggestedStopLoss,
                SuggestedTakeProfit = recommendation.SuggestedTakeProfit,
                RiskScore = recommendation.RiskScore,
                Summary = $"Best-effort plan is ready. Fintex can attempt a {suggestedDirection.ToString().ToLowerInvariant()} on BTC with quantity {quantity:0.########}, stop loss {FormatNullable(recommendation.SuggestedStopLoss)}, and take profit {FormatNullable(recommendation.SuggestedTakeProfit)}.",
                NextAction = progress.RequiredDailyGrowthPercent > 0m && progress.RequiredDailyGrowthPercent > 0.6m
                    ? "The goal is behind pace, but Fintex will still wait for a disciplined entry instead of forcing a trade."
                    : "The goal is on watch and ready to execute if the next BTC setup still meets the same guardrails.",
                ShouldExecute = true
            };
        }

        private static GoalPlanDraft BuildWaitingPlan(string summary)
        {
            return new GoalPlanDraft
            {
                Summary = summary,
                NextAction = "Keep monitoring BTC and re-check on the next eligible market update.",
                ShouldExecute = false
            };
        }

        private static bool IsWithinSession(GoalTarget goal, DateTime nowUtc)
        {
            if (goal.TradingSession == GoalTradingSession.AnyTime)
            {
                return goal.AllowOvernightPositions || nowUtc.Hour >= 6 && nowUtc.Hour < 21;
            }

            return goal.TradingSession switch
            {
                GoalTradingSession.Europe => nowUtc.Hour >= 7 && nowUtc.Hour < 15,
                GoalTradingSession.Us => nowUtc.Hour >= 13 && nowUtc.Hour < 21,
                GoalTradingSession.EuropeUsOverlap => nowUtc.Hour >= 13 && nowUtc.Hour < 16,
                _ => true
            };
        }

        private static string FormatNullable(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("0.########") : "-";
        }
    }
}
