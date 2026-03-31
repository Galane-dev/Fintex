using Fintex.Investments.MarketData.Dto;
using System.Collections.Generic;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Risk and setup feedback for a proposed paper trade.
    /// </summary>
    public class PaperTradeAssessmentDto
    {
        public TradeDirection Direction { get; set; }

        public decimal RiskScore { get; set; }

        public PaperTradeRiskLevel RiskLevel { get; set; }

        public bool ShouldBlock { get; set; }

        public string Headline { get; set; }

        public string Summary { get; set; }

        public decimal ReferencePrice { get; set; }

        public decimal? Spread { get; set; }

        public decimal? SpreadPercent { get; set; }

        public decimal? SuggestedStopLoss { get; set; }

        public decimal? SuggestedTakeProfit { get; set; }

        public decimal? SuggestedRewardRiskRatio { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public decimal? TrendScore { get; set; }

        public decimal? TimeframeAlignmentScore { get; set; }

        public string StructureLabel { get; set; }

        public MarketVerdict MarketVerdict { get; set; }

        public List<string> Reasons { get; set; } = new List<string>();

        public List<string> Suggestions { get; set; } = new List<string>();
    }
}
