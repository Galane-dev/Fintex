using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Fintex.Investments.Events;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Fintex.Web.Host.Hubs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Fintex.Web.Host.Realtime
{
    /// <summary>
    /// Broadcasts persisted market and analytics events to SignalR subscribers.
    /// </summary>
    public class MarketDataSignalREventForwarder :
        IAsyncEventHandler<MarketDataUpdatedEventData>,
        IAsyncEventHandler<TradeAnalysisCompletedEventData>,
        IAsyncEventHandler<TradeExecutedEventData>,
        ITransientDependency
    {
        private readonly IHubContext<MarketDataHub> _hubContext;
        private readonly IMarketDataAppService _marketDataAppService;
        private readonly ILogger<MarketDataSignalREventForwarder> _logger;

        public MarketDataSignalREventForwarder(
            IHubContext<MarketDataHub> hubContext,
            IMarketDataAppService marketDataAppService,
            ILogger<MarketDataSignalREventForwarder> logger)
        {
            _hubContext = hubContext;
            _marketDataAppService = marketDataAppService;
            _logger = logger;
        }

        public async Task HandleEventAsync(MarketDataUpdatedEventData eventData)
        {
            var payload = new
            {
                eventData.MarketDataPointId,
                eventData.Symbol,
                eventData.AssetClass,
                eventData.Provider,
                eventData.Price,
                eventData.Bid,
                eventData.Ask,
                eventData.Volume,
                eventData.Sma,
                eventData.Ema,
                eventData.Rsi,
                eventData.StdDev,
                eventData.Macd,
                eventData.MacdSignal,
                eventData.MacdHistogram,
                eventData.Momentum,
                eventData.RateOfChange,
                eventData.BollingerUpper,
                eventData.BollingerLower,
                eventData.TrendScore,
                eventData.ConfidenceScore,
                Verdict = eventData.Verdict.ToString(),
                eventData.Timestamp
            };

            await _hubContext.Clients.All.SendAsync("marketDataUpdated", payload);
            await _hubContext.Clients.Group(MarketDataHub.BuildSymbolGroup(eventData.Symbol)).SendAsync("marketDataUpdated", payload);

            try
            {
                var input = new GetMarketDataHistoryInput
                {
                    Symbol = eventData.Symbol,
                    Provider = eventData.Provider,
                    Take = 80
                };

                var verdict = await _marketDataAppService.GetRealtimeVerdictAsync(input);
                var timeframeRsi = await _marketDataAppService.GetRelativeStrengthIndexTimeframesAsync(input);
                var verdictPayload = new
                {
                    Verdict = verdict,
                    TimeframeRsi = timeframeRsi.Items
                };

                await _hubContext.Clients.All.SendAsync("marketVerdictUpdated", verdictPayload);
                await _hubContext.Clients.Group(MarketDataHub.BuildSymbolGroup(eventData.Symbol)).SendAsync("marketVerdictUpdated", verdictPayload);
            }
            catch (System.Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to broadcast enriched market verdict update for {Symbol} from {Provider}.",
                    eventData.Symbol,
                    eventData.Provider);
            }
        }

        public async Task HandleEventAsync(TradeAnalysisCompletedEventData eventData)
        {
            var payload = new
            {
                eventData.TradeId,
                eventData.UserId,
                eventData.SnapshotId,
                eventData.RiskScore,
                Recommendation = eventData.Recommendation.ToString(),
                eventData.Narrative,
                eventData.GeneratedAt
            };

            await _hubContext.Clients.Group(MarketDataHub.BuildTradeGroup(eventData.TradeId)).SendAsync("tradeRiskUpdated", payload);
            await _hubContext.Clients.Group(MarketDataHub.BuildUserGroup(eventData.UserId.ToString())).SendAsync("tradeRiskUpdated", payload);
        }

        public async Task HandleEventAsync(TradeExecutedEventData eventData)
        {
            var payload = new
            {
                eventData.TradeId,
                eventData.UserId,
                eventData.Symbol,
                Status = eventData.Status.ToString(),
                eventData.RealizedProfitLoss,
                eventData.OccurredAt
            };

            await _hubContext.Clients.Group(MarketDataHub.BuildTradeGroup(eventData.TradeId)).SendAsync("tradeExecuted", payload);
            await _hubContext.Clients.Group(MarketDataHub.BuildUserGroup(eventData.UserId.ToString())).SendAsync("tradeExecuted", payload);
        }
    }
}
