using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.News
{
    public class NewsRecommendationService : INewsRecommendationService, ITransientDependency
    {
        private readonly INewsIngestionService _newsIngestionService;
        private readonly INewsAnalysisClient _newsAnalysisClient;
        private readonly IRepository<NewsAnalysisSnapshot, long> _newsAnalysisSnapshotRepository;
        private readonly IAbpSession _abpSession;

        public NewsRecommendationService(
            INewsIngestionService newsIngestionService,
            INewsAnalysisClient newsAnalysisClient,
            IRepository<NewsAnalysisSnapshot, long> newsAnalysisSnapshotRepository,
            IAbpSession abpSession)
        {
            _newsIngestionService = newsIngestionService;
            _newsAnalysisClient = newsAnalysisClient;
            _newsAnalysisSnapshotRepository = newsAnalysisSnapshotRepository;
            _abpSession = abpSession;
        }

        public async Task<NewsRecommendationInsight> GetBitcoinUsdInsightAsync(
            MarketVerdictDto marketVerdict,
            CancellationToken cancellationToken)
        {
            var focusKey = NewsFocusKeys.BitcoinUsd;
            await _newsIngestionService.EnsureFreshAsync(focusKey, "recommendation_click", cancellationToken);

            var articles = await _newsIngestionService.GetRecentRelevantArticlesAsync(focusKey, 12, cancellationToken);
            if (articles == null || articles.Count == 0)
            {
                return BuildEmptyInsight(focusKey, marketVerdict);
            }

            var latestPublishedAt = await _newsIngestionService.GetLatestRelevantPublishedAtAsync(focusKey, cancellationToken);
            var freshnessCutoff = DateTime.UtcNow.AddMinutes(-30);
            var cachedSnapshot = await _newsAnalysisSnapshotRepository.GetAll()
                .Where(x => x.FocusKey == focusKey)
                .OrderByDescending(x => x.GeneratedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (cachedSnapshot != null &&
                cachedSnapshot.GeneratedAt >= freshnessCutoff &&
                cachedSnapshot.LatestArticlePublishedAt == latestPublishedAt)
            {
                return MapInsight(cachedSnapshot);
            }

            var generatedInsight = await _newsAnalysisClient.AnalyzeAsync(
                focusKey,
                articles,
                marketVerdict,
                cancellationToken);

            var snapshot = new NewsAnalysisSnapshot(
                _abpSession.TenantId,
                focusKey,
                generatedInsight.GeneratedAt == default ? DateTime.UtcNow : generatedInsight.GeneratedAt,
                articles.Count,
                latestPublishedAt,
                generatedInsight.Sentiment,
                generatedInsight.ImpactScore,
                generatedInsight.RecommendedAction,
                generatedInsight.Summary,
                JsonSerializer.Serialize(generatedInsight.KeyHeadlines ?? new List<string>()),
                generatedInsight.Provider,
                generatedInsight.Model,
                generatedInsight.RawPayloadJson);

            await _newsAnalysisSnapshotRepository.InsertAsync(snapshot);
            return generatedInsight;
        }

        private static NewsRecommendationInsight MapInsight(NewsAnalysisSnapshot snapshot)
        {
            var headlines = new List<string>();
            if (!string.IsNullOrWhiteSpace(snapshot.KeyHeadlines))
            {
                try
                {
                    headlines = JsonSerializer.Deserialize<List<string>>(snapshot.KeyHeadlines) ?? new List<string>();
                }
                catch (JsonException)
                {
                    headlines = new List<string>();
                }
            }

            return new NewsRecommendationInsight
            {
                FocusKey = snapshot.FocusKey,
                Sentiment = snapshot.Sentiment,
                ImpactScore = snapshot.ImpactScore,
                RecommendedAction = snapshot.RecommendedAction,
                Summary = snapshot.Summary,
                KeyHeadlines = headlines,
                Provider = snapshot.Provider,
                Model = snapshot.Model,
                GeneratedAt = snapshot.GeneratedAt,
                LatestArticlePublishedAt = snapshot.LatestArticlePublishedAt,
                WasGenerated = true,
                RawPayloadJson = snapshot.RawPayloadJson
            };
        }

        private static NewsRecommendationInsight BuildEmptyInsight(string focusKey, MarketVerdictDto marketVerdict)
        {
            return new NewsRecommendationInsight
            {
                FocusKey = focusKey,
                Sentiment = NewsImpactSentiment.Neutral,
                ImpactScore = 0m,
                RecommendedAction = marketVerdict?.Verdict ?? MarketVerdict.Hold,
                Summary = "No recent cached Bitcoin or US Dollar headlines are available yet, so the recommendation remains technical-only.",
                Provider = "OpenAI",
                Model = null,
                GeneratedAt = DateTime.UtcNow,
                LatestArticlePublishedAt = null,
                WasGenerated = false,
                RawPayloadJson = null
            };
        }
    }
}
