using Abp.Application.Services.Dto;
using System;

namespace Fintex.Investments.Profiles.Dto
{
    /// <summary>
    /// DTO for exposing user profile preferences and AI insights.
    /// </summary>
    public class UserProfileDto : FullAuditedEntityDto<long>
    {
        public long UserId { get; set; }

        public string PreferredBaseCurrency { get; set; }

        public string FavoriteSymbols { get; set; }

        public decimal RiskTolerance { get; set; }

        public bool IsAiInsightsEnabled { get; set; }

        public decimal BehavioralRiskScore { get; set; }

        public string BehavioralSummary { get; set; }

        public string StrategyNotes { get; set; }

        public string LastAiProvider { get; set; }

        public string LastAiModel { get; set; }

        public DateTime? LastBehavioralAnalysisTime { get; set; }
    }
}
