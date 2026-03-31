using Abp.Application.Services.Dto;

namespace Fintex.Investments.Notifications.Dto
{
    /// <summary>
    /// User-facing view of a delivered notification.
    /// </summary>
    public class NotificationItemDto : EntityDto<long>
    {
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

        public bool IsRead { get; set; }

        public bool EmailSent { get; set; }

        public string EmailError { get; set; }

        public string OccurredAt { get; set; }
    }
}
