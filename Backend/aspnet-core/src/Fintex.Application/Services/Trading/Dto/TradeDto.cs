using Abp.Application.Services.Dto;
using System;

namespace Fintex.Investments.Trading.Dto
{
    /// <summary>
    /// Represents a trade returned by the application layer.
    /// </summary>
    public class TradeDto : FullAuditedEntityDto<long>
    {
        public long UserId { get; set; }

        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeDirection Direction { get; set; }

        public TradeStatus Status { get; set; }

        public decimal Quantity { get; set; }

        public decimal EntryPrice { get; set; }

        public decimal? ExitPrice { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public decimal RealizedProfitLoss { get; set; }

        public decimal UnrealizedProfitLoss { get; set; }

        public decimal LastMarketPrice { get; set; }

        public decimal CurrentRiskScore { get; set; }

        public string CurrentRecommendation { get; set; }

        public string CurrentAnalysisSummary { get; set; }

        public string ExternalOrderId { get; set; }

        public string Notes { get; set; }

        public DateTime ExecutedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public ClosedTradeReviewDto ClosedTradeReview { get; set; }
    }
}
