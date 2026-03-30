using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Generic;

namespace Fintex.Investments.MarketData
{
    public interface IMarketProjectionBuilder
    {
        MarketPriceProjectionDto Build(
            IReadOnlyList<decimal> closes,
            decimal currentPrice,
            DateTime referenceTimestamp,
            int minutesAhead,
            string modelName,
            int smaPeriod,
            int emaPeriod,
            int smmaPeriod,
            decimal? atrPercent);
    }
}
