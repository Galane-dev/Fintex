using Abp.Application.Services.Dto;
using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
        public Task<MarketVerdictDto> GetRealtimeEstimateAsync(GetMarketDataHistoryInput input) =>
            GetRealtimeVerdictAsync(input);

        public async Task<MarketVerdictDto> GetRealtimeVerdictAsync(GetMarketDataHistoryInput input)
        {
            var latest = await _marketDataPointRepository.GetLatestAsync(input.Symbol, input.Provider);
            if (latest == null)
            {
                return null;
            }

            var oneMinuteBars = await GetBarSeriesAsync(input.Symbol, input.Provider, MarketDataTimeframe.OneMinute, VerdictBarTake);
            if (oneMinuteBars.Count == 0)
            {
                return null;
            }

            var primaryCloses = oneMinuteBars.Select(x => x.Close).ToList();
            var primarySnapshot = _indicatorCalculator.Calculate(primaryCloses);
            var atr = _indicatorCalculator.CalculateAtr(oneMinuteBars);
            var adx = _indicatorCalculator.CalculateAdx(oneMinuteBars);
            var atrPercent = atr.HasValue && latest.Price > 0m
                ? decimal.Round((atr.Value / latest.Price) * 100m, 4, MidpointRounding.AwayFromZero)
                : (decimal?)null;
            var structureScore = CalculateMarketStructureScore(oneMinuteBars);
            var timeframeSignals = new List<TimeframeDirectionPoint>();

            foreach (var timeframe in SupportedVerdictTimeframes)
            {
                var bars = timeframe == MarketDataTimeframe.OneMinute
                    ? oneMinuteBars
                    : await GetBarSeriesAsync(input.Symbol, input.Provider, timeframe, VerdictBarTake);
                var timeframeSignal = CalculateTimeframeDirection(timeframe, bars);
                if (timeframeSignal != null)
                {
                    timeframeSignals.Add(timeframeSignal);
                }
            }

            var timeframeAlignmentScore = CalculateTimeframeAlignmentScore(timeframeSignals);
            var baseTrend = primarySnapshot.TrendScore ?? 0m;
            var structureTrend = decimal.Round(structureScore * 100m, 4, MidpointRounding.AwayFromZero);
            var enhancedTrend = decimal.Round(
                Clamp((baseTrend * 0.55m) + (timeframeAlignmentScore * 0.30m) + (structureTrend * 0.15m), -100m, 100m),
                4,
                MidpointRounding.AwayFromZero);

            var coverageScore = CalculateCoverageScore(primarySnapshot, timeframeSignals);
            var consensusScore = Math.Abs(enhancedTrend) * 0.22m;
            var timeframeConfidence = Math.Min(Math.Abs(timeframeAlignmentScore) * 0.18m, 18m);
            var adxBoost = CalculateAdxConfidenceBoost(adx);
            var volatilityPenalty = CalculateVolatilityPenalty(atrPercent);
            var confidenceScore = decimal.Round(
                Clamp(coverageScore + consensusScore + timeframeConfidence + adxBoost - volatilityPenalty, 0m, 100m),
                4,
                MidpointRounding.AwayFromZero);

            return new MarketVerdictDto
            {
                MarketDataPointId = latest.Id,
                Symbol = latest.Symbol,
                Provider = latest.Provider,
                Price = latest.Price,
                TrendScore = enhancedTrend,
                ConfidenceScore = confidenceScore,
                Verdict = CalculateEnhancedVerdict(enhancedTrend, confidenceScore),
                Timestamp = latest.Timestamp,
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
                StructureScore = decimal.Round(structureTrend, 4, MidpointRounding.AwayFromZero),
                StructureLabel = DescribeStructure(structureScore),
                TimeframeAlignmentScore = timeframeAlignmentScore,
                NextOneMinuteProjection = BuildPriceProjection(primaryCloses, latest.Price, latest.Timestamp, 1),
                NextFiveMinuteProjection = BuildPriceProjection(primaryCloses, latest.Price, latest.Timestamp, 5),
                IndicatorScores = BuildEnhancedIndicatorScores(
                    latest.Price,
                    primarySnapshot,
                    atrPercent,
                    adx,
                    structureScore,
                    timeframeAlignmentScore),
                TimeframeSignals = timeframeSignals
                    .Select(x => new MarketVerdictTimeframeDto
                    {
                        Timeframe = x.Timeframe.ToCode(),
                        BiasScore = x.BiasScore,
                        Signal = x.Signal
                    })
                    .ToList()
            };
        }

        private static MarketPriceProjectionDto BuildPriceProjection(
            IReadOnlyList<decimal> closes,
            decimal currentPrice,
            DateTime referenceTimestamp,
            int minutesAhead)
        {
            var smaProjection = ProjectFromMovingAverage(closes, ProjectionSmaPeriod, minutesAhead, currentPrice, CalculateSimpleMovingAverage);
            var emaProjection = ProjectFromMovingAverage(closes, ProjectionEmaPeriod, minutesAhead, currentPrice, CalculateExponentialMovingAverage);
            var smmaProjection = ProjectFromMovingAverage(closes, ProjectionSmmaPeriod, minutesAhead, currentPrice, CalculateSmoothedMovingAverage);
            var estimates = new[] { smaProjection, emaProjection, smmaProjection }
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            return new MarketPriceProjectionDto
            {
                Horizon = minutesAhead == 1 ? "1m" : $"{minutesAhead}m",
                MinutesAhead = minutesAhead,
                TargetTimestamp = referenceTimestamp.AddMinutes(minutesAhead),
                ConsensusPrice = estimates.Count == 0
                    ? (decimal?)null
                    : decimal.Round(estimates.Average(), 8, MidpointRounding.AwayFromZero),
                SmaPrice = smaProjection,
                EmaPrice = emaProjection,
                SmmaPrice = smmaProjection
            };
        }

        private static decimal? ProjectFromMovingAverage(
            IReadOnlyList<decimal> closes,
            int configuredPeriod,
            int stepsAhead,
            decimal currentPrice,
            Func<IReadOnlyList<decimal>, int, decimal?> calculator)
        {
            if (closes == null || closes.Count < 4)
            {
                return null;
            }

            var period = Math.Min(configuredPeriod, closes.Count - 1);
            if (period < 3)
            {
                return null;
            }

            var currentAverage = calculator(closes, period);
            var previousAverage = calculator(closes.Take(closes.Count - 1).ToList(), period);
            if (!currentAverage.HasValue || !previousAverage.HasValue)
            {
                return null;
            }

            var slope = currentAverage.Value - previousAverage.Value;
            var driftProjection = currentPrice + (slope * stepsAhead);
            var anchorProjection = currentAverage.Value + (slope * stepsAhead);
            var blendedProjection = driftProjection + ((anchorProjection - currentPrice) * 0.35m);

            return decimal.Round(blendedProjection, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateSimpleMovingAverage(IReadOnlyList<decimal> values, int period)
        {
            if (values == null || values.Count < period)
            {
                return null;
            }

            return decimal.Round(values.Skip(values.Count - period).Average(), 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateExponentialMovingAverage(IReadOnlyList<decimal> values, int period)
        {
            if (values == null || values.Count < period)
            {
                return null;
            }

            var multiplier = 2m / (period + 1m);
            var average = values.Take(period).Average();
            for (var index = period; index < values.Count; index++)
            {
                average = ((values[index] - average) * multiplier) + average;
            }

            return decimal.Round(average, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateSmoothedMovingAverage(IReadOnlyList<decimal> values, int period)
        {
            if (values == null || values.Count < period)
            {
                return null;
            }

            var average = values.Take(period).Average();
            for (var index = period; index < values.Count; index++)
            {
                average = ((average * (period - 1m)) + values[index]) / period;
            }

            return decimal.Round(average, 8, MidpointRounding.AwayFromZero);
        }
    }
}
