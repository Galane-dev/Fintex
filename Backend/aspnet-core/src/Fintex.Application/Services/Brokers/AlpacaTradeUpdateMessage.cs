using System;

namespace Fintex.Investments.Brokers
{
    /// <summary>
    /// Simplified trade update payload captured from Alpaca's websocket stream.
    /// </summary>
    public class AlpacaTradeUpdateMessage
    {
        public string EventType { get; set; }

        public string ExecutionId { get; set; }

        public string OrderId { get; set; }

        public string ClientOrderId { get; set; }

        public string Symbol { get; set; }

        public string Side { get; set; }

        public string OrderStatus { get; set; }

        public decimal? OrderQuantity { get; set; }

        public decimal? FilledQuantity { get; set; }

        public decimal? EventQuantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? FilledAveragePrice { get; set; }

        public decimal? PositionQuantity { get; set; }

        public DateTime? OccurredAt { get; set; }

        public string RawPayloadJson { get; set; }
    }
}
