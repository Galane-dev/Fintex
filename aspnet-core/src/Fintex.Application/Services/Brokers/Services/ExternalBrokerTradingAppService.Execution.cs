using Abp.Events.Bus;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Events;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Trading.Dto;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    public partial class ExternalBrokerTradingAppService
    {
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

            var brokerResult = await _alpacaBrokerService.PlaceMarketOrderAsync(new AlpacaPlaceOrderRequest
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
            });

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
            await RefreshConnectionHealthAsync(connection, apiSecret, environment);
            await CurrentUnitOfWork.SaveChangesAsync();
            await PublishTradeExecutedEventAsync(trade, trade.ExecutedAt);

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

        private async Task RefreshConnectionHealthAsync(ExternalBrokerConnection connection, string apiSecret, string environment)
        {
            var probeResult = await _alpacaBrokerService.ProbeConnectionAsync(new AlpacaConnectionProbeRequest
            {
                ApiKey = connection.AccountLogin,
                ApiSecret = apiSecret,
                IsPaperEnvironment = string.Equals(environment, "Paper", StringComparison.OrdinalIgnoreCase)
            });

            if (!probeResult.IsSuccess)
            {
                return;
            }

            connection.MarkConnected(
                probeResult.AccountNumber,
                probeResult.Currency,
                probeResult.Company,
                probeResult.Multiplier,
                probeResult.Cash,
                probeResult.Equity,
                DateTime.UtcNow);
        }

        private async Task PublishTradeExecutedEventAsync(Trade trade, DateTime occurredAt)
        {
            await _eventBus.TriggerAsync(new TradeExecutedEventData
            {
                TenantId = trade.TenantId,
                TradeId = trade.Id,
                UserId = trade.UserId,
                Symbol = trade.Symbol,
                Status = trade.Status,
                RealizedProfitLoss = trade.RealizedProfitLoss,
                OccurredAt = occurredAt
            });
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
    }
}
