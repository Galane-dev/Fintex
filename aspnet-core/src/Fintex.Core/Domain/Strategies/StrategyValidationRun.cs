using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// Stores one user-submitted strategy validation along with the market context used.
    /// </summary>
    public class StrategyValidationRun : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxNameLength = 128;
        public const int MaxSymbolLength = 32;
        public const int MaxTimeframeLength = 16;
        public const int MaxDirectionLength = 16;
        public const int MaxStrategyLength = 8000;
        public const int MaxSummaryLength = 4000;
        public const int MaxListJsonLength = 8000;
        public const int MaxProviderLength = 64;
        public const int MaxModelLength = 128;

        protected StrategyValidationRun()
        {
        }

        public StrategyValidationRun(
            int? tenantId,
            long userId,
            string strategyName,
            string symbol,
            MarketDataProvider provider,
            string timeframe,
            string directionPreference,
            string strategyText)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User id must be greater than zero.");
            }

            TenantId = tenantId;
            UserId = userId;
            StrategyName = Limit(strategyName, MaxNameLength);
            Symbol = LimitRequired(symbol, MaxSymbolLength, "Strategy symbol is required.").ToUpperInvariant();
            Provider = provider;
            Timeframe = Limit(timeframe, MaxTimeframeLength);
            DirectionPreference = Limit(directionPreference, MaxDirectionLength);
            StrategyText = LimitRequired(strategyText, MaxStrategyLength, "Strategy details are required.");
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string StrategyName { get; protected set; }

        public string Symbol { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public string Timeframe { get; protected set; }

        public string DirectionPreference { get; protected set; }

        public string StrategyText { get; protected set; }

        public decimal? MarketPrice { get; protected set; }

        public decimal? MarketTrendScore { get; protected set; }

        public decimal? MarketConfidenceScore { get; protected set; }

        public string MarketVerdict { get; protected set; }

        public string NewsSummary { get; protected set; }

        public decimal? ValidationScore { get; protected set; }

        public StrategyValidationOutcome Outcome { get; protected set; }

        public string Summary { get; protected set; }

        public string StrengthsJson { get; protected set; }

        public string RisksJson { get; protected set; }

        public string ImprovementsJson { get; protected set; }

        public string SuggestedAction { get; protected set; }

        public decimal? SuggestedEntryPrice { get; protected set; }

        public decimal? SuggestedStopLoss { get; protected set; }

        public decimal? SuggestedTakeProfit { get; protected set; }

        public string AiProvider { get; protected set; }

        public string AiModel { get; protected set; }

        public void ApplyResult(
            decimal? marketPrice,
            decimal? marketTrendScore,
            decimal? marketConfidenceScore,
            string marketVerdict,
            string newsSummary,
            decimal validationScore,
            StrategyValidationOutcome outcome,
            string summary,
            string strengthsJson,
            string risksJson,
            string improvementsJson,
            string suggestedAction,
            decimal? suggestedEntryPrice,
            decimal? suggestedStopLoss,
            decimal? suggestedTakeProfit,
            string aiProvider,
            string aiModel)
        {
            MarketPrice = marketPrice;
            MarketTrendScore = marketTrendScore;
            MarketConfidenceScore = marketConfidenceScore;
            MarketVerdict = Limit(marketVerdict, MaxDirectionLength);
            NewsSummary = Limit(newsSummary, MaxSummaryLength);
            ValidationScore = Clamp(validationScore, 0m, 100m);
            Outcome = outcome;
            Summary = Limit(summary, MaxSummaryLength);
            StrengthsJson = Limit(strengthsJson, MaxListJsonLength);
            RisksJson = Limit(risksJson, MaxListJsonLength);
            ImprovementsJson = Limit(improvementsJson, MaxListJsonLength);
            SuggestedAction = Limit(suggestedAction, MaxDirectionLength);
            SuggestedEntryPrice = suggestedEntryPrice;
            SuggestedStopLoss = suggestedStopLoss;
            SuggestedTakeProfit = suggestedTakeProfit;
            AiProvider = Limit(aiProvider, MaxProviderLength);
            AiModel = Limit(aiModel, MaxModelLength);
        }

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
            var limited = Limit(value, maxLength);
            if (string.IsNullOrWhiteSpace(limited))
            {
                throw new ArgumentException(error);
            }

            return limited;
        }

        private static string Limit(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
        }
    }
}
