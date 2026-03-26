using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Bridge contract for validating and later operating MetaTrader-based broker accounts.
    /// </summary>
    public interface IMetaTraderBridgeService
    {
        Task<MetaTraderConnectionProbeResult> ProbeConnectionAsync(MetaTraderConnectionProbeRequest request);
    }
}
