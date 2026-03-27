using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.UI;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        private async Task<PaperTradeMarketContext> GetMarketContextAsync(string symbol, MarketDataProvider provider)
        {
            var latestPoint = await GetLatestPointAsync(symbol, provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException(
                    $"No live market price is available yet for {symbol?.Trim()?.ToUpperInvariant()} from {provider}.");
            }

            var realtimeVerdict = await _marketDataAppService.GetRealtimeVerdictAsync(new GetMarketDataHistoryInput
            {
                Symbol = symbol,
                Provider = provider,
                Take = 80
            });
            realtimeVerdict ??= BuildFallbackVerdict(latestPoint);

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

            return new PaperTradeMarketContext
            {
                LatestPoint = latestPoint,
                RealtimeVerdict = realtimeVerdict,
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

        private static MarketVerdictDto BuildFallbackVerdict(MarketDataPoint latestPoint)
        {
            if (latestPoint == null)
            {
                return null;
            }

            var hasDirectionalSignal =
                latestPoint.TrendScore.HasValue ||
                latestPoint.ConfidenceScore.HasValue ||
                latestPoint.Verdict != MarketVerdict.Hold;

            if (!hasDirectionalSignal)
            {
                return null;
            }

            return new MarketVerdictDto
            {
                MarketDataPointId = latestPoint.Id,
                Symbol = latestPoint.Symbol,
                Provider = latestPoint.Provider,
                Price = latestPoint.Price,
                TrendScore = latestPoint.TrendScore,
                ConfidenceScore = latestPoint.ConfidenceScore,
                Verdict = latestPoint.Verdict,
                Timestamp = latestPoint.Timestamp,
                Sma = latestPoint.Sma,
                Ema = latestPoint.Ema,
                Rsi = latestPoint.Rsi,
                Macd = latestPoint.Macd,
                MacdSignal = latestPoint.MacdSignal,
                MacdHistogram = latestPoint.MacdHistogram,
                Momentum = latestPoint.Momentum,
                RateOfChange = latestPoint.RateOfChange,
                StructureLabel = "Waiting",
                IndicatorScores = new List<IndicatorScoreDto>(),
                TimeframeSignals = new List<MarketVerdictTimeframeDto>()
            };
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
    }
}
