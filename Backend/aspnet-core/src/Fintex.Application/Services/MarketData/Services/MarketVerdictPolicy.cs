using Abp.Dependency;
using System;

namespace Fintex.Investments.MarketData
{
    public class MarketVerdictPolicy : IMarketVerdictPolicy, ITransientDependency
    {
        private const decimal MinimumConfidence = 40m;
        private const decimal MinimumTrendMagnitude = 15m;

        public MarketVerdict ResolveVerdict(decimal trendScore, decimal confidenceScore)
        {
            if (confidenceScore < MinimumConfidence || Math.Abs(trendScore) < MinimumTrendMagnitude)
            {
                return MarketVerdict.Hold;
            }

            return trendScore > 0m ? MarketVerdict.Buy : MarketVerdict.Sell;
        }
    }
}
