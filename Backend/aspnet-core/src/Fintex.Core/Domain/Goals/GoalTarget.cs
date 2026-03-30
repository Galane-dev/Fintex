using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Goals
{
    public class GoalTarget : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxNameLength = 128;
        public const int MaxSymbolLength = 32;
        public const int MaxSymbolsLength = 128;
        public const int MaxSummaryLength = 1024;

        protected GoalTarget()
        {
        }

        public GoalTarget(
            int? tenantId,
            long userId,
            string name,
            GoalAccountType accountType,
            long? externalConnectionId,
            string marketSymbol,
            string allowedSymbols,
            GoalTargetType targetType,
            decimal startEquity,
            decimal targetEquity,
            decimal targetPercent,
            DateTime deadlineUtc,
            decimal maxAcceptableRisk,
            decimal maxDrawdownPercent,
            decimal maxPositionSizePercent,
            GoalTradingSession tradingSession,
            bool allowOvernightPositions)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User id must be greater than zero.");
            }

            if (startEquity <= 0m || targetEquity <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(startEquity), "Start and target equity must be greater than zero.");
            }

            TenantId = tenantId;
            UserId = userId;
            Name = LimitRequired(name, MaxNameLength, "Goal name is required.");
            AccountType = accountType;
            ExternalConnectionId = accountType == GoalAccountType.ExternalBroker ? externalConnectionId : null;
            MarketSymbol = LimitRequired(marketSymbol, MaxSymbolLength, "Market symbol is required.").ToUpperInvariant();
            AllowedSymbols = LimitRequired(allowedSymbols, MaxSymbolsLength, "Allowed symbols are required.").ToUpperInvariant();
            TargetType = targetType;
            StartEquity = RoundMoney(startEquity);
            CurrentEquity = StartEquity;
            TargetEquity = RoundMoney(targetEquity);
            TargetPercent = RoundPercent(targetPercent);
            DeadlineUtc = deadlineUtc;
            MaxAcceptableRisk = RoundPercent(maxAcceptableRisk);
            MaxDrawdownPercent = RoundPercent(maxDrawdownPercent);
            MaxPositionSizePercent = RoundPercent(maxPositionSizePercent);
            TradingSession = tradingSession;
            AllowOvernightPositions = allowOvernightPositions;
            Status = GoalStatus.Draft;
        }

        public int? TenantId { get; set; }
        public long UserId { get; protected set; }
        public string Name { get; protected set; }
        public GoalAccountType AccountType { get; protected set; }
        public long? ExternalConnectionId { get; protected set; }
        public string MarketSymbol { get; protected set; }
        public string AllowedSymbols { get; protected set; }
        public GoalTargetType TargetType { get; protected set; }
        public decimal StartEquity { get; protected set; }
        public decimal CurrentEquity { get; protected set; }
        public decimal TargetEquity { get; protected set; }
        public decimal TargetPercent { get; protected set; }
        public DateTime DeadlineUtc { get; protected set; }
        public decimal MaxAcceptableRisk { get; protected set; }
        public decimal MaxDrawdownPercent { get; protected set; }
        public decimal MaxPositionSizePercent { get; protected set; }
        public GoalTradingSession TradingSession { get; protected set; }
        public bool AllowOvernightPositions { get; protected set; }
        public GoalStatus Status { get; protected set; }
        public string StatusReason { get; protected set; }
        public string LatestPlanSummary { get; protected set; }
        public string LatestNextAction { get; protected set; }
        public decimal ProgressPercent { get; protected set; }
        public decimal RequiredDailyGrowthPercent { get; protected set; }
        public DateTime? LastEvaluatedAtUtc { get; protected set; }
        public DateTime? LastExecutedAtUtc { get; protected set; }
        public DateTime? LastExecutionAttemptAtUtc { get; protected set; }
        public int ExecutedTradesCount { get; protected set; }
        public long? LastTradeId { get; protected set; }
        public string LastError { get; protected set; }

        public void ApplyInitialDecision(
            bool accepted,
            decimal currentEquity,
            decimal progressPercent,
            decimal requiredDailyGrowthPercent,
            string summary)
        {
            CurrentEquity = RoundMoney(currentEquity);
            ProgressPercent = RoundPercent(progressPercent);
            RequiredDailyGrowthPercent = RoundPercent(requiredDailyGrowthPercent);
            StatusReason = LimitOptional(summary, MaxSummaryLength);
            Status = accepted ? GoalStatus.Accepted : GoalStatus.Rejected;
            LastEvaluatedAtUtc = DateTime.UtcNow;
        }

        public void Activate(string summary)
        {
            Status = GoalStatus.Active;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
        }

        public void Pause(string summary)
        {
            Status = GoalStatus.Paused;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
        }

        public void Resume(string summary)
        {
            Status = GoalStatus.Active;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
        }

        public void Cancel(string summary)
        {
            Status = GoalStatus.Canceled;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
        }

        public void Complete(decimal currentEquity, string summary)
        {
            CurrentEquity = RoundMoney(currentEquity);
            ProgressPercent = 100m;
            RequiredDailyGrowthPercent = 0m;
            Status = GoalStatus.Completed;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
            LastEvaluatedAtUtc = DateTime.UtcNow;
        }

        public void Expire(decimal currentEquity, string summary)
        {
            CurrentEquity = RoundMoney(currentEquity);
            Status = GoalStatus.Expired;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
            LastEvaluatedAtUtc = DateTime.UtcNow;
        }

        public void RefreshProgress(
            decimal currentEquity,
            decimal progressPercent,
            decimal requiredDailyGrowthPercent,
            string summary,
            DateTime occurredAtUtc)
        {
            CurrentEquity = RoundMoney(currentEquity);
            ProgressPercent = RoundPercent(progressPercent);
            RequiredDailyGrowthPercent = RoundPercent(requiredDailyGrowthPercent);
            StatusReason = LimitOptional(summary, MaxSummaryLength);
            LastEvaluatedAtUtc = occurredAtUtc;
            LastError = null;
        }

        public void UpdatePlan(string summary, string nextAction)
        {
            LatestPlanSummary = LimitOptional(summary, MaxSummaryLength);
            LatestNextAction = LimitOptional(nextAction, MaxSummaryLength);
        }

        public void RecordExecutionAttempt(DateTime occurredAtUtc)
        {
            LastExecutionAttemptAtUtc = occurredAtUtc;
        }

        public void RecordExecutionSuccess(long? tradeId, string summary, DateTime occurredAtUtc)
        {
            LastTradeId = tradeId;
            LastExecutedAtUtc = occurredAtUtc;
            LastExecutionAttemptAtUtc = occurredAtUtc;
            ExecutedTradesCount += 1;
            StatusReason = LimitOptional(summary, MaxSummaryLength);
            LastError = null;
        }

        public void RecordExecutionFailure(string error, DateTime occurredAtUtc)
        {
            LastExecutionAttemptAtUtc = occurredAtUtc;
            LastError = LimitOptional(error, MaxSummaryLength);
        }

        public bool CanAttemptExecution(DateTime nowUtc, TimeSpan cooldown)
        {
            return !LastExecutionAttemptAtUtc.HasValue || nowUtc - LastExecutionAttemptAtUtc.Value >= cooldown;
        }

        private static decimal RoundMoney(decimal value) => decimal.Round(value, 8, MidpointRounding.AwayFromZero);

        private static decimal RoundPercent(decimal value) => decimal.Round(value, 4, MidpointRounding.AwayFromZero);

        private static string LimitRequired(string value, int maxLength, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(error, nameof(value));
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }
    }
}
