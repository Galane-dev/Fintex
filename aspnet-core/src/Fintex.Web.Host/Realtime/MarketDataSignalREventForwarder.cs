using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Fintex.Investments.Events;
using Fintex.Web.Host.Hubs;
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

        public MarketDataSignalREventForwarder(IHubContext<MarketDataHub> hubContext)
        {
            _hubContext = hubContext;
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
                eventData.Timestamp
            };

            await _hubContext.Clients.All.SendAsync("marketDataUpdated", payload);
            await _hubContext.Clients.Group(MarketDataHub.BuildSymbolGroup(eventData.Symbol)).SendAsync("marketDataUpdated", payload);
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
