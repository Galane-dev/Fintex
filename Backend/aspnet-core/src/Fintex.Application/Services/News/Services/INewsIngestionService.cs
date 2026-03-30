using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.News
{
    public interface INewsIngestionService
    {
        Task<NewsRefreshRun> EnsureFreshAsync(string focusKey, string trigger, CancellationToken cancellationToken);

        Task<IReadOnlyList<NewsArticle>> GetRecentRelevantArticlesAsync(string focusKey, int take, CancellationToken cancellationToken);

        Task<DateTime?> GetLatestRelevantPublishedAtAsync(string focusKey, CancellationToken cancellationToken);
    }
}
