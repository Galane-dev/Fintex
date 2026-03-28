namespace Fintex.Investments.Automation.Dto
{
    /// <summary>
    /// Lightweight automation rule payload shown on the dashboard.
    /// </summary>
    public class TradeAutomationRuleDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public TradeAutomationTriggerType TriggerType { get; set; }

        public decimal? CreatedMetricValue { get; set; }

        public decimal? LastObservedMetricValue { get; set; }

        public decimal? TriggerValue { get; set; }

        public MarketVerdict? TargetVerdict { get; set; }

        public decimal? MinimumConfidenceScore { get; set; }

        public TradeAutomationDestination Destination { get; set; }

        public long? ExternalConnectionId { get; set; }

        public TradeDirection TradeDirection { get; set; }

        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? TakeProfit { get; set; }

        public bool NotifyInApp { get; set; }

        public bool NotifyEmail { get; set; }

        public bool IsActive { get; set; }

        public string Notes { get; set; }

        public string CreationTime { get; set; }

        public string LastTriggeredAt { get; set; }

        public long? LastTradeId { get; set; }
    }
}
