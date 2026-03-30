using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Input used to ask the simulator what the user should do right now.
    /// </summary>
    public class GetPaperTradeRecommendationInput
    {
        [Required]
        [MaxLength(PaperOrder.MaxSymbolLength)]
        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; } = AssetClass.Crypto;

        public MarketDataProvider Provider { get; set; } = MarketDataProvider.Binance;

        [Range(0.00000001, 1000000000)]
        public decimal? Quantity { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }
    }
}
