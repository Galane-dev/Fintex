namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// High-level verdict for a validated strategy.
    /// </summary>
    public enum StrategyValidationOutcome
    {
        Fail = 0,
        Caution = 1,
        Validated = 2
    }
}
