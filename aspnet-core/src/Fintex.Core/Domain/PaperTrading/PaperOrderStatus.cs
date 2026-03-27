namespace Fintex.Investments
{
    /// <summary>
    /// Status of a paper order in the internal simulator.
    /// </summary>
    public enum PaperOrderStatus
    {
        Pending = 1,
        Filled = 2,
        Cancelled = 3,
        Rejected = 4
    }
}
