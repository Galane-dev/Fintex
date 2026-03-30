using Abp.Dependency;
using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.MarketData
{
    public class MarketProjectionBuilder : IMarketProjectionBuilder, ITransientDependency
    {
        public MarketPriceProjectionDto Build(
            IReadOnlyList<decimal> closes,
            decimal currentPrice,
            DateTime referenceTimestamp,
            int minutesAhead,
            string modelName,
            int smaPeriod,
            int emaPeriod,
            int smmaPeriod,
            decimal? atrPercent)
        {
            var smaProjection = Project(closes, smaPeriod, minutesAhead, currentPrice, CalculateSimpleMovingAverage);
            var emaProjection = Project(closes, emaPeriod, minutesAhead, currentPrice, CalculateExponentialMovingAverage);
            var smmaProjection = Project(closes, smmaPeriod, minutesAhead, currentPrice, CalculateSmoothedMovingAverage);
            var estimates = new[] { smaProjection.Price, emaProjection.Price, smmaProjection.Price }
                .Where(value => value.HasValue)
                .Select(value => value.Value)
                .ToList();

            var effectivePeriod = new[] { smaProjection.EffectivePeriod, emaProjection.EffectivePeriod, smmaProjection.EffectivePeriod }
                .Where(period => period > 0)
                .DefaultIfEmpty(0)
                .Max();
            var barsUsed = closes?.Count ?? 0;
            var confidenceScore = BuildConfidenceScore(estimates, currentPrice, barsUsed, effectivePeriod, atrPercent);

            return new MarketPriceProjectionDto
            {
                Horizon = minutesAhead == 1 ? "1m" : $"{minutesAhead}m",
                MinutesAhead = minutesAhead,
                TargetTimestamp = referenceTimestamp.AddMinutes(minutesAhead),
                ModelName = modelName,
                ConsensusPrice = estimates.Count == 0
                    ? (decimal?)null
                    : decimal.Round(estimates.Average(), 8, MidpointRounding.AwayFromZero),
                SmaPrice = smaProjection.Price,
                EmaPrice = emaProjection.Price,
                SmmaPrice = smmaProjection.Price,
                ConfidenceScore = confidenceScore,
                Maturity = ResolveMaturity(barsUsed, effectivePeriod),
                BarsUsed = barsUsed,
                EffectivePeriod = effectivePeriod
            };
        }

        private static ProjectionResult Project(
            IReadOnlyList<decimal> closes,
            int configuredPeriod,
            int stepsAhead,
            decimal currentPrice,
            Func<IReadOnlyList<decimal>, int, decimal?> calculator)
        {
            if (closes == null || closes.Count < 4)
            {
                return ProjectionResult.Empty;
            }

            var period = Math.Min(configuredPeriod, closes.Count - 1);
            if (period < 3)
            {
                return ProjectionResult.Empty;
            }

            var currentAverage = calculator(closes, period);
            var previousAverage = calculator(closes.Take(closes.Count - 1).ToList(), period);
            if (!currentAverage.HasValue || !previousAverage.HasValue)
            {
                return ProjectionResult.Empty;
            }

            var slope = currentAverage.Value - previousAverage.Value;
            var acceleration = closes.Count >= 3
                ? ((closes[closes.Count - 1] - closes[closes.Count - 2]) - (closes[closes.Count - 2] - closes[closes.Count - 3])) * 0.20m
                : 0m;
            var adjustedSlope = slope + acceleration;
            var driftProjection = currentPrice + (adjustedSlope * stepsAhead);
            var anchorProjection = currentAverage.Value + (adjustedSlope * stepsAhead);
            var blendedProjection = driftProjection + ((anchorProjection - currentPrice) * 0.35m);

            return new ProjectionResult(
                decimal.Round(blendedProjection, 8, MidpointRounding.AwayFromZero),
                period);
        }

        private static decimal? CalculateSimpleMovingAverage(IReadOnlyList<decimal> values, int period)
        {
            return values == null || values.Count < period
                ? (decimal?)null
                : decimal.Round(values.Skip(values.Count - period).Average(), 8, MidpointRounding.AwayFromZero);
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

        private static decimal BuildConfidenceScore(
            IReadOnlyList<decimal> estimates,
            decimal currentPrice,
            int barsUsed,
            int effectivePeriod,
            decimal? atrPercent)
        {
            if (estimates == null || estimates.Count == 0 || effectivePeriod <= 0)
            {
                return 0m;
            }

            var maturityScore = Math.Min(((decimal)effectivePeriod / 20m) * 55m, 55m);
            var barsCoverageScore = Math.Min(((decimal)barsUsed / 40m) * 20m, 20m);
            var meanEstimate = estimates.Average();
            var estimateSpread = meanEstimate == 0m
                ? 0m
                : estimates.Max(value => Math.Abs(value - meanEstimate)) / Math.Abs(meanEstimate) * 100m;
            var agreementScore = Math.Max(0m, 25m - Math.Min(estimateSpread * 55m, 25m));
            var volatilityPenalty = atrPercent.HasValue
                ? Math.Min(Math.Max((atrPercent.Value - 0.50m) * 18m, 0m), 18m)
                : 4m;

            return decimal.Round(Math.Max(0m, maturityScore + barsCoverageScore + agreementScore - volatilityPenalty), 2, MidpointRounding.AwayFromZero);
        }

        private static MarketProjectionMaturity ResolveMaturity(int barsUsed, int effectivePeriod)
        {
            if (barsUsed < 10 || effectivePeriod < 6)
            {
                return MarketProjectionMaturity.WarmingUp;
            }

            if (barsUsed < 24 || effectivePeriod < 12)
            {
                return MarketProjectionMaturity.Forming;
            }

            return MarketProjectionMaturity.Mature;
        }

        private readonly struct ProjectionResult
        {
            public static ProjectionResult Empty => new(null, 0);

            public ProjectionResult(decimal? price, int effectivePeriod)
            {
                Price = price;
                EffectivePeriod = effectivePeriod;
            }

            public decimal? Price { get; }

            public int EffectivePeriod { get; }
        }
    }
}
