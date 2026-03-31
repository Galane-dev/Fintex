using Fintex.Investments.MarketData.Dto;

namespace Fintex.Investments.PaperTrading
{
    public interface IRecommendationGuardService
    {
        bool ShouldHold(MarketVerdictDto verdict);
    }
}
