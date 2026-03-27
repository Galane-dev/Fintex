namespace Fintex.Investments
{
    /// <summary>
    /// Tracks the lifecycle of a trade.
    /// </summary>
    public enum TradeStatus
    {
        Pending = 1,
        Open = 2,
        Closed = 3,
        Cancelled = 4
    }
}
