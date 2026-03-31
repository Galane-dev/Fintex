using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Strategies.Dto
{
    /// <summary>
    /// User input for validating a strategy against current market conditions.
    /// </summary>
    public class ValidateStrategyInput
    {
        [MaxLength(StrategyValidationRun.MaxNameLength)]
        public string StrategyName { get; set; }

        [Required]
        [MaxLength(StrategyValidationRun.MaxSymbolLength)]
        public string Symbol { get; set; } = "BTCUSDT";

        public MarketDataProvider Provider { get; set; } = MarketDataProvider.Binance;

        [MaxLength(StrategyValidationRun.MaxTimeframeLength)]
        public string Timeframe { get; set; } = "1m";

        [MaxLength(StrategyValidationRun.MaxDirectionLength)]
        public string DirectionPreference { get; set; }

        [Required]
        [MaxLength(StrategyValidationRun.MaxStrategyLength)]
        public string StrategyText { get; set; }
    }
}
