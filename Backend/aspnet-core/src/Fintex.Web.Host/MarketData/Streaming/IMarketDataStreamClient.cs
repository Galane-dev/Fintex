using Fintex.Investments.MarketData;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.MarketData.Streaming
{
    /// <summary>
    /// Contract implemented by real-time market data connectors.
    /// </summary>
    public interface IMarketDataStreamClient
    {
        Task RunAsync(Func<MarketStreamTick, CancellationToken, Task> onTickAsync, CancellationToken cancellationToken);
    }
}
