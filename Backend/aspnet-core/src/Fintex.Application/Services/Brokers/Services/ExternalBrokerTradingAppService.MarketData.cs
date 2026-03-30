using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.UI;
using Fintex.Investments.MarketData.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    public partial class ExternalBrokerTradingAppService
    {
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
    }
}
