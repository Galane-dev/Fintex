using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fintex.Investments.News
{
    public class NewsIngestionService : FintexAppServiceBase, INewsIngestionService, ITransientDependency
    {
        private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);

        private static readonly string[] BitcoinKeywords =
        {
            "bitcoin", "btc", "crypto", "cryptocurrency", "spot etf", "etf", "mining", "stablecoin"
        };

        private static readonly string[] UsdKeywords =
        {
            "federal reserve", "fed", "dollar", "usd", "inflation", "cpi", "ppi", "payroll", "nfp", "rate", "rates", "yield", "treasury"
        };

        private readonly IRepository<NewsSource, long> _newsSourceRepository;
        private readonly IRepository<NewsArticle, long> _newsArticleRepository;
        private readonly IRepository<NewsRefreshRun, long> _newsRefreshRunRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NewsIngestionService(
            IRepository<NewsSource, long> newsSourceRepository,
            IRepository<NewsArticle, long> newsArticleRepository,
            IRepository<NewsRefreshRun, long> newsRefreshRunRepository,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _newsSourceRepository = newsSourceRepository;
            _newsArticleRepository = newsArticleRepository;
            _newsRefreshRunRepository = newsRefreshRunRepository;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<NewsRefreshRun> EnsureFreshAsync(string focusKey, string trigger, CancellationToken cancellationToken)
        {
            await EnsureDefaultSourcesAsync();

            var refreshWindowMinutes = Math.Max(5, _configuration.GetValue<int?>("News:RefreshWindowMinutes") ?? 30);
            var freshCutoff = DateTime.UtcNow.AddMinutes(-refreshWindowMinutes);
            var lastCompletedRun = await _newsRefreshRunRepository.GetAll()
                .Where(x => x.FocusKey == focusKey && x.Status == NewsRefreshStatus.Completed)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastCompletedRun != null &&
                lastCompletedRun.CompletedAt.HasValue &&
                lastCompletedRun.CompletedAt.Value >= freshCutoff)
            {
                var skippedRun = new NewsRefreshRun(AbpSession.TenantId, focusKey, trigger, DateTime.UtcNow);
                skippedRun.MarkSkipped("Skipped refresh because the news cache is still fresh.", DateTime.UtcNow);
                await _newsRefreshRunRepository.InsertAsync(skippedRun);
                await CurrentUnitOfWork.SaveChangesAsync();
                return skippedRun;
            }

            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                lastCompletedRun = await _newsRefreshRunRepository.GetAll()
                    .Where(x => x.FocusKey == focusKey && x.Status == NewsRefreshStatus.Completed)
                    .OrderByDescending(x => x.StartedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lastCompletedRun != null &&
                    lastCompletedRun.CompletedAt.HasValue &&
                    lastCompletedRun.CompletedAt.Value >= freshCutoff)
                {
                    var skippedRun = new NewsRefreshRun(AbpSession.TenantId, focusKey, trigger, DateTime.UtcNow);
                    skippedRun.MarkSkipped("Skipped refresh because another request refreshed the cache.", DateTime.UtcNow);
                    await _newsRefreshRunRepository.InsertAsync(skippedRun);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    return skippedRun;
                }

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

                        source.MarkRefreshSucceeded(DateTime.UtcNow);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        source.MarkRefreshFailed(ex.Message, DateTime.UtcNow);
                        errors.Add($"{source.Name}: {ex.Message}");
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
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
            var results = await _newsArticleRepository.GetAll()
                .Where(x => x.CreationTime >= cutoff && (x.IsBitcoinRelevant || x.IsUsdRelevant))
                .OrderByDescending(x => x.PublishedAt)
                .Take(Math.Max(1, take))
                .ToListAsync(cancellationToken);

            return results;
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
            var existingNames = await _newsSourceRepository.GetAll()
                .Select(x => x.Name)
                .ToListAsync();

            foreach (var source in GetDefaultSources().Where(x => !existingNames.Contains(x.Name)))
            {
                await _newsSourceRepository.InsertAsync(source);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task<List<NewsArticle>> FetchArticlesAsync(NewsSource source, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, source.FeedUrl))
            {
                request.Headers.UserAgent.ParseAdd("FintexNewsBot/1.0");
                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                var payload = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseFeed(source, payload);
            }
        }

        private List<NewsArticle> ParseFeed(NewsSource source, string payload)
        {
            var document = XDocument.Parse(payload, LoadOptions.PreserveWhitespace);
            var root = document.Root;
            if (root == null)
            {
                return new List<NewsArticle>();
            }

            if (string.Equals(root.Name.LocalName, "rss", StringComparison.OrdinalIgnoreCase))
            {
                return root.Descendants().Where(x => x.Name.LocalName == "item")
                    .Select(x => BuildArticleFromRss(source, x, payload))
                    .Where(x => x != null)
                    .ToList();
            }

            if (string.Equals(root.Name.LocalName, "feed", StringComparison.OrdinalIgnoreCase))
            {
                return root.Elements().Where(x => x.Name.LocalName == "entry")
                    .Select(x => BuildArticleFromAtom(source, x, payload))
                    .Where(x => x != null)
                    .ToList();
            }

            return new List<NewsArticle>();
        }

        private NewsArticle BuildArticleFromRss(NewsSource source, XElement item, string rawPayload)
        {
            var title = item.Elements().FirstOrDefault(x => x.Name.LocalName == "title")?.Value?.Trim();
            var link = item.Elements().FirstOrDefault(x => x.Name.LocalName == "link")?.Value?.Trim();
            var summary = item.Elements().FirstOrDefault(x => x.Name.LocalName == "description")?.Value?.Trim();
            var author = item.Elements().FirstOrDefault(x => x.Name.LocalName == "creator" || x.Name.LocalName == "author")?.Value?.Trim();
            var category = item.Elements().FirstOrDefault(x => x.Name.LocalName == "category")?.Value?.Trim() ?? source.Category;
            var publishedAt = ParseDate(
                item.Elements().FirstOrDefault(x => x.Name.LocalName == "pubDate")?.Value ??
                item.Elements().FirstOrDefault(x => x.Name.LocalName == "published")?.Value);

            return BuildRelevantArticle(source, title, link, summary, author, category, publishedAt, rawPayload);
        }

        private NewsArticle BuildArticleFromAtom(NewsSource source, XElement entry, string rawPayload)
        {
            var title = entry.Elements().FirstOrDefault(x => x.Name.LocalName == "title")?.Value?.Trim();
            var link = entry.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "link" && x.Attribute("href") != null)
                ?.Attribute("href")
                ?.Value
                ?.Trim();
            var summary = entry.Elements().FirstOrDefault(x => x.Name.LocalName == "summary" || x.Name.LocalName == "content")?.Value?.Trim();
            var author = entry.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "author")
                ?.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "name")
                ?.Value
                ?.Trim();
            var category = entry.Elements().FirstOrDefault(x => x.Name.LocalName == "category")?.Attribute("term")?.Value?.Trim() ?? source.Category;
            var publishedAt = ParseDate(
                entry.Elements().FirstOrDefault(x => x.Name.LocalName == "updated")?.Value ??
                entry.Elements().FirstOrDefault(x => x.Name.LocalName == "published")?.Value);

            return BuildRelevantArticle(source, title, link, summary, author, category, publishedAt, rawPayload);
        }

        private NewsArticle BuildRelevantArticle(
            NewsSource source,
            string title,
            string link,
            string summary,
            string author,
            string category,
            DateTime publishedAt,
            string rawPayload)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            {
                return null;
            }

            var combinedText = $"{title} {summary}".ToLowerInvariant();
            var bitcoinScore = ScoreKeywords(combinedText, BitcoinKeywords);
            var usdScore = ScoreKeywords(combinedText, UsdKeywords);
            var relevanceScore = bitcoinScore + usdScore;

            if (relevanceScore <= 0)
            {
                return null;
            }

            return new NewsArticle(
                AbpSession.TenantId,
                source.Id,
                link,
                title,
                summary,
                publishedAt == default ? DateTime.UtcNow : publishedAt,
                author,
                category,
                BuildTags(bitcoinScore > 0, usdScore > 0),
                ComputeHash($"{title}|{summary}|{link}"),
                bitcoinScore > 0,
                usdScore > 0,
                relevanceScore,
                rawPayload.Length > NewsArticle.MaxRawPayloadLength
                    ? rawPayload.Substring(0, NewsArticle.MaxRawPayloadLength)
                    : rawPayload);
        }

        private static int ScoreKeywords(string content, IEnumerable<string> keywords)
        {
            return keywords.Count(keyword => content.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildTags(bool isBitcoinRelevant, bool isUsdRelevant)
        {
            var tags = new List<string>();
            if (isBitcoinRelevant)
            {
                tags.Add("BTC");
            }

            if (isUsdRelevant)
            {
                tags.Add("USD");
            }

            return string.Join(",", tags);
        }

        private static DateTime ParseDate(string input)
        {
            if (DateTime.TryParse(input, out var parsed))
            {
                return parsed.ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        private static string ComputeHash(string value)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                return Convert.ToBase64String(bytes);
            }
        }

        private IEnumerable<NewsSource> GetDefaultSources()
        {
            return new[]
            {
                new NewsSource(
                    AbpSession.TenantId,
                    "CoinDesk",
                    NewsSourceKind.Rss,
                    "https://www.coindesk.com",
                    _configuration["News:Sources:CoinDesk:FeedUrl"] ?? "https://www.coindesk.com/arc/outboundfeeds/rss/",
                    "Bitcoin",
                    "BTC,USD"),
                new NewsSource(
                    AbpSession.TenantId,
                    "Federal Reserve Monetary Policy",
                    NewsSourceKind.Rss,
                    "https://www.federalreserve.gov",
                    _configuration["News:Sources:FederalReserve:FeedUrl"] ?? "https://www.federalreserve.gov/feeds/press_monetary.xml",
                    "Macro",
                    "USD,FED"),
                new NewsSource(
                    AbpSession.TenantId,
                    "BLS Latest Releases",
                    NewsSourceKind.Rss,
                    "https://www.bls.gov",
                    _configuration["News:Sources:Bls:FeedUrl"] ?? "https://www.bls.gov/feed/bls_latest.rss",
                    "Macro",
                    "USD,LABOR,INFLATION"),
                new NewsSource(
                    AbpSession.TenantId,
                    "CFTC Press Releases",
                    NewsSourceKind.Rss,
                    "https://www.cftc.gov",
                    _configuration["News:Sources:Cftc:FeedUrl"] ?? "https://www.cftc.gov/RSS/PressReleases.xml",
                    "Regulation",
                    "BTC,USD,REGULATION")
            };
        }
    }
}
