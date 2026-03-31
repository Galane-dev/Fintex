using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.Analytics
{
    public partial class IndicatorCalculator
    {
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

        private static decimal? CalculateMomentum(IReadOnlyList<decimal> values, int period)
        {
            if (values.Count <= period)
            {
                return null;
            }

            return decimal.Round(values[values.Count - 1] - values[values.Count - 1 - period], 8, MidpointRounding.AwayFromZero);
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
            var histogram = signal.HasValue ? latestMacd.Value - signal.Value : (decimal?)null;

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
    }
}
