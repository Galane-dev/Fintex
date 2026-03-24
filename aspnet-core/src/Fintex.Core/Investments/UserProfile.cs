using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Aggregate root containing user trading preferences and behavioral analytics.
    /// </summary>
    public class UserProfile : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxCurrencyLength = 8;
        public const int MaxSymbolsLength = 512;
        public const int MaxSummaryLength = 4000;
        public const int MaxStrategyLength = 4000;
        public const int MaxProviderLength = 64;
        public const int MaxModelLength = 128;

        protected UserProfile()
        {
        }

        public UserProfile(int? tenantId, long userId, string preferredBaseCurrency)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            TenantId = tenantId;
            UserId = userId;
            PreferredBaseCurrency = Limit(preferredBaseCurrency, MaxCurrencyLength) ?? "USD";
            FavoriteSymbols = string.Empty;
            IsAiInsightsEnabled = true;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string PreferredBaseCurrency { get; protected set; }

        public string FavoriteSymbols { get; protected set; }

        public decimal RiskTolerance { get; protected set; }

        public bool IsAiInsightsEnabled { get; protected set; }

        public decimal BehavioralRiskScore { get; protected set; }

        public string BehavioralSummary { get; protected set; }

        public string StrategyNotes { get; protected set; }

        public string LastAiProvider { get; protected set; }

        public string LastAiModel { get; protected set; }

        public DateTime? LastBehavioralAnalysisTime { get; protected set; }

        /// <summary>
        /// Updates user trading preferences that drive analytics and recommendations.
        /// </summary>
        public void UpdatePreferences(string preferredBaseCurrency, string favoriteSymbols, decimal riskTolerance, bool isAiInsightsEnabled, string strategyNotes)
        {
            PreferredBaseCurrency = Limit(preferredBaseCurrency, MaxCurrencyLength) ?? PreferredBaseCurrency;
            FavoriteSymbols = Limit(favoriteSymbols, MaxSymbolsLength);
            RiskTolerance = Clamp(riskTolerance, 0m, 100m);
            IsAiInsightsEnabled = isAiInsightsEnabled;
            StrategyNotes = Limit(strategyNotes, MaxStrategyLength);
        }

        /// <summary>
        /// Persists the most recent behavioral AI insight for the user.
        /// </summary>
        public void ApplyBehavioralInsight(decimal behavioralRiskScore, string summary, string provider, string model, DateTime timestamp)
        {
            BehavioralRiskScore = Clamp(behavioralRiskScore, 0m, 100m);
            BehavioralSummary = Limit(summary, MaxSummaryLength);
            LastAiProvider = Limit(provider, MaxProviderLength);
            LastAiModel = Limit(model, MaxModelLength);
            LastBehavioralAnalysisTime = timestamp;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
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
