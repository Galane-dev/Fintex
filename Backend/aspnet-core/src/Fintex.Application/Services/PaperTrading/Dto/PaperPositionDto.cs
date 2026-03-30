using System;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Open or closed position summary for the paper simulator.
    /// </summary>
    public class PaperPositionDto
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeDirection Direction { get; set; }

        public PaperPositionStatus Status { get; set; }

        public decimal Quantity { get; set; }

        public decimal AverageEntryPrice { get; set; }

        public decimal CurrentMarketPrice { get; set; }

        public decimal RealizedProfitLoss { get; set; }

        public decimal UnrealizedProfitLoss { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public DateTime OpenedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }
    }
}
