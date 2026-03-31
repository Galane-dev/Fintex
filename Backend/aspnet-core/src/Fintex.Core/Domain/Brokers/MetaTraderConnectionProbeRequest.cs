namespace Fintex.Investments
{
    /// <summary>
    /// Request payload sent to the MetaTrader bridge for credential validation.
    /// </summary>
    public class MetaTraderConnectionProbeRequest
    {
        public string AccountLogin { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        public string TerminalPath { get; set; }
    }
}
