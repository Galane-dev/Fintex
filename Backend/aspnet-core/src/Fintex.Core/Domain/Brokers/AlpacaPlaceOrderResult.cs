using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Result returned from Alpaca after a market order submission attempt.
    /// </summary>
    public class AlpacaPlaceOrderResult
    {
        public bool IsSuccess { get; set; }

        public string Error { get; set; }

        public string Endpoint { get; set; }

        public string OrderId { get; set; }

        public string ClientOrderId { get; set; }

        public string Symbol { get; set; }

        public string Status { get; set; }

        public decimal? SubmittedQuantity { get; set; }

        public decimal? FilledQuantity { get; set; }

        public decimal? FilledAveragePrice { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? FilledAt { get; set; }

        public string ResponseJson { get; set; }
    }
}
