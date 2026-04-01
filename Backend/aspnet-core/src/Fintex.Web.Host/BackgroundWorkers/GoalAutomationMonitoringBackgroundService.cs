using Abp.Domain.Uow;
using Fintex.Investments;
using Fintex.Investments.Goals.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Web.Host.BackgroundWorkers
{
    /// <summary>
    /// Evaluates BTC goal automation on a schedule so goals keep progressing even when the live UI stream is idle.
    /// </summary>
    public class GoalAutomationMonitoringBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<GoalAutomationMonitoringBackgroundService> _logger;

        public GoalAutomationMonitoringBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<GoalAutomationMonitoringBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                        var goalMonitoringService = scope.ServiceProvider.GetRequiredService<IGoalMonitoringService>();

                        using (var unitOfWork = unitOfWorkManager.Begin())
                        {
                            await goalMonitoringService.EvaluateLatestAsync("BTCUSDT", MarketDataProvider.Binance, stoppingToken);
                            await unitOfWork.CompleteAsync();
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Goal automation monitoring loop failed. Fintex will retry shortly.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}
