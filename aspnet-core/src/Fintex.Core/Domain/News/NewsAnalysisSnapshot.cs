using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Cached AI interpretation of recent market-relevant news.
    /// </summary>
    public class NewsAnalysisSnapshot : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxFocusKeyLength = 64;
        public const int MaxSummaryLength = 4000;
        public const int MaxProviderLength = 64;
        public const int MaxModelLength = 128;
        public const int MaxHeadlinesLength = 4000;
        public const int MaxRawPayloadLength = 16000;

        protected NewsAnalysisSnapshot()
        {
        }

        public NewsAnalysisSnapshot(
            int? tenantId,
            string focusKey,
            DateTime generatedAt,
            int articleCount,
            DateTime? latestArticlePublishedAt,
            NewsImpactSentiment sentiment,
            decimal impactScore,
            MarketVerdict recommendedAction,
            string summary,
            string keyHeadlines,
            string provider,
            string model,
            string rawPayloadJson)
        {
            TenantId = tenantId;
            FocusKey = LimitRequired(focusKey, MaxFocusKeyLength, "News analysis focus is required.");
            GeneratedAt = generatedAt;
            ArticleCount = articleCount;
            LatestArticlePublishedAt = latestArticlePublishedAt;
            Sentiment = sentiment;
            ImpactScore = Clamp(impactScore, 0m, 100m);
            RecommendedAction = recommendedAction;
            Summary = LimitOptional(summary, MaxSummaryLength);
            KeyHeadlines = LimitOptional(keyHeadlines, MaxHeadlinesLength);
            Provider = LimitOptional(provider, MaxProviderLength);
            Model = LimitOptional(model, MaxModelLength);
            RawPayloadJson = LimitOptional(rawPayloadJson, MaxRawPayloadLength);
        }

        public int? TenantId { get; set; }

        public string FocusKey { get; protected set; }

        public DateTime GeneratedAt { get; protected set; }

        public int ArticleCount { get; protected set; }

        public DateTime? LatestArticlePublishedAt { get; protected set; }

        public NewsImpactSentiment Sentiment { get; protected set; }

        public decimal ImpactScore { get; protected set; }

        public MarketVerdict RecommendedAction { get; protected set; }

        public string Summary { get; protected set; }

        public string KeyHeadlines { get; protected set; }

        public string Provider { get; protected set; }

        public string Model { get; protected set; }

        public string RawPayloadJson { get; protected set; }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
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
