using Abp.Application.Services.Dto;

namespace Fintex.Investments.Notifications.Dto
{
    /// <summary>
    /// User-facing view of an alert rule.
    /// </summary>
    public class NotificationAlertRuleDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public NotificationAlertRuleType AlertType { get; set; }

        public decimal? CreatedPrice { get; set; }

        public decimal? LastObservedPrice { get; set; }

        public decimal TargetPrice { get; set; }

        public bool NotifyInApp { get; set; }

        public bool NotifyEmail { get; set; }

        public bool IsActive { get; set; }

        public string Notes { get; set; }

        public string CreationTime { get; set; }

        public string LastTriggeredAt { get; set; }
    }
}
