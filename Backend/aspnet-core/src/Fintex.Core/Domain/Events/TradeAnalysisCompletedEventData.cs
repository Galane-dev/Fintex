using Abp.Events.Bus;
using System;

namespace Fintex.Investments.Events
{
    /// <summary>
    /// Raised when a trade risk analysis snapshot has been stored.
    /// </summary>
    public class TradeAnalysisCompletedEventData : EventData
    {
        public int? TenantId { get; set; }

        public long TradeId { get; set; }

        public long UserId { get; set; }

        public long SnapshotId { get; set; }

        public decimal RiskScore { get; set; }

        public TradeRecommendation Recommendation { get; set; }

        public string Narrative { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
