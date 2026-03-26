using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    /// <summary>
    /// Persists Alpaca websocket trade update events for later reconciliation and behavioral analysis.
    /// </summary>
    public interface IAlpacaTradeUpdateIngestionService
    {
        Task CaptureAsync(long connectionId, AlpacaTradeUpdateMessage message);
    }
}
