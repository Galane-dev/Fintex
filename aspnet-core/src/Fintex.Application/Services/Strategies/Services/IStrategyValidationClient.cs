using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.News;
using Fintex.Investments.Profiles.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// AI client that scores a strategy against current context.
    /// </summary>
    public interface IStrategyValidationClient
    {
        Task<StrategyValidationInsight> ValidateAsync(
            ValidateStrategyRequest request,
            MarketVerdictDto marketVerdict,
            NewsRecommendationInsight newsInsight,
            UserProfileDto profile,
            CancellationToken cancellationToken);
    }
}
