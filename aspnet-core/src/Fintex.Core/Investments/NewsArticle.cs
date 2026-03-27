using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Persisted news article used later by recommendation and AI analysis.
    /// </summary>
    public class NewsArticle : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxUrlLength = 1024;
        public const int MaxTitleLength = 512;
        public const int MaxAuthorLength = 256;
        public const int MaxCategoryLength = 128;
        public const int MaxTagsLength = 512;
        public const int MaxHashLength = 128;
        public const int MaxSummaryLength = 8000;
        public const int MaxRawPayloadLength = 16000;

        protected NewsArticle()
        {
        }

        public NewsArticle(
            int? tenantId,
            long sourceId,
            string url,
            string title,
            string summary,
            DateTime? publishedAt,
            string author,
            string category,
            string tags,
            string contentHash,
            bool isBitcoinRelevant,
            bool isUsdRelevant,
            int relevanceScore,
            string rawPayloadJson)
        {
            TenantId = tenantId;
            SourceId = sourceId;
            Url = LimitRequired(url, MaxUrlLength, "News article URL is required.");
            Title = LimitRequired(title, MaxTitleLength, "News article title is required.");
            Summary = LimitOptional(summary, MaxSummaryLength);
            PublishedAt = publishedAt;
            Author = LimitOptional(author, MaxAuthorLength);
            Category = LimitOptional(category, MaxCategoryLength);
            Tags = LimitOptional(tags, MaxTagsLength);
            ContentHash = LimitRequired(contentHash, MaxHashLength, "News content hash is required.");
            IsBitcoinRelevant = isBitcoinRelevant;
            IsUsdRelevant = isUsdRelevant;
            RelevanceScore = relevanceScore;
            RawPayloadJson = LimitOptional(rawPayloadJson, MaxRawPayloadLength);
            LastSeenAt = DateTime.UtcNow;
        }

        public int? TenantId { get; set; }

        public long SourceId { get; protected set; }

        public string Url { get; protected set; }

        public string Title { get; protected set; }

        public string Summary { get; protected set; }

        public DateTime? PublishedAt { get; protected set; }

        public string Author { get; protected set; }

        public string Category { get; protected set; }

        public string Tags { get; protected set; }

        public string ContentHash { get; protected set; }

        public bool IsBitcoinRelevant { get; protected set; }

        public bool IsUsdRelevant { get; protected set; }

        public int RelevanceScore { get; protected set; }

        public DateTime LastSeenAt { get; protected set; }

        public string RawPayloadJson { get; protected set; }

        public void RefreshSeenAt(DateTime timestamp)
        {
            LastSeenAt = timestamp;
        }

        private static string LimitRequired(string value, int maxLength, string error)
        {
            var limited = LimitOptional(value, maxLength);
            if (string.IsNullOrWhiteSpace(limited))
            {
                throw new ArgumentException(error);
            }

            return limited;
        }

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
