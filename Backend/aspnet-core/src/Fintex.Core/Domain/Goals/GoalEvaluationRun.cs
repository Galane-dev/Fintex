using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Goals
{
    public class GoalEvaluationRun : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSummaryLength = 1024;

        protected GoalEvaluationRun()
        {
        }

        public GoalEvaluationRun(
            int? tenantId,
            long userId,
            long goalTargetId,
            GoalStatus goalStatus,
            decimal currentEquity,
            decimal requiredGrowthPercent,
            decimal requiredDailyGrowthPercent,
            decimal feasibilityScore,
            string summary,
            decimal? counterProposalTargetEquity,
            decimal? counterProposalTargetPercent,
            DateTime occurredAtUtc)
        {
            TenantId = tenantId;
            UserId = userId;
            GoalTargetId = goalTargetId;
            GoalStatus = goalStatus;
            CurrentEquity = RoundValue(currentEquity);
            RequiredGrowthPercent = RoundValue(requiredGrowthPercent);
            RequiredDailyGrowthPercent = RoundValue(requiredDailyGrowthPercent);
            FeasibilityScore = RoundValue(feasibilityScore);
            Summary = Limit(summary);
            CounterProposalTargetEquity = counterProposalTargetEquity.HasValue ? RoundValue(counterProposalTargetEquity.Value) : null;
            CounterProposalTargetPercent = counterProposalTargetPercent.HasValue ? RoundValue(counterProposalTargetPercent.Value) : null;
            OccurredAtUtc = occurredAtUtc;
        }

        public int? TenantId { get; set; }
        public long UserId { get; protected set; }
        public long GoalTargetId { get; protected set; }
        public GoalStatus GoalStatus { get; protected set; }
        public decimal CurrentEquity { get; protected set; }
        public decimal RequiredGrowthPercent { get; protected set; }
        public decimal RequiredDailyGrowthPercent { get; protected set; }
        public decimal FeasibilityScore { get; protected set; }
        public string Summary { get; protected set; }
        public decimal? CounterProposalTargetEquity { get; protected set; }
        public decimal? CounterProposalTargetPercent { get; protected set; }
        public DateTime OccurredAtUtc { get; protected set; }

        private static decimal RoundValue(decimal value) => decimal.Round(value, 8, MidpointRounding.AwayFromZero);

        private static string Limit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= MaxSummaryLength ? trimmed : trimmed.Substring(0, MaxSummaryLength);
        }
    }
}
