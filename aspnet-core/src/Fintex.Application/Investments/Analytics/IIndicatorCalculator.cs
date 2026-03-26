using System.Collections.Generic;
using Fintex.Investments;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Calculates trading indicators from a rolling price series.
    /// </summary>
    public interface IIndicatorCalculator
    {
        IndicatorSnapshot Calculate(IReadOnlyList<decimal> closingPrices);

        decimal? CalculateRsi(IReadOnlyList<decimal> closingPrices, int period = 14);

        decimal? CalculateAtr(IReadOnlyList<MarketDataTimeframeCandle> candles, int period = 14);

        decimal? CalculateAdx(IReadOnlyList<MarketDataTimeframeCandle> candles, int period = 14);
    }
}
