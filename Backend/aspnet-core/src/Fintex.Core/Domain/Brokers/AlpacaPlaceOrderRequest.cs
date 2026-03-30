using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Request payload for submitting a market order to Alpaca.
    /// </summary>
    public class AlpacaPlaceOrderRequest
    {
        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public bool IsPaperEnvironment { get; set; }

        public string Symbol { get; set; }

        public TradeDirection Direction { get; set; }

        public decimal Quantity { get; set; }

        public string ClientOrderId { get; set; }

        public bool UseBracketExits { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }
    }
}
