using Fintex.Investments.MarketData;
using Fintex.Web.Host.MarketData.Streaming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            var tasks = new List<Task>();
            foreach (var client in _streamClients)
            {
                tasks.Add(client.RunAsync(ProcessTickAsync, stoppingToken));
            }

            return Task.WhenAll(tasks);
        }

        private async Task ProcessTickAsync(MarketStreamTick tick, CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var ingestionService = scope.ServiceProvider.GetRequiredService<IMarketDataIngestionService>();
                    await ingestionService.IngestAsync(tick, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process market tick for {Symbol}.", tick.Symbol);
            }
        }
    }
}
