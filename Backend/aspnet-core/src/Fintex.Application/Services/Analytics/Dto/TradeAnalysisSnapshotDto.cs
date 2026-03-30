using Abp.Application.Services.Dto;
using System;

namespace Fintex.Investments.Analytics.Dto
{
    /// <summary>
    /// DTO for exposing a stored trade analytics snapshot.
    /// </summary>
    public class TradeAnalysisSnapshotDto : CreationAuditedEntityDto<long>
    {
        public long TradeId { get; set; }

        public long UserId { get; set; }

        public DateTime GeneratedAt { get; set; }

        public decimal SmaValue { get; set; }

        public decimal EmaValue { get; set; }

        public decimal RsiValue { get; set; }

        public decimal StdDevValue { get; set; }

        public decimal CompositeRiskScore { get; set; }

        public TradeRecommendation Recommendation { get; set; }

        public string Narrative { get; set; }

        public string BehavioralSummary { get; set; }

        public string ExternalAiProvider { get; set; }

        public string ExternalAiModel { get; set; }
    }
}
