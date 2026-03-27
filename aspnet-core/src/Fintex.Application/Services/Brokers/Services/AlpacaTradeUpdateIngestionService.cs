using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Fintex.Investments.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    /// <summary>
    /// Captures raw Alpaca trade update events and applies lightweight trade-state updates when possible.
    /// </summary>
    public class AlpacaTradeUpdateIngestionService : IAlpacaTradeUpdateIngestionService, ITransientDependency
    {
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;
        private readonly IRepository<ExternalBrokerExecutionEvent, long> _executionEventRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IEventBus _eventBus;

        public AlpacaTradeUpdateIngestionService(
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository,
            IRepository<ExternalBrokerExecutionEvent, long> executionEventRepository,
            ITradeRepository tradeRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IEventBus eventBus)
        {
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
            _executionEventRepository = executionEventRepository;
            _tradeRepository = tradeRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _eventBus = eventBus;
        }

        public async Task CaptureAsync(long connectionId, AlpacaTradeUpdateMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.EventType))
            {
                return;
            }

            var connection = await _externalBrokerConnectionRepository.FirstOrDefaultAsync(connectionId);
            if (connection == null || connection.Provider != ExternalBrokerProvider.Alpaca)
            {
                return;
            }

            var duplicate = await _executionEventRepository.GetAll()
                .AnyAsync(x =>
                    x.ExternalBrokerConnectionId == connectionId &&
                    x.BrokerOrderId == message.OrderId &&
                    x.EventType == message.EventType &&
                    x.ExecutionId == message.ExecutionId);
            if (!duplicate)
            {
                var normalizedSymbol = NormalizeSymbol(message.Symbol);
                var executionEvent = new ExternalBrokerExecutionEvent(
                    connection.TenantId,
                    connection.UserId,
                    connection.Id,
                    connection.Provider,
                    connection.Platform,
                    ResolveEnvironment(connection),
                    message.EventType,
                    message.ExecutionId,
                    message.OrderId,
                    message.ClientOrderId,
                    message.Symbol,
                    normalizedSymbol,
                    message.OrderStatus,
                    MapSide(message.Side),
                    AssetClass.Crypto,
                    message.OrderQuantity,
                    message.FilledQuantity,
                    message.EventQuantity,
                    message.Price,
                    message.FilledAveragePrice,
                    message.PositionQuantity,
                    message.OccurredAt,
                    message.RawPayloadJson);

                await _executionEventRepository.InsertAsync(executionEvent);
            }

            await ApplyLightweightTradeStateAsync(connection.UserId, message);
        }

        private async Task ApplyLightweightTradeStateAsync(long userId, AlpacaTradeUpdateMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.OrderId))
            {
                return;
            }

            var trade = await _tradeRepository.GetByExternalOrderIdAsync(userId, message.OrderId);
            if (trade == null)
            {
                return;
            }

            if (string.Equals(message.EventType, "canceled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(message.EventType, "rejected", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(message.EventType, "expired", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(message.EventType, "order_cancel_rejected", StringComparison.OrdinalIgnoreCase))
            {
                if (trade.Status == TradeStatus.Open)
                {
                    trade.Cancel("Broker update: " + (message.OrderStatus ?? message.EventType));
                    await _tradeRepository.UpdateAsync(trade);

                    await _eventBus.TriggerAsync(new TradeExecutedEventData
                    {
                        TenantId = trade.TenantId,
                        TradeId = trade.Id,
                        UserId = trade.UserId,
                        Symbol = trade.Symbol,
                        Status = trade.Status,
                        RealizedProfitLoss = trade.RealizedProfitLoss,
                        OccurredAt = message.OccurredAt ?? DateTime.UtcNow
                    });
                }

                return;
            }

            if (!string.Equals(message.EventType, "fill", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(message.EventType, "partial_fill", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var marketPrice = message.Price;
            if (!marketPrice.HasValue)
            {
                var normalizedSymbol = NormalizeSymbol(message.Symbol);
                if (!string.IsNullOrWhiteSpace(normalizedSymbol))
                {
                    var latestPoint = await _marketDataPointRepository.GetLatestAsync(normalizedSymbol, MarketDataProvider.Binance)
                        ?? await _marketDataPointRepository.GetLatestBySymbolAsync(normalizedSymbol);
                    marketPrice = latestPoint?.Price;
                }
            }

            if (marketPrice.HasValue)
            {
                trade.RefreshMarketPrice(marketPrice.Value);
                await _tradeRepository.UpdateAsync(trade);
            }

            if (message.PositionQuantity.HasValue && message.PositionQuantity.Value == 0m && trade.Status == TradeStatus.Open)
            {
                var exitPrice = message.Price ?? message.FilledAveragePrice ?? marketPrice;
                if (exitPrice.HasValue)
                {
                    trade.Close(exitPrice.Value, message.OccurredAt ?? DateTime.UtcNow);
                    await _tradeRepository.UpdateAsync(trade);

                    await _eventBus.TriggerAsync(new TradeExecutedEventData
                    {
                        TenantId = trade.TenantId,
                        TradeId = trade.Id,
                        UserId = trade.UserId,
                        Symbol = trade.Symbol,
                        Status = trade.Status,
                        RealizedProfitLoss = trade.RealizedProfitLoss,
                        OccurredAt = trade.ClosedAt ?? DateTime.UtcNow
                    });
                }
            }
        }

        private static string NormalizeSymbol(string symbol)
        {
            var normalized = (symbol ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.EndsWith("/USD", StringComparison.Ordinal))
            {
                return normalized.Replace("/USD", "USDT", StringComparison.Ordinal);
            }

            if (normalized.EndsWith("USD", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "USDT";
            }

            return normalized.Replace("/", string.Empty, StringComparison.Ordinal);
        }

        private static TradeDirection? MapSide(string side)
        {
            if (string.IsNullOrWhiteSpace(side))
            {
                return null;
            }

            return side.Equals("sell", StringComparison.OrdinalIgnoreCase) ||
                   side.Equals("short", StringComparison.OrdinalIgnoreCase)
                ? TradeDirection.Sell
                : TradeDirection.Buy;
        }

        private static string ResolveEnvironment(ExternalBrokerConnection connection)
        {
            return connection.Server != null && connection.Server.Contains("paper-api.alpaca.markets", StringComparison.OrdinalIgnoreCase)
                ? "Paper"
                : "Live";
        }
    }
}
