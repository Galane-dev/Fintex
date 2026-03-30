namespace Fintex.Investments
{
    /// <summary>
    /// Result returned after validating Alpaca API credentials.
    /// </summary>
    public class AlpacaConnectionProbeResult
    {
        public bool IsSuccess { get; set; }

        public string Error { get; set; }

        public string AccountNumber { get; set; }

        public string AccountStatus { get; set; }

        public string Currency { get; set; }

        public string Company { get; set; }

        public int? Multiplier { get; set; }

        public decimal? Cash { get; set; }

        public decimal? Equity { get; set; }

        public string Endpoint { get; set; }
    }
}
