using System;
using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Trading.Dto
{
    /// <summary>
    /// Input for opening a new trade.
    /// </summary>
    public class CreateTradeInput
    {
        [Required]
        [MaxLength(Trade.MaxSymbolLength)]
        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeDirection Direction { get; set; }

        [Range(typeof(decimal), "0.00000001", "999999999")]
        public decimal Quantity { get; set; }

        public decimal? EntryPrice { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public string ExternalOrderId { get; set; }

        public string Notes { get; set; }

        public DateTime? ExecutedAt { get; set; }
    }
}
