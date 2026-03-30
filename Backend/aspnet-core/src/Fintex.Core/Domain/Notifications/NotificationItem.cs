using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Persisted in-app and email notification delivered to a specific user.
    /// </summary>
    public class NotificationItem : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxTitleLength = 256;
        public const int MaxMessageLength = 4000;
        public const int MaxSymbolLength = 32;
        public const int MaxTriggerKeyLength = 256;
        public const int MaxErrorLength = 1024;
        public const int MaxContextJsonLength = 8000;

        protected NotificationItem()
        {
        }

        public NotificationItem(
            int? tenantId,
            long userId,
            NotificationType type,
            NotificationSeverity severity,
            string title,
            string message,
            string symbol,
            MarketDataProvider provider,
            decimal? referencePrice,
            decimal? targetPrice,
            decimal? confidenceScore,
            MarketVerdict? verdict,
            string triggerKey,
            bool emailRequested,
            string contextJson,
            DateTime occurredAt)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User id must be greater than zero.");
            }

            TenantId = tenantId;
            UserId = userId;
            Type = type;
            Severity = severity;
            Title = LimitRequired(title, MaxTitleLength, "Notification title is required.");
            Message = LimitRequired(message, MaxMessageLength, "Notification message is required.");
            Symbol = LimitRequired(symbol, MaxSymbolLength, "Notification symbol is required.").ToUpperInvariant();
            Provider = provider;
            ReferencePrice = referencePrice;
            TargetPrice = targetPrice;
            ConfidenceScore = confidenceScore;
            Verdict = verdict;
            TriggerKey = LimitRequired(triggerKey, MaxTriggerKeyLength, "Notification trigger key is required.");
            EmailRequested = emailRequested;
            ContextJson = LimitOptional(contextJson, MaxContextJsonLength);
            OccurredAt = occurredAt;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public NotificationType Type { get; protected set; }

        public NotificationSeverity Severity { get; protected set; }

        public string Title { get; protected set; }

        public string Message { get; protected set; }

        public string Symbol { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public decimal? ReferencePrice { get; protected set; }

        public decimal? TargetPrice { get; protected set; }

        public decimal? ConfidenceScore { get; protected set; }

        public MarketVerdict? Verdict { get; protected set; }

        public string TriggerKey { get; protected set; }

        public bool IsRead { get; protected set; }

        public DateTime? ReadAt { get; protected set; }

        public bool EmailRequested { get; protected set; }

        public bool EmailSent { get; protected set; }

        public DateTime? EmailSentAt { get; protected set; }

        public string EmailError { get; protected set; }

        public bool InAppDelivered { get; protected set; }

        public DateTime? InAppDeliveredAt { get; protected set; }

        public DateTime OccurredAt { get; protected set; }

        public string ContextJson { get; protected set; }

        public void MarkRead(DateTime occurredAt)
        {
            IsRead = true;
            ReadAt = occurredAt;
        }

        public void MarkEmailSent(DateTime occurredAt)
        {
            EmailSent = true;
            EmailSentAt = occurredAt;
            EmailError = null;
        }

        public void MarkEmailFailed(string error)
        {
            EmailError = LimitOptional(error, MaxErrorLength);
        }

        public void MarkInAppDelivered(DateTime occurredAt)
        {
            InAppDelivered = true;
            InAppDeliveredAt = occurredAt;
        }

        private static string LimitRequired(string value, int maxLength, string error)
        {
            var limited = LimitOptional(value, maxLength);
            if (string.IsNullOrWhiteSpace(limited))
            {
                throw new ArgumentException(error);
            }

            return limited;
        }

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
