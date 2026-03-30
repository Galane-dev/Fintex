namespace Fintex.Investments
{
    public enum MarketVerdictState
    {
        WarmingUp = 1,
        Live = 2,
        Degraded = 3,
        Stale = 4,
        Fallback = 5
    }
}
