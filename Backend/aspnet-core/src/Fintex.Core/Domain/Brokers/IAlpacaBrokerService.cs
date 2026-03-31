using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fintex.Investments
{
    /// <summary>
    /// Validates and later operates Alpaca broker connections.
    /// </summary>
    public interface IAlpacaBrokerService
    {
        Task<AlpacaConnectionProbeResult> ProbeConnectionAsync(AlpacaConnectionProbeRequest request);

        Task<AlpacaPlaceOrderResult> PlaceMarketOrderAsync(AlpacaPlaceOrderRequest request);

        Task<List<AlpacaPositionSnapshot>> GetOpenPositionsAsync(AlpacaAccountRequest request);

        Task<List<AlpacaOrderSnapshot>> GetRecentOrdersAsync(AlpacaAccountRequest request);
    }
}
