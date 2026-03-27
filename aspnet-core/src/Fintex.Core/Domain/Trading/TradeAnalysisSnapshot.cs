using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Aggregate root that stores the analytics snapshot calculated for a trade.
    /// </summary>
    public class TradeAnalysisSnapshot : CreationAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSummaryLength = 4000;
        public const int MaxProviderLength = 64;
        public const int MaxModelLength = 128;

        protected TradeAnalysisSnapshot()
        {
        }

        public TradeAnalysisSnapshot(int? tenantId, long tradeId, long userId, DateTime generatedAt)
        {
            TenantId = tenantId;
            TradeId = tradeId;
            UserId = userId;
            GeneratedAt = generatedAt;
            Recommendation = TradeRecommendation.Monitor;
        }

        public int? TenantId { get; set; }

        public long TradeId { get; protected set; }

        public long UserId { get; protected set; }

        public DateTime GeneratedAt { get; protected set; }

        public decimal SmaValue { get; protected set; }

        public decimal EmaValue { get; protected set; }

        public decimal RsiValue { get; protected set; }

        public decimal StdDevValue { get; protected set; }

        public decimal CompositeRiskScore { get; protected set; }

        public TradeRecommendation Recommendation { get; protected set; }

        public string Narrative { get; protected set; }

        public string BehavioralSummary { get; protected set; }

        public string ExternalAiProvider { get; protected set; }

        public string ExternalAiModel { get; protected set; }

        /// <summary>
        /// Populates the snapshot with the final indicator and AI analysis values.
        /// </summary>
        public void Complete(
            decimal smaValue,
            decimal emaValue,
            decimal rsiValue,
            decimal stdDevValue,
            decimal compositeRiskScore,
            TradeRecommendation recommendation,
            string narrative,
            string behavioralSummary,
            string externalAiProvider,
            string externalAiModel)
        {
            SmaValue = smaValue;
            EmaValue = emaValue;
            RsiValue = rsiValue;
            StdDevValue = stdDevValue;
            CompositeRiskScore = compositeRiskScore;
            Recommendation = recommendation;
            Narrative = Limit(narrative, MaxSummaryLength);
            BehavioralSummary = Limit(behavioralSummary, MaxSummaryLength);
            ExternalAiProvider = Limit(externalAiProvider, MaxProviderLength);
            ExternalAiModel = Limit(externalAiModel, MaxModelLength);
        }

        private static string Limit(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return value.Length <= maxLength
                ? value.Trim()
                : value.Trim().Substring(0, maxLength);
        }
    }
}
