namespace Fintex.Investments
{
    /// <summary>
    /// Current connection state for an external broker account.
    /// </summary>
    public enum ExternalBrokerConnectionStatus
    {
        Pending = 1,
        Connected = 2,
        Failed = 3,
        Disconnected = 4
    }
}
