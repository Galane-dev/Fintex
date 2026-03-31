using Fintex.Investments.MarketData.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.News
{
    public interface INewsAnalysisClient
    {
        Task<NewsRecommendationInsight> AnalyzeAsync(
            string focusKey,
            IReadOnlyList<NewsArticle> articles,
            MarketVerdictDto marketVerdict,
            CancellationToken cancellationToken);
    }
}
