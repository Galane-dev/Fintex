using Abp.Dependency;
using System.Collections.Generic;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Computes raw indicator values and base indicator scores from live price history.
    /// Verdict selection is handled by the dedicated market verdict pipeline.
    /// </summary>
    public partial class IndicatorCalculator : IIndicatorCalculator, ITransientDependency
    {
        private const int SmaPeriod = 20;
        private const int EmaPeriod = 9;
        private const int RsiPeriod = 14;
        private const int StdDevPeriod = 20;
        private const int MomentumPeriod = 14;
        private const int AtrPeriod = 14;
        private const int AdxPeriod = 14;
        private const int MacdFastPeriod = 12;
        private const int MacdSlowPeriod = 26;
        private const int MacdSignalPeriod = 9;
        private const decimal BollingerBandMultiplier = 2m;
        private const decimal SmaWeight = 8m;
        private const decimal EmaWeight = 16m;
        private const decimal RsiWeight = 10m;
        private const decimal MacdWeight = 18m;
        private const decimal MomentumWeight = 8m;
        private const decimal RateOfChangeWeight = 6m;
        private const decimal BollingerWeight = 4m;
        private const decimal VolatilityCautionWeight = 10m;

        public IndicatorSnapshot Calculate(IReadOnlyList<decimal> closingPrices)
        {
            if (closingPrices == null || closingPrices.Count == 0)
            {
                return new IndicatorSnapshot();
            }

            var macdSnapshot = CalculateMacd(closingPrices);
            var bollingerSnapshot = CalculateBollingerBands(closingPrices, StdDevPeriod, BollingerBandMultiplier);

            return new IndicatorSnapshot
            {
                Sma = CalculateSma(closingPrices, SmaPeriod),
                Ema = CalculateEma(closingPrices, EmaPeriod),
                Rsi = CalculateRsi(closingPrices, RsiPeriod),
                StdDev = CalculateStdDev(closingPrices, StdDevPeriod),
                Macd = macdSnapshot.Macd,
                MacdSignal = macdSnapshot.Signal,
                MacdHistogram = macdSnapshot.Histogram,
                Momentum = CalculateMomentum(closingPrices, MomentumPeriod),
                RateOfChange = CalculateRateOfChange(closingPrices, MomentumPeriod),
                BollingerUpper = bollingerSnapshot.Upper,
                BollingerLower = bollingerSnapshot.Lower,
                Scores = BuildIndicatorScores(closingPrices[closingPrices.Count - 1], closingPrices)
            };
        }
    }
}
