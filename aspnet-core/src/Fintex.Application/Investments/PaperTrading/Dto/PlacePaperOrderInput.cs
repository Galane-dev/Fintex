using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Places a simulated market order against the current paper trading account.
    /// </summary>
    public class PlacePaperOrderInput
    {
        [Required]
        [MaxLength(PaperOrder.MaxSymbolLength)]
        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; } = AssetClass.Crypto;

        public MarketDataProvider Provider { get; set; } = MarketDataProvider.Binance;

        public TradeDirection Direction { get; set; }

        [Range(0.00000001, 1000000000)]
        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        [MaxLength(PaperOrder.MaxNotesLength)]
        public string Notes { get; set; }
    }
}
