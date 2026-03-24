using System.Collections.Generic;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Calculates trading indicators from a rolling price series.
    /// </summary>
    public interface IIndicatorCalculator
    {
        IndicatorSnapshot Calculate(IReadOnlyList<decimal> closingPrices);
    }
}
