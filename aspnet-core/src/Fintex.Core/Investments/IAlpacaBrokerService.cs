using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Validates and later operates Alpaca broker connections.
    /// </summary>
    public interface IAlpacaBrokerService
    {
        Task<AlpacaConnectionProbeResult> ProbeConnectionAsync(AlpacaConnectionProbeRequest request);
    }
}
