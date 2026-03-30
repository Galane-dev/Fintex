using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Captures one refresh cycle across the configured news sources.
    /// </summary>
    public class NewsRefreshRun : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxFocusKeyLength = 64;
        public const int MaxTriggerLength = 64;
        public const int MaxSummaryLength = 4000;

        protected NewsRefreshRun()
        {
        }

        public NewsRefreshRun(int? tenantId, string focusKey, string trigger, DateTime startedAt)
        {
            TenantId = tenantId;
            FocusKey = LimitRequired(focusKey, MaxFocusKeyLength, "News focus is required.");
            Trigger = LimitRequired(trigger, MaxTriggerLength, "News trigger is required.");
            StartedAt = startedAt;
            Status = NewsRefreshStatus.Started;
        }

        public int? TenantId { get; set; }

        public string FocusKey { get; protected set; }

        public string Trigger { get; protected set; }

        public DateTime StartedAt { get; protected set; }

        public DateTime? CompletedAt { get; protected set; }

        public NewsRefreshStatus Status { get; protected set; }

        public int SourceCount { get; protected set; }

        public int ArticlesFetched { get; protected set; }

        public int NewArticles { get; protected set; }

        public string Summary { get; protected set; }

        public void MarkCompleted(int sourceCount, int fetched, int inserted, string summary, DateTime completedAt)
        {
            SourceCount = sourceCount;
            ArticlesFetched = fetched;
            NewArticles = inserted;
            Summary = LimitOptional(summary, MaxSummaryLength);
            CompletedAt = completedAt;
            Status = NewsRefreshStatus.Completed;
        }

        public void MarkSkipped(string summary, DateTime completedAt)
        {
            Summary = LimitOptional(summary, MaxSummaryLength);
            CompletedAt = completedAt;
            Status = NewsRefreshStatus.Skipped;
        }

        public void MarkFailed(int sourceCount, int fetched, int inserted, string summary, DateTime completedAt)
        {
            SourceCount = sourceCount;
            ArticlesFetched = fetched;
            NewArticles = inserted;
            Summary = LimitOptional(summary, MaxSummaryLength);
            CompletedAt = completedAt;
            Status = NewsRefreshStatus.Failed;
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
