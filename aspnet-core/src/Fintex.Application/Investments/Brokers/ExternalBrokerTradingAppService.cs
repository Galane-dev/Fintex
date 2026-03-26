using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Events;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Trading.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    /// <summary>
    /// Routes live trade execution through connected external broker accounts.
    /// </summary>
    [AbpAuthorize]
    public class ExternalBrokerTradingAppService : FintexAppServiceBase, IExternalBrokerTradingAppService
    {
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;
        private readonly IAlpacaBrokerService _alpacaBrokerService;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IMarketDataAppService _marketDataAppService;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IRepository<TradeExecutionContext, long> _tradeExecutionContextRepository;
        private readonly IEventBus _eventBus;

        public ExternalBrokerTradingAppService(
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository,
            IAlpacaBrokerService alpacaBrokerService,
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataAppService marketDataAppService,
            IUserProfileRepository userProfileRepository,
            ITradeRepository tradeRepository,
            IRepository<TradeExecutionContext, long> tradeExecutionContextRepository,
            IEventBus eventBus)
        {
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
            _alpacaBrokerService = alpacaBrokerService;
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataAppService = marketDataAppService;
            _userProfileRepository = userProfileRepository;
            _tradeRepository = tradeRepository;
            _tradeExecutionContextRepository = tradeExecutionContextRepository;
            _eventBus = eventBus;
        }

        public async Task<ExternalBrokerTradeExecutionDto> PlaceMarketOrderAsync(PlaceExternalBrokerMarketOrderInput input)
        {
            var userId = AbpSession.GetUserId();
            var connection = await _externalBrokerConnectionRepository.GetByIdForUserAsync(input.ConnectionId, userId);
            if (connection == null || !connection.IsActive)
            {
                throw new UserFriendlyException("The external broker connection could not be found.");
            }

            if (connection.Provider != ExternalBrokerProvider.Alpaca)
            {
                throw new UserFriendlyException("This external broker connection is not supported for live routing yet.");
            }

            var marketContext = await GetMarketContextAsync(input.Symbol, input.Provider);
            var apiSecret = SimpleStringCipher.Instance.Decrypt(connection.EncryptedPassword);
            var environment = connection.Server != null && connection.Server.Contains("paper-api.alpaca.markets", StringComparison.OrdinalIgnoreCase)
                ? "Paper"
                : "Live";
            var brokerSymbol = MapToAlpacaSymbol(input.Symbol);
            var clientOrderId = BuildClientOrderId(userId);

            var brokerRequest = new AlpacaPlaceOrderRequest
            {
                ApiKey = connection.AccountLogin,
                ApiSecret = apiSecret,
                IsPaperEnvironment = string.Equals(environment, "Paper", StringComparison.OrdinalIgnoreCase),
                Symbol = brokerSymbol,
                Direction = input.Direction,
                Quantity = input.Quantity,
                ClientOrderId = clientOrderId,
                UseBracketExits = input.AssetClass != AssetClass.Crypto &&
                    input.StopLoss.HasValue &&
                    input.TakeProfit.HasValue,
                StopLoss = input.StopLoss,
                TakeProfit = input.TakeProfit
            };

            var brokerResult = await _alpacaBrokerService.PlaceMarketOrderAsync(brokerRequest);
            if (!brokerResult.IsSuccess)
            {
                connection.MarkFailed(brokerResult.Error, DateTime.UtcNow);
                await CurrentUnitOfWork.SaveChangesAsync();
                throw new UserFriendlyException(brokerResult.Error ?? "The live broker order could not be placed.");
            }

            var executedAt = brokerResult.FilledAt ?? brokerResult.SubmittedAt ?? DateTime.UtcNow;
            var entryPrice = brokerResult.FilledAveragePrice ?? marketContext.LatestPoint.Price;

            var trade = new Trade(
                AbpSession.TenantId,
                userId,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                input.Quantity,
                entryPrice,
                executedAt,
                input.StopLoss,
                input.TakeProfit,
                brokerResult.OrderId,
                input.Notes);

            trade.RefreshMarketPrice(marketContext.LatestPoint.Price);
            await _tradeRepository.InsertAsync(trade);
            await CurrentUnitOfWork.SaveChangesAsync();

            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            var executionContext = new TradeExecutionContext(
                AbpSession.TenantId,
                trade.Id,
                userId,
                connection.Id,
                connection.Provider,
                connection.Platform,
                environment,
                brokerSymbol,
                input.Direction,
                input.AssetClass,
                input.Provider,
                input.Quantity,
                marketContext.LatestPoint.Price,
                marketContext.LatestPoint.Bid,
                marketContext.LatestPoint.Ask,
                input.StopLoss,
                input.TakeProfit,
                input.Notes);

            executionContext.ApplyMarketContext(
                marketContext.LatestPoint,
                marketContext.Spread,
                marketContext.SpreadPercent);
            executionContext.ApplyVerdictContext(
                marketContext.Verdict?.Verdict ?? MarketVerdict.Hold,
                marketContext.Verdict?.TrendScore,
                marketContext.Verdict?.ConfidenceScore,
                marketContext.Verdict?.TimeframeAlignmentScore,
                marketContext.Verdict?.StructureScore,
                marketContext.Verdict?.StructureLabel,
                marketContext.Verdict?.Atr,
                marketContext.Verdict?.AtrPercent,
                marketContext.Verdict?.Adx,
                BuildDecisionSummary(input.Direction, marketContext.Verdict));
            executionContext.ApplyUserContext(
                profile?.RiskTolerance,
                profile?.BehavioralRiskScore,
                profile?.BehavioralSummary);
            executionContext.ApplyBrokerExecution(
                brokerResult.OrderId,
                brokerResult.ClientOrderId,
                brokerResult.Status,
                brokerResult.SubmittedQuantity,
                brokerResult.FilledQuantity,
                brokerResult.FilledAveragePrice,
                brokerResult.SubmittedAt,
                brokerResult.FilledAt,
                BuildRequestPayloadJson(input, brokerSymbol, clientOrderId),
                brokerResult.ResponseJson);

            await _tradeExecutionContextRepository.InsertAsync(executionContext);

            var probeResult = await _alpacaBrokerService.ProbeConnectionAsync(new AlpacaConnectionProbeRequest
            {
                ApiKey = connection.AccountLogin,
                ApiSecret = apiSecret,
                IsPaperEnvironment = string.Equals(environment, "Paper", StringComparison.OrdinalIgnoreCase)
            });

            if (probeResult.IsSuccess)
            {
                connection.MarkConnected(
                    probeResult.AccountNumber,
                    probeResult.Currency,
                    probeResult.Company,
                    probeResult.Multiplier,
                    probeResult.Cash,
                    probeResult.Equity,
                    DateTime.UtcNow);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _eventBus.TriggerAsync(new TradeExecutedEventData
            {
                TenantId = trade.TenantId,
                TradeId = trade.Id,
                UserId = trade.UserId,
                Symbol = trade.Symbol,
                Status = trade.Status,
                RealizedProfitLoss = trade.RealizedProfitLoss,
                OccurredAt = trade.ExecutedAt
            });

            return new ExternalBrokerTradeExecutionDto
            {
                ConnectionId = connection.Id,
                BrokerName = connection.DisplayName,
                BrokerEnvironment = environment,
                BrokerSymbol = brokerSymbol,
                BrokerOrderId = brokerResult.OrderId,
                BrokerOrderStatus = brokerResult.Status,
                FilledAveragePrice = brokerResult.FilledAveragePrice,
                Trade = ObjectMapper.Map<TradeDto>(trade),
                Headline = input.Direction == TradeDirection.Buy
                    ? "Live Alpaca buy order submitted."
                    : "Live Alpaca sell order submitted.",
                Summary = BuildExecutionSummary(environment, brokerResult, marketContext.Verdict)
            };
        }

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

        private async Task<LiveTradeMarketContext> GetMarketContextAsync(string symbol, MarketDataProvider provider)
        {
            var latestPoint = await GetLatestPointAsync(symbol, provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException(
                    $"No live market price is available yet for {symbol?.Trim()?.ToUpperInvariant()} from {provider}.");
            }

            var verdict = await _marketDataAppService.GetRealtimeVerdictAsync(new GetMarketDataHistoryInput
            {
                Symbol = symbol,
                Provider = provider,
                Take = 80
            });

            decimal? spread = null;
            decimal? spreadPercent = null;
            if (latestPoint.Bid.HasValue &&
                latestPoint.Ask.HasValue &&
                latestPoint.Ask.Value >= latestPoint.Bid.Value)
            {
                spread = decimal.Round(latestPoint.Ask.Value - latestPoint.Bid.Value, 4, MidpointRounding.AwayFromZero);
                spreadPercent = latestPoint.Price > 0m
                    ? decimal.Round((spread.Value / latestPoint.Price) * 100m, 4, MidpointRounding.AwayFromZero)
                    : (decimal?)null;
            }

            return new LiveTradeMarketContext
            {
                LatestPoint = latestPoint,
                Verdict = verdict,
                Spread = spread,
                SpreadPercent = spreadPercent
            };
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
            var openTrades = await _tradeRepository.GetUserOpenTradesAsync(userId);
            var relevantOpenTrades = await _tradeExecutionContextRepository.GetAll()
                .Where(x => x.UserId == userId && x.ExternalBrokerConnectionId == connection.Id)
                .Join(
                    _tradeRepository.GetAll(),
                    context => context.TradeId,
                    trade => trade.Id,
                    (context, trade) => new { Context = context, Trade = trade })
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
                var key = BuildExposureKey(item.Trade.Symbol, item.Trade.Direction);
                if (!exposureAllocations.TryGetValue(key, out var allocation) || allocation.RemainingQuantity <= 0m)
                {
                    var exitPrice = await ResolveLatestPriceAsync(item.Trade.Symbol, item.Trade.Provider);
                    item.Trade.Close(exitPrice, DateTime.UtcNow);
                    await _tradeRepository.UpdateAsync(item.Trade);
                    closed++;

                    await _eventBus.TriggerAsync(new TradeExecutedEventData
                    {
                        TenantId = item.Trade.TenantId,
                        TradeId = item.Trade.Id,
                        UserId = item.Trade.UserId,
                        Symbol = item.Trade.Symbol,
                        Status = item.Trade.Status,
                        RealizedProfitLoss = item.Trade.RealizedProfitLoss,
                        OccurredAt = item.Trade.ClosedAt ?? DateTime.UtcNow
                    });
                    continue;
                }

                var markPrice = allocation.CurrentPrice ?? await ResolveLatestPriceAsync(item.Trade.Symbol, item.Trade.Provider);
                item.Trade.RefreshMarketPrice(markPrice);
                await _tradeRepository.UpdateAsync(item.Trade);
                updated++;
                allocation.RemainingQuantity = Math.Max(0m, allocation.RemainingQuantity - item.Trade.Quantity);
            }

            foreach (var order in orders
                .Where(IsHistoricalFilledOrderCandidate)
                .Where(x => !string.IsNullOrWhiteSpace(x.OrderId))
                .OrderByDescending(x => x.FilledAt ?? x.SubmittedAt ?? DateTime.MinValue))
            {
                var existingTrade = await _tradeRepository.GetByExternalOrderIdAsync(userId, order.OrderId);
                if (existingTrade != null)
                {
                    consumedOrderIds.Add(order.OrderId);
                    continue;
                }

                var symbol = MapFromAlpacaSymbol(order.Symbol);
                var direction = MapAlpacaSide(order.Side);
                var key = BuildExposureKey(symbol, direction);
                var orderQuantity = order.FilledQuantity ?? order.Quantity ?? 0m;
                if (orderQuantity <= 0m)
                {
                    continue;
                }

                if (exposureAllocations.TryGetValue(key, out var allocation) && allocation.RemainingQuantity > 0m)
                {
                    var importQuantity = decimal.Min(orderQuantity, allocation.RemainingQuantity);
                    if (importQuantity > 0m)
                    {
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

                        await _eventBus.TriggerAsync(new TradeExecutedEventData
                        {
                            TenantId = importedTrade.TenantId,
                            TradeId = importedTrade.Id,
                            UserId = importedTrade.UserId,
                            Symbol = importedTrade.Symbol,
                            Status = importedTrade.Status,
                            RealizedProfitLoss = importedTrade.RealizedProfitLoss,
                            OccurredAt = importedTrade.ExecutedAt
                        });

                        imported++;
                        allocation.RemainingQuantity -= importQuantity;
                        consumedOrderIds.Add(order.OrderId);
                    }
                }
            }

            foreach (var order in orders
                .Where(IsHistoricalFilledOrderCandidate)
                .Where(x => !string.IsNullOrWhiteSpace(x.OrderId))
                .Where(x => !consumedOrderIds.Contains(x.OrderId)))
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

                await _eventBus.TriggerAsync(new TradeExecutedEventData
                {
                    TenantId = historicalTrade.TenantId,
                    TradeId = historicalTrade.Id,
                    UserId = historicalTrade.UserId,
                    Symbol = historicalTrade.Symbol,
                    Status = historicalTrade.Status,
                    RealizedProfitLoss = historicalTrade.RealizedProfitLoss,
                    OccurredAt = historicalTrade.ClosedAt ?? executedAt
                });

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

        private static bool IsHistoricalFilledOrderCandidate(AlpacaOrderSnapshot order)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.OrderId))
            {
                return false;
            }

            var status = order.Status?.Trim();
            if (!string.Equals(status, "filled", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return (order.FilledQuantity ?? order.Quantity ?? 0m) > 0m;
        }

        private static string BuildExposureKey(string symbol, TradeDirection direction)
        {
            return $"{symbol?.Trim()?.ToUpperInvariant()}::{direction}";
        }

        private async Task<MarketDataPoint> GetLatestPointAsync(string symbol, MarketDataProvider provider)
        {
            MarketDataPoint latestPoint;
            var alternateSymbol = GetAlternateMarketSymbol(symbol, provider);

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                latestPoint = await _marketDataPointRepository.GetLatestAsync(symbol, provider);
                if (latestPoint == null && !string.IsNullOrWhiteSpace(alternateSymbol))
                {
                    latestPoint = await _marketDataPointRepository.GetLatestAsync(alternateSymbol, provider);
                }

                if (latestPoint == null)
                {
                    latestPoint = await _marketDataPointRepository.GetLatestBySymbolAsync(symbol);
                }

                if (latestPoint == null && !string.IsNullOrWhiteSpace(alternateSymbol))
                {
                    latestPoint = await _marketDataPointRepository.GetLatestBySymbolAsync(alternateSymbol);
                }
            }

            return latestPoint;
        }

        private async Task<decimal> ResolveLatestPriceAsync(string symbol, MarketDataProvider provider)
        {
            var latestPoint = await GetLatestPointAsync(symbol, provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException(
                    $"No live market price is available yet for {symbol?.Trim()?.ToUpperInvariant()} from {provider}.");
            }

            return latestPoint.Price;
        }

        private static string MapToAlpacaSymbol(string symbol)
        {
            var normalized = (symbol ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.EndsWith("USDT", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 4) + "/USD";
            }

            if (normalized.EndsWith("USD", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "/USD";
            }

            throw new UserFriendlyException("Only USD-quoted crypto symbols are supported for Alpaca routing right now.");
        }

        private static string GetAlternateMarketSymbol(string symbol, MarketDataProvider provider)
        {
            if (provider != MarketDataProvider.Binance || string.IsNullOrWhiteSpace(symbol))
            {
                return null;
            }

            var normalized = symbol.Trim().ToUpperInvariant().Replace("/", string.Empty, StringComparison.Ordinal);
            if (normalized.EndsWith("USD", StringComparison.Ordinal) &&
                !normalized.EndsWith("USDT", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "USDT";
            }

            return null;
        }

        private static string MapFromAlpacaSymbol(string symbol)
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

        private static TradeDirection MapAlpacaSide(string side)
        {
            return string.Equals(side, "sell", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(side, "short", StringComparison.OrdinalIgnoreCase)
                ? TradeDirection.Sell
                : TradeDirection.Buy;
        }

        private static string BuildClientOrderId(long userId)
        {
            return $"fintex-{userId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        private static string BuildExecutionSummary(
            string environment,
            AlpacaPlaceOrderResult brokerResult,
            MarketVerdictDto verdict)
        {
            var filledPrice = brokerResult.FilledAveragePrice.HasValue
                ? brokerResult.FilledAveragePrice.Value.ToString("0.########", CultureInfo.InvariantCulture)
                : "pending";
            var verdictLabel = verdict == null ? "market verdict pending" : $"{verdict.Verdict} bias";
            return $"The order was routed to Alpaca {environment.ToLowerInvariant()} with broker status {brokerResult.Status ?? "unknown"} at approx. {filledPrice}. Current market read: {verdictLabel}.";
        }

        private static string BuildDecisionSummary(TradeDirection direction, MarketVerdictDto verdict)
        {
            if (verdict == null)
            {
                return $"Live {direction} order was placed before the realtime verdict stack was available.";
            }

            return $"Live {direction} order aligned against a {verdict.Verdict} market stance with confidence {verdict.ConfidenceScore?.ToString("0.##", CultureInfo.InvariantCulture) ?? "-"} and trend {verdict.TrendScore?.ToString("0.##", CultureInfo.InvariantCulture) ?? "-"}.";
        }

        private static string BuildRequestPayloadJson(PlaceExternalBrokerMarketOrderInput input, string brokerSymbol, string clientOrderId)
        {
            return JsonSerializer.Serialize(new
            {
                connectionId = input.ConnectionId,
                symbol = input.Symbol,
                brokerSymbol,
                direction = input.Direction.ToString(),
                quantity = input.Quantity,
                stopLoss = input.StopLoss,
                takeProfit = input.TakeProfit,
                notes = input.Notes,
                clientOrderId
            });
        }

        private async Task EnsureExecutionContextAsync(
            Trade trade,
            ExternalBrokerConnection connection,
            string environment,
            AlpacaOrderSnapshot matchingOrder,
            decimal latestPrice,
            decimal? bid,
            decimal? ask,
            string decisionSummary,
            string rawOrderPayload)
        {
            var existingContext = await _tradeExecutionContextRepository.GetAll()
                .FirstOrDefaultAsync(x => x.TradeId == trade.Id);
            if (existingContext != null)
            {
                return;
            }

            var profile = await _userProfileRepository.GetByUserIdAsync(trade.UserId);
            var context = new TradeExecutionContext(
                trade.TenantId,
                trade.Id,
                trade.UserId,
                connection.Id,
                connection.Provider,
                connection.Platform,
                environment,
                matchingOrder?.Symbol ?? MapToAlpacaSymbol(trade.Symbol),
                trade.Direction,
                trade.AssetClass,
                trade.Provider,
                trade.Quantity,
                latestPrice,
                bid,
                ask,
                trade.StopLoss,
                trade.TakeProfit,
                trade.Notes);

            context.ApplyUserContext(
                profile?.RiskTolerance,
                profile?.BehavioralRiskScore,
                profile?.BehavioralSummary);
            context.ApplyVerdictContext(
                MarketVerdict.Hold,
                null,
                null,
                null,
                null,
                "Imported",
                null,
                null,
                null,
                decisionSummary);
            context.ApplyBrokerExecution(
                matchingOrder?.OrderId,
                matchingOrder?.ClientOrderId,
                matchingOrder?.Status,
                matchingOrder?.Quantity,
                matchingOrder?.FilledQuantity,
                matchingOrder?.FilledAveragePrice,
                matchingOrder?.SubmittedAt,
                matchingOrder?.FilledAt,
                null,
                rawOrderPayload);

            await _tradeExecutionContextRepository.InsertAsync(context);
        }

        private sealed class LiveTradeMarketContext
        {
            public MarketDataPoint LatestPoint { get; set; }

            public MarketVerdictDto Verdict { get; set; }

            public decimal? Spread { get; set; }

            public decimal? SpreadPercent { get; set; }
        }

        private sealed class BrokerExposureAllocation
        {
            public string Symbol { get; set; }

            public TradeDirection Direction { get; set; }

            public decimal RemainingQuantity { get; set; }

            public decimal? CurrentPrice { get; set; }

            public decimal? AverageEntryPrice { get; set; }
        }
    }
}
