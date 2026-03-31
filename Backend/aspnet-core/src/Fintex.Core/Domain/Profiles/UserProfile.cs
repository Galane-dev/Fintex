using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Fintex.Investments.Academy;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public const int MaxAcademyLessonKeysLength = 2000;

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
            AcademyStage = AcademyStage.IntroCourse;
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

        public AcademyStage AcademyStage { get; protected set; }

        public int IntroQuizAttemptsCount { get; protected set; }

        public decimal BestIntroQuizScore { get; protected set; }

        public DateTime? IntroQuizPassedAt { get; protected set; }

        public DateTime? AcademyGraduatedAt { get; protected set; }

        public string CurrentIntroLessonKey { get; protected set; }

        public string CompletedIntroLessonKeys { get; protected set; }

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

        /// <summary>
        /// Records the outcome of the intro academy quiz and unlocks trade academy access on pass.
        /// </summary>
        public void RecordIntroQuizAttempt(decimal scorePercent, decimal requiredScorePercent, DateTime occurredAt)
        {
            IntroQuizAttemptsCount += 1;
            BestIntroQuizScore = Math.Max(BestIntroQuizScore, Clamp(scorePercent, 0m, 100m));

            if (scorePercent < requiredScorePercent)
            {
                return;
            }

            IntroQuizPassedAt ??= occurredAt;
            if (AcademyStage == AcademyStage.IntroCourse)
            {
                AcademyStage = AcademyStage.TradeAcademy;
            }
        }

        /// <summary>
        /// Synchronizes graduation based on academy paper-account growth.
        /// </summary>
        public bool SyncAcademyGraduation(bool hasMetGrowthGoal, DateTime occurredAt)
        {
            if (!IntroQuizPassedAt.HasValue)
            {
                return false;
            }

            var nextStage = hasMetGrowthGoal
                ? AcademyStage.Graduated
                : AcademyStage.TradeAcademy;

            if (AcademyStage == nextStage && (!hasMetGrowthGoal || AcademyGraduatedAt.HasValue))
            {
                return false;
            }

            AcademyStage = nextStage;
            if (hasMetGrowthGoal)
            {
                AcademyGraduatedAt ??= occurredAt;
            }

            return true;
        }

        /// <summary>
        /// Persists where the user currently is in the intro academy flow.
        /// </summary>
        public void UpdateIntroLessonProgress(string currentLessonKey, IReadOnlyCollection<string> completedLessonKeys)
        {
            CurrentIntroLessonKey = Limit(currentLessonKey, MaxAcademyLessonKeysLength);

            var normalizedKeys = (completedLessonKeys ?? Array.Empty<string>())
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => key.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            CompletedIntroLessonKeys = Limit(string.Join("|", normalizedKeys), MaxAcademyLessonKeysLength);
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
