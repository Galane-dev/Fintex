using Abp.Application.Services.Dto;
using System.Collections.Generic;

namespace Fintex.Investments.Strategies.Dto
{
    /// <summary>
    /// Validation result returned to the dashboard.
    /// </summary>
    public class StrategyValidationResultDto : FullAuditedEntityDto<long>
    {
        public string StrategyName { get; set; }

        public string Symbol { get; set; }

        public string Timeframe { get; set; }

        public string DirectionPreference { get; set; }

        public string StrategyText { get; set; }

        public decimal? MarketPrice { get; set; }

        public decimal? MarketTrendScore { get; set; }

        public decimal? MarketConfidenceScore { get; set; }

        public string MarketVerdict { get; set; }

        public string NewsSummary { get; set; }

        public decimal ValidationScore { get; set; }

        public StrategyValidationOutcome Outcome { get; set; }

        public string Summary { get; set; }

        public List<string> Strengths { get; set; } = new List<string>();

        public List<string> Risks { get; set; } = new List<string>();

        public List<string> Improvements { get; set; } = new List<string>();

        public string SuggestedAction { get; set; }

        public decimal? SuggestedEntryPrice { get; set; }

        public decimal? SuggestedStopLoss { get; set; }

        public decimal? SuggestedTakeProfit { get; set; }

        public string AiProvider { get; set; }

        public string AiModel { get; set; }
    }
}
