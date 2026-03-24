using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Computes SMA, EMA, RSI, and standard deviation for live market data.
    /// </summary>
    public class IndicatorCalculator : IIndicatorCalculator, ITransientDependency
    {
        private const int IndicatorPeriod = 14;
        private const int StdDevPeriod = 20;

        public IndicatorSnapshot Calculate(IReadOnlyList<decimal> closingPrices)
        {
            if (closingPrices == null || closingPrices.Count == 0)
            {
                return new IndicatorSnapshot();
            }

            return new IndicatorSnapshot
            {
                Sma = CalculateSma(closingPrices, IndicatorPeriod),
                Ema = CalculateEma(closingPrices, IndicatorPeriod),
                Rsi = CalculateRsi(closingPrices, IndicatorPeriod),
                StdDev = CalculateStdDev(closingPrices, StdDevPeriod)
            };
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
    }
}
