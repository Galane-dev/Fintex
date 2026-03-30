using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Fintex.Investments.Events;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Sends notification inbox and email updates for trade fill lifecycle events.
    /// </summary>
    public class TradeExecutionNotificationHandler : IAsyncEventHandler<TradeExecutedEventData>, ITransientDependency
    {
        private readonly INotificationDispatchService _notificationDispatchService;

        public TradeExecutionNotificationHandler(INotificationDispatchService notificationDispatchService)
        {
            _notificationDispatchService = notificationDispatchService;
        }

        public async Task HandleEventAsync(TradeExecutedEventData eventData)
        {
            if (eventData == null || eventData.UserId <= 0 || string.IsNullOrWhiteSpace(eventData.Symbol))
            {
                return;
            }

            await _notificationDispatchService.DispatchAsync(new NotificationDispatchRequest
            {
                TenantId = eventData.TenantId,
                UserId = eventData.UserId,
                Type = NotificationType.TradeFill,
                Severity = ResolveSeverity(eventData),
                Title = BuildTitle(eventData),
                Message = BuildMessage(eventData),
                Symbol = eventData.Symbol,
                Provider = eventData.Provider,
                ReferencePrice = eventData.ExecutionPrice,
                ConfidenceScore = null,
                Verdict = null,
                TriggerKey = BuildTriggerKey(eventData),
                NotifyInApp = true,
                NotifyEmail = true,
                ContextJson = BuildContextJson(eventData),
                OccurredAt = eventData.OccurredAt
            });
        }

        private static NotificationSeverity ResolveSeverity(TradeExecutedEventData eventData)
        {
            if (eventData.Status == TradeStatus.Cancelled)
            {
                return NotificationSeverity.Warning;
            }

            if (eventData.Status == TradeStatus.Closed && eventData.RealizedProfitLoss.GetValueOrDefault() < 0m)
            {
                return NotificationSeverity.Warning;
            }

            return NotificationSeverity.Success;
        }

        private static string BuildTitle(TradeExecutedEventData eventData)
        {
            var source = string.IsNullOrWhiteSpace(eventData.Source) ? "Trade" : eventData.Source;
            return eventData.Status switch
            {
                TradeStatus.Closed => $"{source} trade closed",
                TradeStatus.Cancelled => $"{source} trade canceled",
                _ => $"{source} fill completed"
            };
        }

        private static string BuildMessage(TradeExecutedEventData eventData)
        {
            var direction = eventData.Direction.ToString().ToLowerInvariant();
            var quantity = eventData.Quantity?.ToString("0.########", CultureInfo.InvariantCulture) ?? "-";
            var price = eventData.ExecutionPrice?.ToString("0.########", CultureInfo.InvariantCulture) ?? "market";

            if (eventData.Status == TradeStatus.Closed)
            {
                var pnl = eventData.RealizedProfitLoss.HasValue
                    ? eventData.RealizedProfitLoss.Value.ToString("0.########", CultureInfo.InvariantCulture)
                    : "-";
                return $"{eventData.Symbol} closed after a {direction} fill of {quantity} near {price}. Realized P/L: {pnl}.";
            }

            if (eventData.Status == TradeStatus.Cancelled)
            {
                return $"{eventData.Symbol} received a broker cancellation/update after the {direction} order flow.";
            }

            return $"{eventData.Symbol} filled {direction} for {quantity} near {price}.";
        }

        private static string BuildTriggerKey(TradeExecutedEventData eventData)
        {
            return $"trade-fill:{eventData.Source ?? "trade"}:{eventData.TradeId}:{eventData.Status}:{eventData.OccurredAt:O}";
        }

        private static string BuildContextJson(TradeExecutedEventData eventData)
        {
            return $"{{\"tradeId\":{eventData.TradeId},\"status\":\"{eventData.Status}\",\"direction\":\"{eventData.Direction}\",\"quantity\":{eventData.Quantity?.ToString("0.########", CultureInfo.InvariantCulture) ?? "null"},\"price\":{eventData.ExecutionPrice?.ToString("0.########", CultureInfo.InvariantCulture) ?? "null"},\"source\":\"{(eventData.Source ?? "trade").Replace("\"", "'")}\"}}";
        }
    }
}
