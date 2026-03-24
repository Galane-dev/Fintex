using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Fintex.Investments.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Generates risk snapshots for live trades and enriches them with behavioral AI insights.
    /// </summary>
    public class TradeAnalysisService : ITransientDependency
    {
        private readonly IRepository<TradeAnalysisSnapshot, long> _snapshotRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IBehavioralAnalysisClient _behavioralAnalysisClient;
        private readonly IEventBus _eventBus;

        public TradeAnalysisService(
            IRepository<TradeAnalysisSnapshot, long> snapshotRepository,
            IUserProfileRepository userProfileRepository,
            ITradeRepository tradeRepository,
            IBehavioralAnalysisClient behavioralAnalysisClient,
            IEventBus eventBus)
        {
            _snapshotRepository = snapshotRepository;
            _userProfileRepository = userProfileRepository;
            _tradeRepository = tradeRepository;
            _behavioralAnalysisClient = behavioralAnalysisClient;
            _eventBus = eventBus;
        }

        /// <summary>
        /// Builds and stores a fresh analytics snapshot for a trade.
        /// </summary>
        public async Task<TradeAnalysisSnapshot> AnalyzeAndPersistAsync(Trade trade, MarketDataPoint latestPoint, CancellationToken cancellationToken)
        {
            var profile = await EnsureProfileAsync(trade);
            var recentTrades = await _tradeRepository.GetAll()
                .Where(x => x.UserId == trade.UserId)
                .OrderByDescending(x => x.CreationTime)
                .Take(20)
                .ToListAsync(cancellationToken);

            var shouldRefreshBehavioralInsight = profile.IsAiInsightsEnabled
                && (!profile.LastBehavioralAnalysisTime.HasValue
                    || profile.LastBehavioralAnalysisTime.Value <= DateTime.UtcNow.AddMinutes(-30));

            var behavioralInsight = shouldRefreshBehavioralInsight
                ? await _behavioralAnalysisClient.AnalyzeAsync(profile, recentTrades, cancellationToken)
                : new UserBehaviorInsight
                {
                    RiskScore = profile.BehavioralRiskScore,
                    Summary = profile.BehavioralSummary,
                    Provider = profile.LastAiProvider,
                    Model = profile.LastAiModel,
                    WasGenerated = false
                };

            if (behavioralInsight.WasGenerated)
            {
                profile.ApplyBehavioralInsight(
                    behavioralInsight.RiskScore,
                    behavioralInsight.Summary,
                    behavioralInsight.Provider,
                    behavioralInsight.Model,
                    DateTime.UtcNow);

                await _userProfileRepository.UpdateAsync(profile);
            }

            var riskScore = CalculateRiskScore(trade, latestPoint, profile, behavioralInsight);
            var recommendation = GetRecommendation(riskScore, trade, latestPoint);
            var narrative = BuildNarrative(trade, latestPoint, riskScore, recommendation, behavioralInsight);

            var snapshot = new TradeAnalysisSnapshot(trade.TenantId, trade.Id, trade.UserId, DateTime.UtcNow);
            snapshot.Complete(
                latestPoint.Sma ?? 0m,
                latestPoint.Ema ?? 0m,
                latestPoint.Rsi ?? 0m,
                latestPoint.StdDev ?? 0m,
                riskScore,
                recommendation,
                narrative,
                behavioralInsight.Summary,
                behavioralInsight.Provider,
                behavioralInsight.Model);

            snapshot.Id = await _snapshotRepository.InsertAndGetIdAsync(snapshot);

            trade.ApplyAnalysis(riskScore, recommendation, narrative);
            trade.RefreshMarketPrice(latestPoint.Price);
            await _tradeRepository.UpdateAsync(trade);

            await _eventBus.TriggerAsync(new TradeAnalysisCompletedEventData
            {
                TenantId = trade.TenantId,
                TradeId = trade.Id,
                UserId = trade.UserId,
                SnapshotId = snapshot.Id,
                RiskScore = riskScore,
                Recommendation = recommendation,
                Narrative = narrative,
                GeneratedAt = snapshot.GeneratedAt
            });

            return snapshot;
        }

        private async Task<UserProfile> EnsureProfileAsync(Trade trade)
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(trade.UserId);
            if (profile != null)
            {
                return profile;
            }

            profile = new UserProfile(trade.TenantId, trade.UserId, "USD");
            await _userProfileRepository.InsertAsync(profile);
            return profile;
        }

        private static decimal CalculateRiskScore(Trade trade, MarketDataPoint latestPoint, UserProfile profile, UserBehaviorInsight behavioralInsight)
        {
            decimal score = 0m;

            if (latestPoint.Rsi.HasValue)
            {
                if (latestPoint.Rsi.Value >= 70m || latestPoint.Rsi.Value <= 30m)
                {
                    score += 20m;
                }
                else
                {
                    score += Math.Abs(50m - latestPoint.Rsi.Value) / 2m;
                }
            }

            if (latestPoint.StdDev.HasValue && latestPoint.Price > 0m)
            {
                score += Math.Min(25m, (latestPoint.StdDev.Value / latestPoint.Price) * 1000m);
            }

            if (latestPoint.Ema.HasValue && latestPoint.Price > 0m)
            {
                var emaDeviation = Math.Abs(latestPoint.Price - latestPoint.Ema.Value) / latestPoint.Price * 100m;
                score += Math.Min(15m, emaDeviation * 2m);
            }

            if (latestPoint.Sma.HasValue && latestPoint.Price > 0m)
            {
                var smaDeviation = Math.Abs(latestPoint.Price - latestPoint.Sma.Value) / latestPoint.Price * 100m;
                score += Math.Min(10m, smaDeviation * 2m);
            }

            if (trade.UnrealizedProfitLoss < 0m)
            {
                score += 10m;
            }

            score += behavioralInsight == null ? profile.BehavioralRiskScore * 0.15m : behavioralInsight.RiskScore * 0.15m;
            score += (50m - profile.RiskTolerance) * 0.2m;

            if (score < 0m)
            {
                return 0m;
            }

            return score > 100m ? 100m : decimal.Round(score, 4, MidpointRounding.AwayFromZero);
        }

        private static TradeRecommendation GetRecommendation(decimal riskScore, Trade trade, MarketDataPoint latestPoint)
        {
            if (riskScore >= 80m)
            {
                return TradeRecommendation.Exit;
            }

            if (riskScore >= 60m)
            {
                return TradeRecommendation.ReduceExposure;
            }

            if (riskScore <= 30m && latestPoint.Ema.HasValue)
            {
                var trendIsSupportive = trade.Direction == TradeDirection.Buy
                    ? latestPoint.Price >= latestPoint.Ema.Value
                    : latestPoint.Price <= latestPoint.Ema.Value;

                if (trendIsSupportive)
                {
                    return TradeRecommendation.ScaleIn;
                }
            }

            return riskScore <= 45m ? TradeRecommendation.Hold : TradeRecommendation.Monitor;
        }

        private static string BuildNarrative(Trade trade, MarketDataPoint latestPoint, decimal riskScore, TradeRecommendation recommendation, UserBehaviorInsight behavioralInsight)
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                "{0} {1} {2} last={3:0.########} risk={4:0.##} recommendation={5}.",
                trade.Symbol,
                trade.AssetClass,
                trade.Direction,
                latestPoint.Price,
                riskScore,
                recommendation);

            if (latestPoint.Rsi.HasValue)
            {
                builder.AppendFormat(" RSI={0:0.##}.", latestPoint.Rsi.Value);
            }

            if (latestPoint.StdDev.HasValue)
            {
                builder.AppendFormat(" StdDev={0:0.########}.", latestPoint.StdDev.Value);
            }

            if (behavioralInsight != null && !string.IsNullOrWhiteSpace(behavioralInsight.Summary))
            {
                builder.Append(" Behavioral insight: ");
                builder.Append(behavioralInsight.Summary);
            }

            return builder.ToString();
        }
    }
}
