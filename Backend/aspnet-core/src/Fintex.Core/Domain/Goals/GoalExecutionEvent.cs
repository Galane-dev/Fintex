using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Goals
{
    public class GoalExecutionEvent : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxTypeLength = 64;
        public const int MaxStatusLength = 32;
        public const int MaxSummaryLength = 1024;

        protected GoalExecutionEvent()
        {
        }

        public GoalExecutionEvent(
            int? tenantId,
            long userId,
            long goalTargetId,
            string eventType,
            string status,
            string summary,
            long? tradeId,
            decimal? equityAfterExecution,
            DateTime occurredAtUtc)
        {
            TenantId = tenantId;
            UserId = userId;
            GoalTargetId = goalTargetId;
            EventType = LimitRequired(eventType, MaxTypeLength);
            Status = LimitRequired(status, MaxStatusLength);
            Summary = LimitRequired(summary, MaxSummaryLength);
            TradeId = tradeId;
            EquityAfterExecution = equityAfterExecution.HasValue ? decimal.Round(equityAfterExecution.Value, 8, MidpointRounding.AwayFromZero) : null;
            OccurredAtUtc = occurredAtUtc;
        }

        public int? TenantId { get; set; }
        public long UserId { get; protected set; }
        public long GoalTargetId { get; protected set; }
        public string EventType { get; protected set; }
        public string Status { get; protected set; }
        public string Summary { get; protected set; }
        public long? TradeId { get; protected set; }
        public decimal? EquityAfterExecution { get; protected set; }
        public DateTime OccurredAtUtc { get; protected set; }

        private static string LimitRequired(string value, int maxLength)
        {
            var trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("Value is required.", nameof(value));
            }

            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }
    }
}
