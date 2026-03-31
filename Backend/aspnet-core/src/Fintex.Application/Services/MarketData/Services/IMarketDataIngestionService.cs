using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    /// <summary>
    /// Persists raw market ticks and triggers downstream analytics.
    /// </summary>
    public interface IMarketDataIngestionService
    {
        Task<MarketDataPoint> IngestAsync(MarketStreamTick tick, CancellationToken cancellationToken);
    }
}
