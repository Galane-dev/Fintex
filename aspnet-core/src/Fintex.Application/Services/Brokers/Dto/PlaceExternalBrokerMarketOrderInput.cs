using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Brokers.Dto
{
    /// <summary>
    /// Input for placing a live market order through a connected external broker.
    /// </summary>
    public class PlaceExternalBrokerMarketOrderInput
    {
        [Required]
        public long ConnectionId { get; set; }

        [Required]
        [MaxLength(Trade.MaxSymbolLength)]
        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeDirection Direction { get; set; }

        [Range(typeof(decimal), "0.00000001", "999999999")]
        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public string Notes { get; set; }
    }
}
