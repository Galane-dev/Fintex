using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.Analytics
{
    public partial class IndicatorCalculator
    {
        public decimal? CalculateRsi(IReadOnlyList<decimal> values, int period = RsiPeriod)
        {
            return CalculateRsiInternal(values, period);
        }

        private static decimal? CalculateRsiInternal(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count <= period)
            {
                return null;
            }

            decimal totalGain = 0m;
            decimal totalLoss = 0m;
            for (var index = 1; index <= period; index++)
            {
                var delta = values[index] - values[index - 1];
                if (delta >= 0m) totalGain += delta;
                else totalLoss -= delta;
            }

            var averageGain = totalGain / period;
            var averageLoss = totalLoss / period;
            for (var index = period + 1; index < values.Count; index++)
            {
                var delta = values[index] - values[index - 1];
                var gain = delta > 0m ? delta : 0m;
                var loss = delta < 0m ? -delta : 0m;
                averageGain = ((averageGain * (period - 1m)) + gain) / period;
                averageLoss = ((averageLoss * (period - 1m)) + loss) / period;
            }

            if (averageLoss == 0m && averageGain == 0m) return 50m;
            if (averageLoss == 0m) return 100m;
            if (averageGain == 0m) return 0m;

            var relativeStrength = averageGain / averageLoss;
            var rsi = 100m - (100m / (1m + relativeStrength));
            return decimal.Round(rsi, 8, MidpointRounding.AwayFromZero);
        }

        public decimal? CalculateAtr(IReadOnlyList<MarketDataTimeframeCandle> candles, int period = AtrPeriod)
        {
            if (candles == null || candles.Count <= period)
            {
                return null;
            }

            var trueRanges = new List<decimal>();
            for (var index = 1; index < candles.Count; index++)
            {
                var current = candles[index];
                var previousClose = candles[index - 1].Close;
                trueRanges.Add(Math.Max(current.High - current.Low, Math.Max(Math.Abs(current.High - previousClose), Math.Abs(current.Low - previousClose))));
            }

            if (trueRanges.Count < period)
            {
                return null;
            }

            var atr = trueRanges.Take(period).Average();
            for (var index = period; index < trueRanges.Count; index++)
            {
                atr = ((atr * (period - 1m)) + trueRanges[index]) / period;
            }

            return decimal.Round(atr, 8, MidpointRounding.AwayFromZero);
        }

        public decimal? CalculateAdx(IReadOnlyList<MarketDataTimeframeCandle> candles, int period = AdxPeriod)
        {
            if (candles == null || candles.Count <= period * 2)
            {
                return null;
            }

            var trueRanges = new List<decimal>();
            var positiveDirectionalMoves = new List<decimal>();
            var negativeDirectionalMoves = new List<decimal>();

            for (var index = 1; index < candles.Count; index++)
            {
                var current = candles[index];
                var previous = candles[index - 1];
                var upMove = current.High - previous.High;
                var downMove = previous.Low - current.Low;

                positiveDirectionalMoves.Add(upMove > downMove && upMove > 0m ? upMove : 0m);
                negativeDirectionalMoves.Add(downMove > upMove && downMove > 0m ? downMove : 0m);
                trueRanges.Add(Math.Max(current.High - current.Low, Math.Max(Math.Abs(current.High - previous.Close), Math.Abs(current.Low - previous.Close))));
            }

            var atr = trueRanges.Take(period).Sum();
            var positiveDm = positiveDirectionalMoves.Take(period).Sum();
            var negativeDm = negativeDirectionalMoves.Take(period).Sum();
            var dxValues = new List<decimal>();

            for (var index = period; index < trueRanges.Count; index++)
            {
                if (index > period)
                {
                    atr = atr - (atr / period) + trueRanges[index];
                    positiveDm = positiveDm - (positiveDm / period) + positiveDirectionalMoves[index];
                    negativeDm = negativeDm - (negativeDm / period) + negativeDirectionalMoves[index];
                }

                if (atr <= 0m)
                {
                    continue;
                }

                var positiveDi = (positiveDm / atr) * 100m;
                var negativeDi = (negativeDm / atr) * 100m;
                var diSum = positiveDi + negativeDi;
                dxValues.Add(diSum <= 0m ? 0m : (Math.Abs(positiveDi - negativeDi) / diSum) * 100m);
            }

            if (dxValues.Count < period)
            {
                return null;
            }

            var adx = dxValues.Take(period).Average();
            for (var index = period; index < dxValues.Count; index++)
            {
                adx = ((adx * (period - 1m)) + dxValues[index]) / period;
            }

            return decimal.Round(adx, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateStdDev(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count < period)
            {
                return null;
            }

            var window = values.Skip(values.Count - period).Select(Convert.ToDouble).ToArray();
            var average = window.Average();
            var variance = window.Select(value => Math.Pow(value - average, 2d)).Average();
            return decimal.Round((decimal)Math.Sqrt(variance), 8, MidpointRounding.AwayFromZero);
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
    }
}
