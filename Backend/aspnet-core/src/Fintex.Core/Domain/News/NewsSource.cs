using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Configured source from which market-moving news is ingested.
    /// </summary>
    public class NewsSource : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxNameLength = 128;
        public const int MaxUrlLength = 512;
        public const int MaxCategoryLength = 64;
        public const int MaxFocusTagsLength = 256;
        public const int MaxErrorLength = 2048;

        protected NewsSource()
        {
        }

        public NewsSource(
            int? tenantId,
            string name,
            NewsSourceKind sourceKind,
            string siteUrl,
            string feedUrl,
            string category,
            string focusTags)
        {
            TenantId = tenantId;
            Name = LimitRequired(name, MaxNameLength, "News source name is required.");
            SourceKind = sourceKind;
            SiteUrl = LimitRequired(siteUrl, MaxUrlLength, "News site URL is required.");
            FeedUrl = LimitRequired(feedUrl, MaxUrlLength, "News feed URL is required.");
            Category = LimitOptional(category, MaxCategoryLength);
            FocusTags = LimitOptional(focusTags, MaxFocusTagsLength);
            IsActive = true;
        }

        public int? TenantId { get; set; }

        public string Name { get; protected set; }

        public NewsSourceKind SourceKind { get; protected set; }

        public string SiteUrl { get; protected set; }

        public string FeedUrl { get; protected set; }

        public string Category { get; protected set; }

        public string FocusTags { get; protected set; }

        public bool IsActive { get; protected set; }

        public DateTime? LastAttemptedRefreshTime { get; protected set; }

        public DateTime? LastSuccessfulRefreshTime { get; protected set; }

        public string LastError { get; protected set; }

        public void MarkRefreshStarted(DateTime timestamp)
        {
            LastAttemptedRefreshTime = timestamp;
            LastError = null;
        }

        public void MarkRefreshSucceeded(DateTime timestamp)
        {
            LastAttemptedRefreshTime = timestamp;
            LastSuccessfulRefreshTime = timestamp;
            LastError = null;
        }

        public void MarkRefreshFailed(string error, DateTime timestamp)
        {
            LastAttemptedRefreshTime = timestamp;
            LastError = LimitOptional(error, MaxErrorLength);
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
