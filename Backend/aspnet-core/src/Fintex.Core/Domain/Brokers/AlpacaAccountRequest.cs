namespace Fintex.Investments
{
    /// <summary>
    /// Shared access payload for Alpaca account data requests.
    /// </summary>
    public class AlpacaAccountRequest
    {
        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public bool IsPaperEnvironment { get; set; }
    }
}
