using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// Input for requesting a single market indicator.
    /// </summary>
    public class GetMarketIndicatorInput : GetMarketDataHistoryInput
    {
        [Required]
        public MarketIndicatorType Indicator { get; set; }
    }
}
