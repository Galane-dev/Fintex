using Fintex.Investments.MarketData.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
        private static readonly TimeSpan StaleVerdictAge = TimeSpan.FromMinutes(3);

        public Task<MarketVerdictDto> GetRealtimeEstimateAsync(GetMarketDataHistoryInput input) =>
            GetRealtimeVerdictAsync(input);

        public async Task<MarketVerdictDto> GetRealtimeVerdictAsync(GetMarketDataHistoryInput input)
        {
            var latest = await _marketDataPointRepository.GetLatestAsync(input.Symbol, input.Provider);
            if (latest == null)
            {
                return null;
            }

            var cacheKey = BuildVerdictCacheKey(input.Symbol, input.Provider, latest.Id);
            var cachedVerdict = TryGetCachedVerdict(cacheKey);
            if (cachedVerdict != null)
            {
                return cachedVerdict;
            }

            var oneMinuteBarsTask = GetBarSeriesAsync(input.Symbol, input.Provider, MarketDataTimeframe.OneMinute, VerdictBarTake);
            var fiveMinuteBarsTask = GetBarSeriesAsync(input.Symbol, input.Provider, MarketDataTimeframe.FiveMinutes, VerdictBarTake);
            var fifteenMinuteBarsTask = GetBarSeriesAsync(input.Symbol, input.Provider, MarketDataTimeframe.FifteenMinutes, VerdictBarTake);
            var oneHourBarsTask = GetBarSeriesAsync(input.Symbol, input.Provider, MarketDataTimeframe.OneHour, VerdictBarTake);
            await Task.WhenAll(oneMinuteBarsTask, fiveMinuteBarsTask, fifteenMinuteBarsTask, oneHourBarsTask);

            var oneMinuteBars = oneMinuteBarsTask.Result;
            if (oneMinuteBars.Count == 0)
            {
                var fallbackVerdict = BuildFallbackVerdict(latest, MarketVerdictState.WarmingUp, "Waiting for the first reliable 1m candle sequence.");
                CacheVerdict(cacheKey, fallbackVerdict);
                return fallbackVerdict;
            }

            var primaryCloses = oneMinuteBars.Select(item => item.Close).ToList();
            var primarySnapshot = _indicatorCalculator.Calculate(primaryCloses);
            var atr = _indicatorCalculator.CalculateAtr(oneMinuteBars);
            var adx = _indicatorCalculator.CalculateAdx(oneMinuteBars);
            var atrPercent = atr.HasValue && latest.Price > 0m
                ? decimal.Round((atr.Value / latest.Price) * 100m, 4, MidpointRounding.AwayFromZero)
                : (decimal?)null;
            var structureScore = CalculateMarketStructureScore(oneMinuteBars);

            var timeframeSignals = BuildTimeframeSignals(oneMinuteBars, fiveMinuteBarsTask.Result, fifteenMinuteBarsTask.Result, oneHourBarsTask.Result);
            var timeframeAlignmentScore = CalculateTimeframeAlignmentScore(timeframeSignals);
            var enhancedTrend = BuildEnhancedTrend(primarySnapshot, structureScore, timeframeAlignmentScore);
            var confidenceScore = BuildConfidenceScore(primarySnapshot, timeframeSignals, enhancedTrend, adx, atrPercent);
            var verdict = _marketVerdictPolicy.ResolveVerdict(enhancedTrend, confidenceScore);

            var nextOneMinuteProjection = _marketProjectionBuilder.Build(primaryCloses, latest.Price, latest.Timestamp, 1, "1m-moving-average-drift", ProjectionSmaPeriod, ProjectionEmaPeriod, ProjectionSmmaPeriod, atrPercent);
            var nextFiveMinuteProjection = _marketProjectionBuilder.Build(fiveMinuteBarsTask.Result.Select(item => item.Close).ToList(), latest.Price, latest.Timestamp, 5, "5m-moving-average-drift", ProjectionSmaPeriod, ProjectionEmaPeriod, ProjectionSmmaPeriod, atrPercent);
            var evaluatedAtUtc = DateTime.UtcNow;
            var verdictState = ResolveVerdictState(latest.Timestamp, oneMinuteBars.Count, timeframeSignals.Count, atr, adx, nextOneMinuteProjection, nextFiveMinuteProjection);
            var stateReason = BuildVerdictStateReason(verdictState, oneMinuteBars.Count, timeframeSignals.Count, latest.Timestamp, nextOneMinuteProjection, nextFiveMinuteProjection);

            var liveVerdict = new MarketVerdictDto
            {
                MarketDataPointId = latest.Id,
                Symbol = latest.Symbol,
                Provider = latest.Provider,
                Price = latest.Price,
                TrendScore = enhancedTrend,
                ConfidenceScore = confidenceScore,
                Verdict = verdict,
                VerdictState = verdictState,
                VerdictStateReason = stateReason,
                Timestamp = latest.Timestamp,
                EvaluatedAtUtc = evaluatedAtUtc,
                Sma = primarySnapshot.Sma,
                Ema = primarySnapshot.Ema,
                Rsi = primarySnapshot.Rsi,
                Macd = primarySnapshot.Macd,
                MacdSignal = primarySnapshot.MacdSignal,
                MacdHistogram = primarySnapshot.MacdHistogram,
                Momentum = primarySnapshot.Momentum,
                RateOfChange = primarySnapshot.RateOfChange,
                Atr = atr,
                AtrPercent = atrPercent,
                Adx = adx,
                StructureScore = decimal.Round(structureScore * 100m, 4, MidpointRounding.AwayFromZero),
                StructureLabel = DescribeStructure(structureScore),
                TimeframeAlignmentScore = timeframeAlignmentScore,
                NextOneMinuteProjection = nextOneMinuteProjection,
                NextFiveMinuteProjection = nextFiveMinuteProjection,
                IndicatorScores = BuildEnhancedIndicatorScores(latest.Price, primarySnapshot, atrPercent, adx, structureScore, timeframeAlignmentScore),
                TimeframeSignals = timeframeSignals
                    .Select(item => new MarketVerdictTimeframeDto
                    {
                        Timeframe = item.Timeframe.ToCode(),
                        BiasScore = item.BiasScore,
                        Signal = item.Signal
                    })
                    .ToList()
            };

            CacheVerdict(cacheKey, liveVerdict);
            LogVerdict(liveVerdict);
            return liveVerdict;
        }

        private List<TimeframeDirectionPoint> BuildTimeframeSignals(
            List<MarketDataTimeframeCandle> oneMinuteBars,
            List<MarketDataTimeframeCandle> fiveMinuteBars,
            List<MarketDataTimeframeCandle> fifteenMinuteBars,
            List<MarketDataTimeframeCandle> oneHourBars)
        {
            var barsByTimeframe = new Dictionary<MarketDataTimeframe, List<MarketDataTimeframeCandle>>
            {
                [MarketDataTimeframe.OneMinute] = oneMinuteBars,
                [MarketDataTimeframe.FiveMinutes] = fiveMinuteBars,
                [MarketDataTimeframe.FifteenMinutes] = fifteenMinuteBars,
                [MarketDataTimeframe.OneHour] = oneHourBars
            };

            return SupportedVerdictTimeframes
                .Select(timeframe => CalculateTimeframeDirection(timeframe, barsByTimeframe[timeframe]))
                .Where(signal => signal != null)
                .ToList();
        }

        private static decimal BuildEnhancedTrend(Analytics.IndicatorSnapshot snapshot, decimal structureScore, decimal timeframeAlignmentScore)
        {
            var baseTrend = BuildPrimaryDirectionalBias(snapshot);
            var structureTrend = decimal.Round(structureScore * 100m, 4, MidpointRounding.AwayFromZero);
            return decimal.Round(
                Clamp((baseTrend * 0.55m) + (timeframeAlignmentScore * 0.30m) + (structureTrend * 0.15m), -100m, 100m),
                4,
                MidpointRounding.AwayFromZero);
        }

        private static decimal BuildPrimaryDirectionalBias(Analytics.IndicatorSnapshot snapshot)
        {
            if (snapshot?.Scores == null || snapshot.Scores.Count == 0)
            {
                return 0m;
            }

            var directionalScores = snapshot.Scores
                .Where(score => score.Signal == IndicatorSignal.Bullish || score.Signal == IndicatorSignal.Bearish)
                .ToList();
            if (directionalScores.Count == 0)
            {
                return 0m;
            }

            var signedWeight = directionalScores.Sum(score => score.Signal == IndicatorSignal.Bullish ? score.Score : -score.Score);
            var totalWeight = directionalScores.Sum(score => score.Score);
            return totalWeight <= 0m
                ? 0m
                : decimal.Round(Clamp((signedWeight / totalWeight) * 100m, -100m, 100m), 4, MidpointRounding.AwayFromZero);
        }

        private static decimal BuildConfidenceScore(
            Analytics.IndicatorSnapshot snapshot,
            IReadOnlyList<TimeframeDirectionPoint> timeframeSignals,
            decimal enhancedTrend,
            decimal? adx,
            decimal? atrPercent)
        {
            var coverageScore = CalculateCoverageScore(snapshot, timeframeSignals);
            var consensusScore = Math.Abs(enhancedTrend) * 0.22m;
            var timeframeConfidence = Math.Min(Math.Abs(CalculateTimeframeAlignmentScore(timeframeSignals)) * 0.18m, 18m);
            var adxBoost = CalculateAdxConfidenceBoost(adx);
            var volatilityPenalty = CalculateVolatilityPenalty(atrPercent);
            return decimal.Round(
                Clamp(coverageScore + consensusScore + timeframeConfidence + adxBoost - volatilityPenalty, 0m, 100m),
                4,
                MidpointRounding.AwayFromZero);
        }

        private static MarketVerdictState ResolveVerdictState(
            DateTime latestTimestamp,
            int oneMinuteBarCount,
            int timeframeSignalCount,
            decimal? atr,
            decimal? adx,
            MarketPriceProjectionDto nextOneMinuteProjection,
            MarketPriceProjectionDto nextFiveMinuteProjection)
        {
            if (DateTime.UtcNow - latestTimestamp > StaleVerdictAge)
            {
                return MarketVerdictState.Stale;
            }

            if (oneMinuteBarCount < 20)
            {
                return MarketVerdictState.WarmingUp;
            }

            if (timeframeSignalCount < SupportedVerdictTimeframes.Length || !atr.HasValue || !adx.HasValue)
            {
                return MarketVerdictState.Degraded;
            }

            if (nextOneMinuteProjection?.ConfidenceScore < 40m || nextFiveMinuteProjection?.ConfidenceScore < 35m)
            {
                return MarketVerdictState.Degraded;
            }

            return MarketVerdictState.Live;
        }

        private static string BuildVerdictStateReason(
            MarketVerdictState verdictState,
            int oneMinuteBarCount,
            int timeframeSignalCount,
            DateTime latestTimestamp,
            MarketPriceProjectionDto nextOneMinuteProjection,
            MarketPriceProjectionDto nextFiveMinuteProjection)
        {
            return verdictState switch
            {
                MarketVerdictState.WarmingUp => $"The verdict engine is still warming up and has only {oneMinuteBarCount} one-minute bars.",
                MarketVerdictState.Stale => $"The latest BTC market point is stale from {latestTimestamp:O}.",
                MarketVerdictState.Degraded => $"The verdict is usable but degraded because coverage is incomplete: timeframes={timeframeSignalCount}, projection confidence={nextOneMinuteProjection?.ConfidenceScore?.ToString("0.##") ?? "-"}/{nextFiveMinuteProjection?.ConfidenceScore?.ToString("0.##") ?? "-"}.",
                MarketVerdictState.Fallback => "The verdict is using the last persisted market point because the richer stack is unavailable.",
                _ => "The verdict engine has full live coverage."
            };
        }

        private MarketVerdictDto BuildFallbackVerdict(MarketDataPoint latest, MarketVerdictState verdictState, string reason)
        {
            var fallbackVerdict = latest.Verdict;
            if (latest.ConfidenceScore.GetValueOrDefault() < 40m || Math.Abs(latest.TrendScore.GetValueOrDefault()) < 15m)
            {
                fallbackVerdict = MarketVerdict.Hold;
            }

            return new MarketVerdictDto
            {
                MarketDataPointId = latest.Id,
                Symbol = latest.Symbol,
                Provider = latest.Provider,
                Price = latest.Price,
                TrendScore = latest.TrendScore,
                ConfidenceScore = latest.ConfidenceScore,
                Verdict = fallbackVerdict,
                VerdictState = verdictState,
                VerdictStateReason = reason,
                Timestamp = latest.Timestamp,
                EvaluatedAtUtc = DateTime.UtcNow,
                Sma = latest.Sma,
                Ema = latest.Ema,
                Rsi = latest.Rsi,
                Macd = latest.Macd,
                MacdSignal = latest.MacdSignal,
                MacdHistogram = latest.MacdHistogram,
                Momentum = latest.Momentum,
                RateOfChange = latest.RateOfChange,
                IndicatorScores = new List<IndicatorScoreDto>(),
                TimeframeSignals = new List<MarketVerdictTimeframeDto>()
            };
        }

        private void LogVerdict(MarketVerdictDto verdict)
        {
            _logger.LogDebug(
                "Verdict computed for {Symbol}/{Provider}: state={State}, verdict={Verdict}, confidence={Confidence}, trend={Trend}, p1={Projection1}, p5={Projection5}",
                verdict.Symbol,
                verdict.Provider,
                verdict.VerdictState,
                verdict.Verdict,
                verdict.ConfidenceScore,
                verdict.TrendScore,
                verdict.NextOneMinuteProjection?.ConfidenceScore,
                verdict.NextFiveMinuteProjection?.ConfidenceScore);
        }
    }
}
