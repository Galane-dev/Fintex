using Abp.Events.Bus;
using System;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Event raised whenever a notification is persisted for a user.
    /// </summary>
    public class NotificationCreatedEventData : EventData
    {
        public long NotificationId { get; set; }

        public long UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Symbol { get; set; }

        public string Severity { get; set; }

        public string Type { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
