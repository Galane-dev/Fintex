using Abp.Events.Bus;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    public partial class ExternalBrokerTradingAppService
    {
        public async Task<ExternalBrokerSyncResultDto> SyncMyConnectionsAsync()
        {
            var userId = AbpSession.GetUserId();
            var connections = await _externalBrokerConnectionRepository.GetForUserAsync(userId);
            var activeConnections = connections
                .Where(x => x.IsActive && x.Provider == ExternalBrokerProvider.Alpaca)
                .ToList();

            var result = new ExternalBrokerSyncResultDto();
            foreach (var connection in activeConnections)
            {
                var syncResult = await SyncConnectionAsync(connection, userId);
                result.ImportedTrades += syncResult.ImportedTrades;
                result.UpdatedTrades += syncResult.UpdatedTrades;
                result.ClosedTrades += syncResult.ClosedTrades;
            }

            return result;
        }

        private async Task<ExternalBrokerSyncResultDto> SyncConnectionAsync(ExternalBrokerConnection connection, long userId)
        {
            var environment = connection.Server != null && connection.Server.Contains("paper-api.alpaca.markets", StringComparison.OrdinalIgnoreCase)
                ? "Paper"
                : "Live";
            var access = new AlpacaAccountRequest
            {
                ApiKey = connection.AccountLogin,
                ApiSecret = SimpleStringCipher.Instance.Decrypt(connection.EncryptedPassword),
                IsPaperEnvironment = string.Equals(environment, "Paper", StringComparison.OrdinalIgnoreCase)
            };

            var positions = await _alpacaBrokerService.GetOpenPositionsAsync(access);
            var orders = await _alpacaBrokerService.GetRecentOrdersAsync(access);
            var relevantOpenTrades = await _tradeExecutionContextRepository.GetAll()
                .Where(x => x.UserId == userId && x.ExternalBrokerConnectionId == connection.Id)
                .Join(_tradeRepository.GetAll(), context => context.TradeId, trade => trade.Id, (context, trade) => new { Context = context, Trade = trade })
                .Where(x => x.Trade.Status == TradeStatus.Open)
                .ToListAsync();

            var imported = 0;
            var updated = 0;
            var closed = 0;
            var consumedOrderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var exposureAllocations = positions
                .Select(position => new BrokerExposureAllocation
                {
                    Symbol = MapFromAlpacaSymbol(position.Symbol),
                    Direction = MapAlpacaSide(position.Side),
                    RemainingQuantity = position.Quantity,
                    CurrentPrice = position.CurrentPrice,
                    AverageEntryPrice = position.AverageEntryPrice
                })
                .ToDictionary(
                    allocation => BuildExposureKey(allocation.Symbol, allocation.Direction),
                    allocation => allocation,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var item in relevantOpenTrades.OrderByDescending(x => x.Trade.ExecutedAt))
            {
                var wasUpdated = await SyncOpenTradeAllocationAsync(item.Trade, exposureAllocations);
                if (wasUpdated)
                {
                    updated++;
                    continue;
                }

                var exitPrice = await ResolveLatestPriceAsync(item.Trade.Symbol, item.Trade.Provider);
                item.Trade.Close(exitPrice, DateTime.UtcNow);
                await _tradeRepository.UpdateAsync(item.Trade);
                await PublishTradeExecutedEventAsync(item.Trade, item.Trade.ClosedAt ?? DateTime.UtcNow);
                closed++;
            }

            foreach (var order in GetHistoricalFillCandidates(orders).OrderByDescending(x => x.FilledAt ?? x.SubmittedAt ?? DateTime.MinValue))
            {
                var existingTrade = await _tradeRepository.GetByExternalOrderIdAsync(userId, order.OrderId);
                if (existingTrade != null)
                {
                    consumedOrderIds.Add(order.OrderId);
                    continue;
                }

                if (await TryImportOpenExposureAsync(connection, userId, environment, order, exposureAllocations))
                {
                    imported++;
                    consumedOrderIds.Add(order.OrderId);
                }
            }

            foreach (var order in GetHistoricalFillCandidates(orders).Where(x => !consumedOrderIds.Contains(x.OrderId)))
            {
                await ImportHistoricalClosedTradeAsync(connection, userId, environment, order);
                imported++;
                closed++;
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return new ExternalBrokerSyncResultDto
            {
                ImportedTrades = imported,
                UpdatedTrades = updated,
                ClosedTrades = closed
            };
        }

        private async Task<bool> SyncOpenTradeAllocationAsync(Trade trade, IDictionary<string, BrokerExposureAllocation> exposureAllocations)
        {
            var key = BuildExposureKey(trade.Symbol, trade.Direction);
            if (!exposureAllocations.TryGetValue(key, out var allocation) || allocation.RemainingQuantity <= 0m)
            {
                return false;
            }

            var markPrice = allocation.CurrentPrice ?? await ResolveLatestPriceAsync(trade.Symbol, trade.Provider);
            trade.RefreshMarketPrice(markPrice);
            await _tradeRepository.UpdateAsync(trade);
            allocation.RemainingQuantity = Math.Max(0m, allocation.RemainingQuantity - trade.Quantity);
            return true;
        }

        private static IEnumerable<AlpacaOrderSnapshot> GetHistoricalFillCandidates(IEnumerable<AlpacaOrderSnapshot> orders)
        {
            return orders
                .Where(order => order != null && !string.IsNullOrWhiteSpace(order.OrderId))
                .Where(order => string.Equals(order.Status?.Trim(), "filled", StringComparison.OrdinalIgnoreCase))
                .Where(order => (order.FilledQuantity ?? order.Quantity ?? 0m) > 0m);
        }

        private async Task<bool> TryImportOpenExposureAsync(
            ExternalBrokerConnection connection,
            long userId,
            string environment,
            AlpacaOrderSnapshot order,
            IDictionary<string, BrokerExposureAllocation> exposureAllocations)
        {
            var symbol = MapFromAlpacaSymbol(order.Symbol);
            var direction = MapAlpacaSide(order.Side);
            var key = BuildExposureKey(symbol, direction);
            var orderQuantity = order.FilledQuantity ?? order.Quantity ?? 0m;

            if (orderQuantity <= 0m ||
                !exposureAllocations.TryGetValue(key, out var allocation) ||
                allocation.RemainingQuantity <= 0m)
            {
                return false;
            }

            var importQuantity = decimal.Min(orderQuantity, allocation.RemainingQuantity);
            if (importQuantity <= 0m)
            {
                return false;
            }

            var latestPrice = allocation.CurrentPrice ?? await ResolveLatestPriceAsync(symbol, MarketDataProvider.Binance);
            var importedTrade = new Trade(
                AbpSession.TenantId,
                userId,
                symbol,
                AssetClass.Crypto,
                MarketDataProvider.Binance,
                direction,
                importQuantity,
                order.FilledAveragePrice ?? allocation.AverageEntryPrice ?? latestPrice,
                order.FilledAt ?? order.SubmittedAt ?? DateTime.UtcNow,
                null,
                null,
                order.OrderId,
                "Imported from Alpaca sync.");

            importedTrade.RefreshMarketPrice(latestPrice);
            await _tradeRepository.InsertAsync(importedTrade);
            await CurrentUnitOfWork.SaveChangesAsync();

            await EnsureExecutionContextAsync(
                importedTrade,
                connection,
                environment,
                order,
                latestPrice,
                null,
                null,
                "Imported from Alpaca sync; no Fintex-side click context was available.",
                order.RawJson);

            await PublishTradeExecutedEventAsync(importedTrade, importedTrade.ExecutedAt);
            allocation.RemainingQuantity -= importQuantity;
            return true;
        }

        private async Task ImportHistoricalClosedTradeAsync(
            ExternalBrokerConnection connection,
            long userId,
            string environment,
            AlpacaOrderSnapshot order)
        {
            var symbol = MapFromAlpacaSymbol(order.Symbol);
            var direction = MapAlpacaSide(order.Side);
            var fillPrice = order.FilledAveragePrice ?? await ResolveLatestPriceAsync(symbol, MarketDataProvider.Binance);
            var executedAt = order.FilledAt ?? order.SubmittedAt ?? DateTime.UtcNow;

            var historicalTrade = new Trade(
                AbpSession.TenantId,
                userId,
                symbol,
                AssetClass.Crypto,
                MarketDataProvider.Binance,
                direction,
                order.FilledQuantity ?? order.Quantity ?? 0.00000001m,
                fillPrice,
                executedAt,
                null,
                null,
                order.OrderId,
                "Imported historical Alpaca fill. Broker-side exit context was unavailable, so Fintex mirrored the fill price as the closed-trade marker.");

            historicalTrade.RefreshMarketPrice(fillPrice);
            await _tradeRepository.InsertAsync(historicalTrade);
            await CurrentUnitOfWork.SaveChangesAsync();

            historicalTrade.Close(fillPrice, executedAt);
            await _tradeRepository.UpdateAsync(historicalTrade);

            await EnsureExecutionContextAsync(
                historicalTrade,
                connection,
                environment,
                order,
                fillPrice,
                null,
                null,
                "Imported from Alpaca order history; the broker reported a filled order without an active position snapshot, so Fintex stored the execution for analysis and history visibility.",
                order.RawJson);

            await PublishTradeExecutedEventAsync(historicalTrade, historicalTrade.ClosedAt ?? executedAt);
        }
    }
}
