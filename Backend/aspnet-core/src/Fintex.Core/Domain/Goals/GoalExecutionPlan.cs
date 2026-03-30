using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Goals
{
    public class GoalExecutionPlan : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSummaryLength = 1024;
        public const int MaxSymbolLength = 32;

        protected GoalExecutionPlan()
        {
        }

        public GoalExecutionPlan(
            int? tenantId,
            long userId,
            long goalTargetId,
            string executionSymbol,
            TradeDirection? suggestedDirection,
            decimal? suggestedQuantity,
            decimal? suggestedStopLoss,
            decimal? suggestedTakeProfit,
            decimal? riskScore,
            string summary,
            string nextAction,
            DateTime generatedAtUtc)
        {
            TenantId = tenantId;
            UserId = userId;
            GoalTargetId = goalTargetId;
            Refresh(executionSymbol, suggestedDirection, suggestedQuantity, suggestedStopLoss, suggestedTakeProfit, riskScore, summary, nextAction, generatedAtUtc);
        }

        public int? TenantId { get; set; }
        public long UserId { get; protected set; }
        public long GoalTargetId { get; protected set; }
        public string ExecutionSymbol { get; protected set; }
        public TradeDirection? SuggestedDirection { get; protected set; }
        public decimal? SuggestedQuantity { get; protected set; }
        public decimal? SuggestedStopLoss { get; protected set; }
        public decimal? SuggestedTakeProfit { get; protected set; }
        public decimal? RiskScore { get; protected set; }
        public string Summary { get; protected set; }
        public string NextAction { get; protected set; }
        public DateTime GeneratedAtUtc { get; protected set; }

        public void Refresh(
            string executionSymbol,
            TradeDirection? suggestedDirection,
            decimal? suggestedQuantity,
            decimal? suggestedStopLoss,
            decimal? suggestedTakeProfit,
            decimal? riskScore,
            string summary,
            string nextAction,
            DateTime generatedAtUtc)
        {
            ExecutionSymbol = LimitSymbol(executionSymbol);
            SuggestedDirection = suggestedDirection;
            SuggestedQuantity = RoundNullable(suggestedQuantity);
            SuggestedStopLoss = RoundNullable(suggestedStopLoss);
            SuggestedTakeProfit = RoundNullable(suggestedTakeProfit);
            RiskScore = RoundNullable(riskScore);
            Summary = LimitText(summary);
            NextAction = LimitText(nextAction);
            GeneratedAtUtc = generatedAtUtc;
        }

        private static decimal? RoundNullable(decimal? value) => value.HasValue ? decimal.Round(value.Value, 8, MidpointRounding.AwayFromZero) : null;

        private static string LimitText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= MaxSummaryLength ? trimmed : trimmed.Substring(0, MaxSummaryLength);
        }

        private static string LimitSymbol(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim().ToUpperInvariant();
            return trimmed.Length <= MaxSymbolLength ? trimmed : trimmed.Substring(0, MaxSymbolLength);
        }
    }
}
