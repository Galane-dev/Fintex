using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Computes realtime indicators and a market-only estimate from live price history.
    /// </summary>
    public class IndicatorCalculator : IIndicatorCalculator, ITransientDependency
    {
        private const int IndicatorPeriod = 14;
        private const int StdDevPeriod = 20;
        private const int MomentumPeriod = 10;
        private const int MacdFastPeriod = 12;
        private const int MacdSlowPeriod = 26;
        private const int MacdSignalPeriod = 9;
        private const decimal BollingerBandMultiplier = 2m;
        private const decimal SmaWeight = 12m;
        private const decimal EmaWeight = 15m;
        private const decimal RsiWeight = 16m;
        private const decimal MacdWeight = 20m;
        private const decimal MomentumWeight = 12m;
        private const decimal RateOfChangeWeight = 10m;
        private const decimal BollingerWeight = 8m;
        private const decimal VolatilityCautionWeight = 15m;
        private const decimal TotalDirectionalWeight = SmaWeight + EmaWeight + RsiWeight + MacdWeight + MomentumWeight + RateOfChangeWeight + BollingerWeight;

        public IndicatorSnapshot Calculate(IReadOnlyList<decimal> closingPrices)
        {
            if (closingPrices == null || closingPrices.Count == 0)
            {
                return new IndicatorSnapshot();
            }

            var macdSnapshot = CalculateMacd(closingPrices);
            var bollingerSnapshot = CalculateBollingerBands(closingPrices, StdDevPeriod, BollingerBandMultiplier);
            var snapshot = new IndicatorSnapshot
            {
                Sma = CalculateSma(closingPrices, IndicatorPeriod),
                Ema = CalculateEma(closingPrices, IndicatorPeriod),
                Rsi = CalculateRsi(closingPrices, IndicatorPeriod),
                StdDev = CalculateStdDev(closingPrices, StdDevPeriod),
                Macd = macdSnapshot.Macd,
                MacdSignal = macdSnapshot.Signal,
                MacdHistogram = macdSnapshot.Histogram,
                Momentum = CalculateMomentum(closingPrices, MomentumPeriod),
                RateOfChange = CalculateRateOfChange(closingPrices, MomentumPeriod),
                BollingerUpper = bollingerSnapshot.Upper,
                BollingerLower = bollingerSnapshot.Lower
            };

            var estimate = CalculateEstimate(closingPrices[closingPrices.Count - 1], snapshot);
            snapshot.TrendScore = estimate.TrendScore;
            snapshot.ConfidenceScore = estimate.ConfidenceScore;
            snapshot.Verdict = estimate.Verdict;
            snapshot.Scores = estimate.Scores;

            return snapshot;
        }

        private static decimal? CalculateSma(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count < period)
            {
                return null;
            }

            return decimal.Round(values.Skip(values.Count - period).Average(), 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateEma(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count < period)
            {
                return null;
            }

            var multiplier = 2m / (period + 1m);
            var ema = values.Take(period).Average();

            for (var index = period; index < values.Count; index++)
            {
                ema = ((values[index] - ema) * multiplier) + ema;
            }

            return decimal.Round(ema, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateRsi(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count <= period)
            {
                return null;
            }

            decimal gains = 0m;
            decimal losses = 0m;

            for (var index = values.Count - period; index < values.Count; index++)
            {
                var previous = values[index - 1];
                var current = values[index];
                var delta = current - previous;

                if (delta >= 0m)
                {
                    gains += delta;
                }
                else
                {
                    losses -= delta;
                }
            }

            if (losses == 0m)
            {
                return 100m;
            }

            var relativeStrength = gains / losses;
            var rsi = 100m - (100m / (1m + relativeStrength));
            return decimal.Round(rsi, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateStdDev(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count < period)
            {
                return null;
            }

            var window = values.Skip(values.Count - period).Select(Convert.ToDouble).ToArray();
            var average = window.Average();
            var variance = window.Select(x => Math.Pow(x - average, 2d)).Average();
            return decimal.Round((decimal)Math.Sqrt(variance), 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateMomentum(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count <= period)
            {
                return null;
            }

            var momentum = values[values.Count - 1] - values[values.Count - 1 - period];
            return decimal.Round(momentum, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateRateOfChange(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count <= period)
            {
                return null;
            }

            var basePrice = values[values.Count - 1 - period];
            if (basePrice == 0m)
            {
                return null;
            }

            var roc = ((values[values.Count - 1] - basePrice) / basePrice) * 100m;
            return decimal.Round(roc, 8, MidpointRounding.AwayFromZero);
        }

        private static MacdSnapshot CalculateMacd(IReadOnlyList<decimal> values)
        {
            var fastSeries = BuildEmaSeries(values, MacdFastPeriod);
            var slowSeries = BuildEmaSeries(values, MacdSlowPeriod);

            var macdSeries = new List<decimal>();
            decimal? latestMacd = null;

            for (var index = 0; index < values.Count; index++)
            {
                if (!fastSeries[index].HasValue || !slowSeries[index].HasValue)
                {
                    continue;
                }

                latestMacd = fastSeries[index].Value - slowSeries[index].Value;
                macdSeries.Add(latestMacd.Value);
            }

            if (!latestMacd.HasValue)
            {
                return new MacdSnapshot();
            }

            var signal = CalculateEma(macdSeries, MacdSignalPeriod);
            var histogram = signal.HasValue
                ? latestMacd.Value - signal.Value
                : (decimal?)null;

            return new MacdSnapshot
            {
                Macd = decimal.Round(latestMacd.Value, 8, MidpointRounding.AwayFromZero),
                Signal = signal,
                Histogram = histogram.HasValue
                    ? decimal.Round(histogram.Value, 8, MidpointRounding.AwayFromZero)
                    : (decimal?)null
            };
        }

        private static List<decimal?> BuildEmaSeries(IReadOnlyList<decimal> values, int period)
        {
            var series = new List<decimal?>(Enumerable.Repeat((decimal?)null, values.Count));
            if (values.Count < period)
            {
                return series;
            }

            var multiplier = 2m / (period + 1m);
            var ema = values.Take(period).Average();
            series[period - 1] = decimal.Round(ema, 8, MidpointRounding.AwayFromZero);

            for (var index = period; index < values.Count; index++)
            {
                ema = ((values[index] - ema) * multiplier) + ema;
                series[index] = decimal.Round(ema, 8, MidpointRounding.AwayFromZero);
            }

            return series;
        }

        private static BollingerSnapshot CalculateBollingerBands(IReadOnlyList<decimal> values, int period, decimal multiplier)
        {
            var sma = CalculateSma(values, period);
            var stdDev = CalculateStdDev(values, period);
            if (!sma.HasValue || !stdDev.HasValue)
            {
                return new BollingerSnapshot();
            }

            return new BollingerSnapshot
            {
                Upper = decimal.Round(sma.Value + (stdDev.Value * multiplier), 8, MidpointRounding.AwayFromZero),
                Lower = decimal.Round(sma.Value - (stdDev.Value * multiplier), 8, MidpointRounding.AwayFromZero)
            };
        }

        private static EstimateSnapshot CalculateEstimate(decimal latestPrice, IndicatorSnapshot snapshot)
        {
            var weightedScores = new List<WeightedIndicatorScore>();
            decimal volatilityPenalty = 0m;

            if (snapshot.Sma.HasValue && snapshot.Sma.Value > 0m)
            {
                var normalized = Clamp(((latestPrice - snapshot.Sma.Value) / snapshot.Sma.Value) / 0.02m, -1m, 1m);
                weightedScores.Add(CreateDirectionalScore("SMA", snapshot.Sma.Value, normalized, SmaWeight));
            }

            if (snapshot.Ema.HasValue && snapshot.Ema.Value > 0m)
            {
                var normalized = Clamp(((latestPrice - snapshot.Ema.Value) / snapshot.Ema.Value) / 0.015m, -1m, 1m);
                weightedScores.Add(CreateDirectionalScore("EMA", snapshot.Ema.Value, normalized, EmaWeight));
            }

            if (snapshot.Rsi.HasValue)
            {
                weightedScores.Add(CreateDirectionalScore("RSI", snapshot.Rsi.Value, NormalizeRsi(snapshot.Rsi.Value), RsiWeight));
            }

            if (snapshot.MacdHistogram.HasValue && latestPrice > 0m)
            {
                var normalized = Clamp((snapshot.MacdHistogram.Value / latestPrice) / 0.0025m, -1m, 1m);
                weightedScores.Add(CreateDirectionalScore("MACD Histogram", snapshot.MacdHistogram.Value, normalized, MacdWeight));
            }

            if (snapshot.Momentum.HasValue && latestPrice > 0m)
            {
                var normalized = Clamp((snapshot.Momentum.Value / latestPrice) / 0.02m, -1m, 1m);
                weightedScores.Add(CreateDirectionalScore("Momentum", snapshot.Momentum.Value, normalized, MomentumWeight));
            }

            if (snapshot.RateOfChange.HasValue)
            {
                var normalized = Clamp(snapshot.RateOfChange.Value / 3m, -1m, 1m);
                weightedScores.Add(CreateDirectionalScore("Rate Of Change", snapshot.RateOfChange.Value, normalized, RateOfChangeWeight));
            }

            if (snapshot.BollingerUpper.HasValue && snapshot.BollingerLower.HasValue)
            {
                var normalized = NormalizeBollinger(latestPrice, snapshot.BollingerLower.Value, snapshot.BollingerUpper.Value);
                if (normalized != 0m)
                {
                    weightedScores.Add(CreateDirectionalScore("Bollinger Bands", latestPrice, normalized, BollingerWeight));
                }
            }

            if (snapshot.StdDev.HasValue && latestPrice > 0m)
            {
                var caution = Clamp((snapshot.StdDev.Value / latestPrice) / 0.01m, 0m, 1m);
                if (caution > 0m)
                {
                    volatilityPenalty = caution * 25m;
                    weightedScores.Add(CreateCautionScore("Volatility", snapshot.StdDev.Value, caution, VolatilityCautionWeight));
                }
            }

            var directionalScores = weightedScores.Where(x => x.Score.Signal == IndicatorSignal.Bullish || x.Score.Signal == IndicatorSignal.Bearish).ToList();
            var activeWeight = directionalScores.Sum(x => x.Weight);
            var weightedDirection = directionalScores.Sum(x => x.Weight * x.SignedStrength);
            var trendScore = activeWeight <= 0m
                ? 0m
                : decimal.Round((weightedDirection / activeWeight) * 100m, 4, MidpointRounding.AwayFromZero);

            var coverageScore = TotalDirectionalWeight <= 0m
                ? 0m
                : (activeWeight / TotalDirectionalWeight) * 60m;
            var consensusScore = Math.Abs(trendScore) * 0.4m;
            var confidenceScore = decimal.Round(Clamp(coverageScore + consensusScore - volatilityPenalty, 0m, 100m), 4, MidpointRounding.AwayFromZero);

            return new EstimateSnapshot
            {
                TrendScore = trendScore,
                ConfidenceScore = confidenceScore,
                Verdict = CalculateVerdict(trendScore, confidenceScore),
                Scores = weightedScores.Select(x => x.Score).ToList()
            };
        }

        private static WeightedIndicatorScore CreateDirectionalScore(string name, decimal value, decimal normalizedStrength, decimal weight)
        {
            var clamped = Clamp(normalizedStrength, -1m, 1m);
            var signal = clamped > 0m
                ? IndicatorSignal.Bullish
                : clamped < 0m
                    ? IndicatorSignal.Bearish
                    : IndicatorSignal.Neutral;

            return new WeightedIndicatorScore
            {
                Weight = weight,
                SignedStrength = clamped,
                Score = new IndicatorScore(
                    name,
                    decimal.Round(value, 8, MidpointRounding.AwayFromZero),
                    decimal.Round(Math.Abs(clamped) * 100m, 4, MidpointRounding.AwayFromZero),
                    signal)
            };
        }

        private static WeightedIndicatorScore CreateCautionScore(string name, decimal value, decimal cautionStrength, decimal weight)
        {
            var clamped = Clamp(cautionStrength, 0m, 1m);
            return new WeightedIndicatorScore
            {
                Weight = weight,
                SignedStrength = 0m,
                Score = new IndicatorScore(
                    name,
                    decimal.Round(value, 8, MidpointRounding.AwayFromZero),
                    decimal.Round(clamped * 100m, 4, MidpointRounding.AwayFromZero),
                    clamped > 0m ? IndicatorSignal.Caution : IndicatorSignal.Neutral)
            };
        }

        private static decimal NormalizeRsi(decimal rsi)
        {
            if (rsi >= 75m)
            {
                return -Clamp((rsi - 75m) / 15m, 0m, 1m);
            }

            if (rsi <= 25m)
            {
                return Clamp((25m - rsi) / 15m, 0m, 1m);
            }

            return Clamp((rsi - 50m) / 20m, -1m, 1m);
        }

        private static decimal NormalizeBollinger(decimal latestPrice, decimal lowerBand, decimal upperBand)
        {
            if (upperBand <= lowerBand)
            {
                return 0m;
            }

            if (latestPrice > upperBand)
            {
                return -Clamp(((latestPrice - upperBand) / upperBand) / 0.01m, 0m, 1m);
            }

            if (latestPrice < lowerBand)
            {
                return Clamp(((lowerBand - latestPrice) / lowerBand) / 0.01m, 0m, 1m);
            }

            return 0m;
        }

        private static MarketVerdict CalculateVerdict(decimal trendScore, decimal confidenceScore)
        {
            if (confidenceScore < 35m || Math.Abs(trendScore) < 12m)
            {
                return MarketVerdict.Hold;
            }

            return trendScore > 0m ? MarketVerdict.Buy : MarketVerdict.Sell;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private sealed class MacdSnapshot
        {
            public decimal? Macd { get; set; }

            public decimal? Signal { get; set; }

            public decimal? Histogram { get; set; }
        }

        private sealed class BollingerSnapshot
        {
            public decimal? Upper { get; set; }

            public decimal? Lower { get; set; }
        }

        private sealed class WeightedIndicatorScore
        {
            public IndicatorScore Score { get; set; }

            public decimal Weight { get; set; }

            public decimal SignedStrength { get; set; }
        }

        private sealed class EstimateSnapshot
        {
            public decimal TrendScore { get; set; }

            public decimal ConfidenceScore { get; set; }

            public MarketVerdict Verdict { get; set; }

            public IReadOnlyList<IndicatorScore> Scores { get; set; }
        }
    }
}
