namespace Fintex.Investments.Brokers.Dto
{
    /// <summary>
    /// Summary returned after importing broker activity into local trade state.
    /// </summary>
    public class ExternalBrokerSyncResultDto
    {
        public int ImportedTrades { get; set; }

        public int UpdatedTrades { get; set; }

        public int ClosedTrades { get; set; }
    }
}
