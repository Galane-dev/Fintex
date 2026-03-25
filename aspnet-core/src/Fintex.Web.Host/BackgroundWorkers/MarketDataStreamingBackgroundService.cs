using Fintex.Investments.MarketData;
using Fintex.Web.Host.MarketData.Streaming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.BackgroundWorkers
{
    /// <summary>
    /// Starts all enabled market connectors and forwards ticks into the application layer.
    /// </summary>
    public class MarketDataStreamingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEnumerable<IMarketDataStreamClient> _streamClients;
        private readonly ILogger<MarketDataStreamingBackgroundService> _logger;
        private readonly SemaphoreSlim _tickProcessingLock = new SemaphoreSlim(1, 1);

        public MarketDataStreamingBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            IEnumerable<IMarketDataStreamClient> streamClients,
            ILogger<MarketDataStreamingBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _streamClients = streamClients;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var clients = _streamClients.ToList();
            _logger.LogInformation("Market data background worker starting with {ClientCount} stream clients.", clients.Count);

            if (clients.Count == 0)
            {
                _logger.LogWarning("No market data stream clients were registered. Live ingestion will not run.");
                return Task.CompletedTask;
            }

            var tasks = new List<Task>();
            foreach (var client in clients)
            {
                _logger.LogInformation("Starting market data stream client {ClientName}.", client.GetType().Name);
                tasks.Add(client.RunAsync(ProcessTickAsync, stoppingToken));
            }

            return Task.WhenAll(tasks);
        }

        private async Task ProcessTickAsync(MarketStreamTick tick, CancellationToken cancellationToken)
        {
            await _tickProcessingLock.WaitAsync(cancellationToken);

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var ingestionService = scope.ServiceProvider.GetRequiredService<IMarketDataIngestionService>();
                    await ingestionService.IngestAsync(tick, cancellationToken);
                    _logger.LogDebug("Persisted market tick for {Symbol} from {Provider} at {Timestamp}.", tick.Symbol, tick.Provider, tick.Timestamp);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process market tick for {Symbol}.", tick.Symbol);
            }
            finally
            {
                _tickProcessingLock.Release();
            }
        }
    }
}
