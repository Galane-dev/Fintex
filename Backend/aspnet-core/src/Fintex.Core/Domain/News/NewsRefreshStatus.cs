namespace Fintex.Investments
{
    /// <summary>
    /// Tracks the lifecycle of a news refresh attempt.
    /// </summary>
    public enum NewsRefreshStatus
    {
        Started = 1,
        Completed = 2,
        Failed = 3,
        Skipped = 4
    }
}
