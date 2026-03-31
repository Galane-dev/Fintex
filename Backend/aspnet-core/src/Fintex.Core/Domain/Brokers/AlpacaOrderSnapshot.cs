using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Flattened order view returned from Alpaca.
    /// </summary>
    public class AlpacaOrderSnapshot
    {
        public string OrderId { get; set; }

        public string ClientOrderId { get; set; }

        public string Symbol { get; set; }

        public string Side { get; set; }

        public string Status { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? FilledQuantity { get; set; }

        public decimal? FilledAveragePrice { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? FilledAt { get; set; }

        public string RawJson { get; set; }
    }
}
