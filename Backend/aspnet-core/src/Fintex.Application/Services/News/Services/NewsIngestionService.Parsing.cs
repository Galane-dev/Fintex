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
    public partial class NewsIngestionService
    {
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
                return root.Descendants()
                    .Where(x => x.Name.LocalName == "item")
                    .Select(x => BuildArticleFromRss(source, x, payload))
                    .Where(x => x != null)
                    .ToList();
            }

            if (string.Equals(root.Name.LocalName, "feed", StringComparison.OrdinalIgnoreCase))
            {
                return root.Elements()
                    .Where(x => x.Name.LocalName == "entry")
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
            if (isBitcoinRelevant) tags.Add("BTC");
            if (isUsdRelevant) tags.Add("USD");
            return string.Join(",", tags);
        }

        private static DateTime ParseDate(string input)
        {
            return DateTime.TryParse(input, out var parsed) ? parsed.ToUniversalTime() : DateTime.UtcNow;
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
