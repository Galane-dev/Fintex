namespace Fintex.Investments.Automation
{
    /// <summary>
    /// Supported trigger conditions for automatic trade execution rules.
    /// </summary>
    public enum TradeAutomationTriggerType
    {
        PriceTarget = 1,
        RelativeStrengthIndex = 2,
        MacdHistogram = 3,
        Momentum = 4,
        TrendScore = 5,
        ConfidenceScore = 6,
        Verdict = 7
    }
}
