using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Automation.Dto
{
    /// <summary>
    /// Creates a one-shot auto-execution rule for the current user.
    /// </summary>
    public class CreateTradeAutomationRuleInput
    {
        [Required]
        [MaxLength(TradeAutomationRule.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [MaxLength(TradeAutomationRule.MaxSymbolLength)]
        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; } = MarketDataProvider.Binance;

        public TradeAutomationTriggerType TriggerType { get; set; }

        public decimal? TriggerValue { get; set; }

        public MarketVerdict? TargetVerdict { get; set; }

        public decimal? MinimumConfidenceScore { get; set; }

        public TradeAutomationDestination Destination { get; set; }

        public long? ExternalConnectionId { get; set; }

        public TradeDirection TradeDirection { get; set; }

        [Range(typeof(decimal), "0.00000001", "999999999")]
        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public bool NotifyInApp { get; set; } = true;

        public bool NotifyEmail { get; set; } = true;

        [MaxLength(TradeAutomationRule.MaxNotesLength)]
        public string Notes { get; set; }
    }
}
