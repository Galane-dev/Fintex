using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.News
{
    public partial class NewsIngestionService
    {
        public async Task<NewsRefreshRun> EnsureFreshAsync(string focusKey, string trigger, CancellationToken cancellationToken)
        {
            await EnsureDefaultSourcesAsync();

            var refreshWindowMinutes = Math.Max(5, _configuration.GetValue<int?>("News:RefreshWindowMinutes") ?? 30);
            var freshCutoff = DateTime.UtcNow.AddMinutes(-refreshWindowMinutes);
            var lastCompletedRun = await GetLastCompletedRunAsync(focusKey, cancellationToken);

            if (IsRunFresh(lastCompletedRun, freshCutoff))
            {
                return await CreateSkippedRunAsync(focusKey, trigger, "Skipped refresh because the news cache is still fresh.");
            }

            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                lastCompletedRun = await GetLastCompletedRunAsync(focusKey, cancellationToken);
                if (IsRunFresh(lastCompletedRun, freshCutoff))
                {
                    return await CreateSkippedRunAsync(focusKey, trigger, "Skipped refresh because another request refreshed the cache.");
                }

                return await ExecuteRefreshAsync(focusKey, trigger, cancellationToken);
            }
            catch (Exception ex)
            {
                var failedRun = new NewsRefreshRun(AbpSession.TenantId, focusKey, trigger, DateTime.UtcNow);
                failedRun.MarkFailed(0, 0, 0, ex.Message, DateTime.UtcNow);
                await _newsRefreshRunRepository.InsertAsync(failedRun);
                await CurrentUnitOfWork.SaveChangesAsync();
                return failedRun;
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        public async Task<IReadOnlyList<NewsArticle>> GetRecentRelevantArticlesAsync(string focusKey, int take, CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddDays(-2);
            return await _newsArticleRepository.GetAll()
                .Where(x => x.CreationTime >= cutoff && (x.IsBitcoinRelevant || x.IsUsdRelevant))
                .OrderByDescending(x => x.PublishedAt)
                .Take(Math.Max(1, take))
                .ToListAsync(cancellationToken);
        }

        public async Task<DateTime?> GetLatestRelevantPublishedAtAsync(string focusKey, CancellationToken cancellationToken)
        {
            return await _newsArticleRepository.GetAll()
                .Where(x => x.IsBitcoinRelevant || x.IsUsdRelevant)
                .OrderByDescending(x => x.PublishedAt)
                .Select(x => (DateTime?)x.PublishedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task EnsureDefaultSourcesAsync()
        {
            var existingNames = await _newsSourceRepository.GetAll().Select(x => x.Name).ToListAsync();
            foreach (var source in GetDefaultSources().Where(x => !existingNames.Contains(x.Name)))
            {
                await _newsSourceRepository.InsertAsync(source);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task<NewsRefreshRun> GetLastCompletedRunAsync(string focusKey, CancellationToken cancellationToken)
        {
            return await _newsRefreshRunRepository.GetAll()
                .Where(x => x.FocusKey == focusKey && x.Status == NewsRefreshStatus.Completed)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static bool IsRunFresh(NewsRefreshRun run, DateTime freshCutoff)
        {
            return run != null && run.CompletedAt.HasValue && run.CompletedAt.Value >= freshCutoff;
        }

        private async Task<NewsRefreshRun> CreateSkippedRunAsync(string focusKey, string trigger, string reason)
        {
            var skippedRun = new NewsRefreshRun(AbpSession.TenantId, focusKey, trigger, DateTime.UtcNow);
            skippedRun.MarkSkipped(reason, DateTime.UtcNow);
            await _newsRefreshRunRepository.InsertAsync(skippedRun);
            await CurrentUnitOfWork.SaveChangesAsync();
            return skippedRun;
        }

        private async Task<NewsRefreshRun> ExecuteRefreshAsync(string focusKey, string trigger, CancellationToken cancellationToken)
        {
            var run = new NewsRefreshRun(AbpSession.TenantId, focusKey, trigger, DateTime.UtcNow);
            await _newsRefreshRunRepository.InsertAsync(run);
            await CurrentUnitOfWork.SaveChangesAsync();

            var sources = await _newsSourceRepository.GetAll()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var fetchedCount = 0;
            var newArticleCount = 0;
            var errors = new List<string>();

            foreach (var source in sources)
            {
                try
                {
                    source.MarkRefreshStarted(DateTime.UtcNow);
                    var articles = await FetchArticlesAsync(source, cancellationToken);
                    fetchedCount += articles.Count;
                    newArticleCount += await UpsertArticlesAsync(source, articles, cancellationToken);
                    source.MarkRefreshSucceeded(DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    source.MarkRefreshFailed(ex.Message, DateTime.UtcNow);
                    errors.Add($"{source.Name}: {ex.Message}");
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }

            run.MarkCompleted(
                sources.Count,
                fetchedCount,
                newArticleCount,
                errors.Count == 0
                    ? $"Refreshed {sources.Count} sources for {focusKey}."
                    : $"Refreshed {sources.Count} sources with {errors.Count} source errors: {string.Join(" | ", errors.Take(3))}",
                DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();
            return run;
        }

        private async Task<int> UpsertArticlesAsync(NewsSource source, IEnumerable<NewsArticle> articles, CancellationToken cancellationToken)
        {
            var newArticleCount = 0;

            foreach (var article in articles)
            {
                var existing = await _newsArticleRepository.GetAll()
                    .Where(x => x.SourceId == source.Id && x.Url == article.Url)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existing != null)
                {
                    existing.RefreshSeenAt(DateTime.UtcNow);
                    continue;
                }

                await _newsArticleRepository.InsertAsync(article);
                newArticleCount++;
            }

            return newArticleCount;
        }
    }
}
