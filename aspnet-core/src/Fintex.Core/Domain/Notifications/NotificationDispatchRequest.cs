using System;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Describes a notification that should be persisted and optionally delivered in-app and by email.
    /// </summary>
    public class NotificationDispatchRequest
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public NotificationType Type { get; set; }

        public NotificationSeverity Severity { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public decimal? ReferencePrice { get; set; }

        public decimal? TargetPrice { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public MarketVerdict? Verdict { get; set; }

        public string TriggerKey { get; set; }

        public bool NotifyInApp { get; set; } = true;

        public bool NotifyEmail { get; set; }

        public string ContextJson { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
