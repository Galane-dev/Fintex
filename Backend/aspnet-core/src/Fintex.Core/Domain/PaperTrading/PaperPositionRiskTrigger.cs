namespace Fintex.Investments
{
    /// <summary>
    /// Describes which paper risk rule, if any, has been hit by the latest market price.
    /// </summary>
    public enum PaperPositionRiskTrigger
    {
        None = 0,
        StopLoss = 1,
        TakeProfit = 2
    }
}
