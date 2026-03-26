namespace Fintex.Investments
{
    /// <summary>
    /// Request payload for validating an Alpaca API connection.
    /// </summary>
    public class AlpacaConnectionProbeRequest
    {
        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public bool IsPaperEnvironment { get; set; }
    }
}
