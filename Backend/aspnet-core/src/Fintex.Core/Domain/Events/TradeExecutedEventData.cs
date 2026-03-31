using Abp.Events.Bus;
using System;

namespace Fintex.Investments.Events
{
    /// <summary>
    /// Raised when a trade is opened, closed, or cancelled.
    /// </summary>
    public class TradeExecutedEventData : EventData
    {
        public int? TenantId { get; set; }

        public long TradeId { get; set; }

        public long UserId { get; set; }

        public long? PositionId { get; set; }

        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeDirection Direction { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? ExecutionPrice { get; set; }

        public string Source { get; set; }

        public TradeStatus Status { get; set; }

        public decimal? RealizedProfitLoss { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
