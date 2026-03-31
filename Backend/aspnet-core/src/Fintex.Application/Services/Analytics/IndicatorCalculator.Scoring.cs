using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.Analytics
{
    public partial class IndicatorCalculator
    {
        private static IReadOnlyList<IndicatorScore> BuildIndicatorScores(decimal latestPrice, IReadOnlyList<decimal> closingPrices)
        {
            var snapshot = new IndicatorSnapshot
            {
                Sma = CalculateSma(closingPrices, SmaPeriod),
                Ema = CalculateEma(closingPrices, EmaPeriod),
                Rsi = CalculateRsiInternal(closingPrices, RsiPeriod),
                StdDev = CalculateStdDev(closingPrices, StdDevPeriod)
            };
            var macdSnapshot = CalculateMacd(closingPrices);
            snapshot.Macd = macdSnapshot.Macd;
            snapshot.MacdSignal = macdSnapshot.Signal;
            snapshot.MacdHistogram = macdSnapshot.Histogram;
            snapshot.Momentum = CalculateMomentum(closingPrices, MomentumPeriod);
            snapshot.RateOfChange = CalculateRateOfChange(closingPrices, MomentumPeriod);
            var bollinger = CalculateBollingerBands(closingPrices, StdDevPeriod, BollingerBandMultiplier);
            snapshot.BollingerUpper = bollinger.Upper;
            snapshot.BollingerLower = bollinger.Lower;

            var weightedScores = new List<WeightedIndicatorScore>();
            if (snapshot.Sma.HasValue && snapshot.Sma.Value > 0m)
            {
                weightedScores.Add(CreateDirectionalScore("SMA", snapshot.Sma.Value, Clamp(((latestPrice - snapshot.Sma.Value) / snapshot.Sma.Value) / 0.02m, -1m, 1m), SmaWeight));
            }
            if (snapshot.Ema.HasValue && snapshot.Ema.Value > 0m)
            {
                weightedScores.Add(CreateDirectionalScore("EMA", snapshot.Ema.Value, Clamp(((latestPrice - snapshot.Ema.Value) / snapshot.Ema.Value) / 0.015m, -1m, 1m), EmaWeight));
            }
            if (snapshot.Rsi.HasValue)
            {
                weightedScores.Add(CreateDirectionalScore("RSI", snapshot.Rsi.Value, NormalizeRsi(snapshot.Rsi.Value), RsiWeight));
            }
            if (snapshot.MacdHistogram.HasValue && latestPrice > 0m)
            {
                weightedScores.Add(CreateDirectionalScore("MACD Histogram", snapshot.MacdHistogram.Value, Clamp((snapshot.MacdHistogram.Value / latestPrice) / 0.0025m, -1m, 1m), MacdWeight));
            }
            if (snapshot.Momentum.HasValue && latestPrice > 0m)
            {
                weightedScores.Add(CreateDirectionalScore("Momentum", snapshot.Momentum.Value, Clamp((snapshot.Momentum.Value / latestPrice) / 0.02m, -1m, 1m), MomentumWeight));
            }
            if (snapshot.RateOfChange.HasValue)
            {
                weightedScores.Add(CreateDirectionalScore("Rate Of Change", snapshot.RateOfChange.Value, Clamp(snapshot.RateOfChange.Value / 3m, -1m, 1m), RateOfChangeWeight));
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
                    weightedScores.Add(CreateCautionScore("Volatility", snapshot.StdDev.Value, caution, VolatilityCautionWeight));
                }
            }

            return weightedScores.Select(score => score.Score).ToList();
        }

        private static WeightedIndicatorScore CreateDirectionalScore(string name, decimal value, decimal normalizedStrength, decimal weight)
        {
            var clamped = Clamp(normalizedStrength, -1m, 1m);
            var signal = clamped > 0m ? IndicatorSignal.Bullish : clamped < 0m ? IndicatorSignal.Bearish : IndicatorSignal.Neutral;
            return new WeightedIndicatorScore(weight, clamped, new IndicatorScore(name, decimal.Round(value, 8, MidpointRounding.AwayFromZero), decimal.Round(Math.Abs(clamped) * 100m, 4, MidpointRounding.AwayFromZero), signal));
        }

        private static WeightedIndicatorScore CreateCautionScore(string name, decimal value, decimal cautionStrength, decimal weight)
        {
            var clamped = Clamp(cautionStrength, 0m, 1m);
            var signal = clamped > 0m ? IndicatorSignal.Caution : IndicatorSignal.Neutral;
            return new WeightedIndicatorScore(weight, 0m, new IndicatorScore(name, decimal.Round(value, 8, MidpointRounding.AwayFromZero), decimal.Round(clamped * 100m, 4, MidpointRounding.AwayFromZero), signal));
        }

        private static decimal NormalizeRsi(decimal rsi)
        {
            if (rsi <= 30m) return Clamp(((30m - rsi) / 15m) + 0.20m, 0m, 1m);
            if (rsi >= 70m) return -Clamp(((rsi - 70m) / 15m) + 0.20m, 0m, 1m);
            if (rsi < 45m) return Clamp((45m - rsi) / 30m, 0m, 0.45m);
            if (rsi > 55m) return -Clamp((rsi - 55m) / 30m, 0m, 0.45m);
            return 0m;
        }

        private static decimal NormalizeBollinger(decimal latestPrice, decimal lowerBand, decimal upperBand)
        {
            if (upperBand <= lowerBand) return 0m;
            if (latestPrice > upperBand) return -Clamp(((latestPrice - upperBand) / upperBand) / 0.01m, 0m, 1m);
            if (latestPrice < lowerBand) return Clamp(((lowerBand - latestPrice) / lowerBand) / 0.01m, 0m, 1m);
            return 0m;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
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
            public WeightedIndicatorScore(decimal weight, decimal signedStrength, IndicatorScore score)
            {
                Weight = weight;
                SignedStrength = signedStrength;
                Score = score;
            }

            public decimal Weight { get; }
            public decimal SignedStrength { get; }
            public IndicatorScore Score { get; }
        }
    }
}
