using Abp.Dependency;
using Fintex.Investments.MarketData.Dto;
using System;

namespace Fintex.Investments.PaperTrading
{
    public class RecommendationGuardService : IRecommendationGuardService, ITransientDependency
    {
        public bool ShouldHold(MarketVerdictDto verdict)
        {
            return verdict == null ||
                verdict.VerdictState != MarketVerdictState.Live ||
                verdict.Verdict == MarketVerdict.Hold ||
                !verdict.ConfidenceScore.HasValue ||
                verdict.ConfidenceScore.Value < 45m ||
                !verdict.TrendScore.HasValue ||
                Math.Abs(verdict.TrendScore.Value) < 15m;
        }
    }
}
