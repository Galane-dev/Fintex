using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// Input for requesting market data history.
    /// </summary>
    public class GetMarketDataHistoryInput
    {
        [Required]
        [MaxLength(MarketDataPoint.MaxSymbolLength)]
        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        [Range(1, 500)]
        public int Take { get; set; } = 100;
    }
}
