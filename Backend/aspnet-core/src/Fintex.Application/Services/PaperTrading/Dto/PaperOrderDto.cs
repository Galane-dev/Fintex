using System;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Order result for the paper trading simulator.
    /// </summary>
    public class PaperOrderDto
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public long? PositionId { get; set; }

        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeDirection Direction { get; set; }

        public PaperOrderType OrderType { get; set; }

        public PaperOrderStatus Status { get; set; }

        public decimal Quantity { get; set; }

        public decimal? RequestedPrice { get; set; }

        public decimal? ExecutedPrice { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public string Notes { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? ExecutedAt { get; set; }
    }
}
