namespace Fintex.Investments
{
    /// <summary>
    /// Result returned from the MetaTrader bridge after validating a broker login.
    /// </summary>
    public class MetaTraderConnectionProbeResult
    {
        public bool IsSuccess { get; set; }

        public string Error { get; set; }

        public string AccountLogin { get; set; }

        public string AccountName { get; set; }

        public string Server { get; set; }

        public string Company { get; set; }

        public string Currency { get; set; }

        public int? Leverage { get; set; }

        public decimal? Balance { get; set; }

        public decimal? Equity { get; set; }
    }
}
