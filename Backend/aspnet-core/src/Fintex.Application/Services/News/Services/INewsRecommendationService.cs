using Fintex.Investments.MarketData.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.News
{
    public interface INewsRecommendationService
    {
        Task<NewsRecommendationInsight> GetBitcoinUsdInsightAsync(
            MarketVerdictDto marketVerdict,
            CancellationToken cancellationToken);
    }
}
