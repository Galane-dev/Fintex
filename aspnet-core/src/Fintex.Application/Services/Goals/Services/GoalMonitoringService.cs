using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Fintex.Investments.Brokers;
using Fintex.Investments.Notifications;
using Fintex.Investments.PaperTrading;
using Fintex.Investments.PaperTrading.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals.Services
{
    public class GoalMonitoringService : IGoalMonitoringService, ITransientDependency
    {
        private static readonly TimeSpan ExecutionCooldown = TimeSpan.FromMinutes(45);

        private readonly IGoalTargetRepository _goalTargetRepository;
        private readonly IRepository<GoalEvaluationRun, long> _goalEvaluationRepository;
        private readonly IRepository<GoalExecutionPlan, long> _goalPlanRepository;
        private readonly IRepository<GoalExecutionEvent, long> _goalEventRepository;
        private readonly IPaperTradingAppService _paperTradingAppService;
        private readonly IRecommendationSnapshotCache _recommendationSnapshotCache;
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IGoalProgressService _goalProgressService;
        private readonly IGoalPlannerService _goalPlannerService;
        private readonly IGoalExecutionService _goalExecutionService;
        private readonly IAbpSession _abpSession;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<GoalMonitoringService> _logger;

        public GoalMonitoringService(
            IGoalTargetRepository goalTargetRepository,
            IRepository<GoalEvaluationRun, long> goalEvaluationRepository,
            IRepository<GoalExecutionPlan, long> goalPlanRepository,
            IRepository<GoalExecutionEvent, long> goalEventRepository,
            IPaperTradingAppService paperTradingAppService,
            IRecommendationSnapshotCache recommendationSnapshotCache,
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository,
            ITradeRepository tradeRepository,
            IGoalProgressService goalProgressService,
            IGoalPlannerService goalPlannerService,
            IGoalExecutionService goalExecutionService,
            IAbpSession abpSession,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<GoalMonitoringService> logger)
        {
            _goalTargetRepository = goalTargetRepository;
            _goalEvaluationRepository = goalEvaluationRepository;
            _goalPlanRepository = goalPlanRepository;
            _goalEventRepository = goalEventRepository;
            _paperTradingAppService = paperTradingAppService;
            _recommendationSnapshotCache = recommendationSnapshotCache;
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
            _tradeRepository = tradeRepository;
            _goalProgressService = goalProgressService;
            _goalPlannerService = goalPlannerService;
            _goalExecutionService = goalExecutionService;
            _abpSession = abpSession;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public async Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (snapshot == null || !string.Equals(snapshot.Symbol, "BTCUSDT", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var goals = await _goalTargetRepository.GetActiveGoalsAsync("BTCUSDT");
            foreach (var goal in goals)
            {
                await EvaluateGoalAsync(goal, cancellationToken);
            }
        }

        private async Task EvaluateGoalAsync(GoalTarget goal, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            using (_abpSession.Use(goal.TenantId, goal.UserId))
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var previousEvaluationAtUtc = goal.LastEvaluatedAtUtc;
                var context = await ResolveContextAsync(goal);
                if (!context.HasAccount || !context.CurrentEquity.HasValue)
                {
                    goal.RefreshProgress(goal.CurrentEquity, goal.ProgressPercent, goal.RequiredDailyGrowthPercent, "The goal account is unavailable, so monitoring is waiting.", nowUtc);
                    await _goalTargetRepository.UpdateAsync(goal);
                    return;
                }

                var progress = _goalProgressService.Calculate(goal, context.CurrentEquity.Value, nowUtc);
                if (goal.Status == GoalStatus.Accepted)
                {
                    goal.Activate("Goal accepted and now actively monitoring BTC on a best-effort basis.");
                }

                if (progress.IsCompleted)
                {
                    goal.Complete(progress.CurrentEquity, "Best-effort target reached.");
                    await PersistEventAsync(goal, "goal-completed", "completed", "The goal target has been reached.", progress.CurrentEquity, null, nowUtc);
                    await _goalTargetRepository.UpdateAsync(goal);
                    return;
                }

                if (progress.IsExpired)
                {
                    goal.Expire(progress.CurrentEquity, "Deadline reached before the target was hit.");
                    await PersistEventAsync(goal, "goal-expired", "expired", "The goal deadline passed before the target could be reached.", progress.CurrentEquity, null, nowUtc);
                    await _goalTargetRepository.UpdateAsync(goal);
                    return;
                }

                var recommendation = await _recommendationSnapshotCache.GetOrCreateAsync(
                    goal.MarketSymbol,
                    MarketDataProvider.Binance,
                    () => _paperTradingAppService.GetRecommendationAsync(new GetPaperTradeRecommendationInput
                    {
                        Symbol = goal.MarketSymbol,
                        Provider = MarketDataProvider.Binance,
                        AssetClass = AssetClass.Crypto
                    }));
                var plan = _goalPlannerService.BuildPlan(goal, progress, recommendation, context.HasOpenExposure);

                goal.RefreshProgress(progress.CurrentEquity, progress.ProgressPercent, progress.RequiredDailyGrowthPercent, progress.Summary, nowUtc);
                goal.UpdatePlan(plan.Summary, plan.NextAction);
                await UpsertPlanAsync(goal, plan, nowUtc);

                if (!previousEvaluationAtUtc.HasValue || nowUtc - previousEvaluationAtUtc.Value >= TimeSpan.FromHours(1))
                {
                    await _goalEvaluationRepository.InsertAsync(new GoalEvaluationRun(
                        goal.TenantId,
                        goal.UserId,
                        goal.Id,
                        goal.Status,
                        progress.CurrentEquity,
                        goal.TargetPercent,
                        progress.RequiredDailyGrowthPercent,
                        plan.RiskScore ?? recommendation?.RiskScore ?? 0m,
                        plan.Summary,
                        null,
                        null,
                        nowUtc));
                }

                if (!plan.ShouldExecute || !goal.CanAttemptExecution(nowUtc, ExecutionCooldown))
                {
                    _logger.LogDebug(
                        "Goal {GoalId} skipped execution. Status={Status}, ShouldExecute={ShouldExecute}, NextAction={NextAction}, Summary={Summary}",
                        goal.Id,
                        goal.Status,
                        plan.ShouldExecute,
                        plan.NextAction,
                        plan.Summary);
                    await _goalTargetRepository.UpdateAsync(goal);
                    return;
                }

                goal.RecordExecutionAttempt(nowUtc);
                var execution = await _goalExecutionService.ExecuteAsync(goal, plan, cancellationToken);
                if (execution.WasExecuted)
                {
                    goal.RecordExecutionSuccess(execution.TradeId, execution.Summary, nowUtc);
                    await PersistEventAsync(goal, "trade-executed", "executed", execution.Summary, progress.CurrentEquity, execution.TradeId, nowUtc);
                    _logger.LogInformation(
                        "Goal {GoalId} executed a trade. TradeId={TradeId}, Symbol={Symbol}, Summary={Summary}",
                        goal.Id,
                        execution.TradeId,
                        goal.MarketSymbol,
                        execution.Summary);
                }
                else if (!string.IsNullOrWhiteSpace(execution.Error))
                {
                    goal.RecordExecutionFailure(execution.Error, nowUtc);
                    await PersistEventAsync(goal, "trade-blocked", "blocked", execution.Error, progress.CurrentEquity, execution.TradeId, nowUtc);
                    _logger.LogWarning(
                        "Goal {GoalId} blocked execution. Symbol={Symbol}, Error={Error}",
                        goal.Id,
                        goal.MarketSymbol,
                        execution.Error);
                }

                await _goalTargetRepository.UpdateAsync(goal);
            }
        }

        private async Task UpsertPlanAsync(GoalTarget goal, GoalPlanDraft plan, DateTime occurredAtUtc)
        {
            var existingPlan = await _goalPlanRepository.GetAll()
                .Where(x => x.GoalTargetId == goal.Id)
                .OrderByDescending(x => x.GeneratedAtUtc)
                .FirstOrDefaultAsync();

            if (existingPlan == null)
            {
                await _goalPlanRepository.InsertAsync(new GoalExecutionPlan(
                    goal.TenantId,
                    goal.UserId,
                    goal.Id,
                    plan.ExecutionSymbol,
                    plan.SuggestedDirection,
                    plan.SuggestedQuantity,
                    plan.SuggestedStopLoss,
                    plan.SuggestedTakeProfit,
                    plan.RiskScore,
                    plan.Summary,
                    plan.NextAction,
                    occurredAtUtc));
                return;
            }

            existingPlan.Refresh(
                plan.ExecutionSymbol,
                plan.SuggestedDirection,
                plan.SuggestedQuantity,
                plan.SuggestedStopLoss,
                plan.SuggestedTakeProfit,
                plan.RiskScore,
                plan.Summary,
                plan.NextAction,
                occurredAtUtc);
            await _goalPlanRepository.UpdateAsync(existingPlan);
        }

        private async Task PersistEventAsync(GoalTarget goal, string eventType, string status, string summary, decimal? equityAfter, long? tradeId, DateTime occurredAtUtc)
        {
            await _goalEventRepository.InsertAsync(new GoalExecutionEvent(
                goal.TenantId,
                goal.UserId,
                goal.Id,
                eventType,
                status,
                summary,
                tradeId,
                equityAfter,
                occurredAtUtc));
        }

        private async Task<(bool HasAccount, decimal? CurrentEquity, bool HasOpenExposure)> ResolveContextAsync(GoalTarget goal)
        {
            if (goal.AccountType == GoalAccountType.PaperTrading)
            {
                var snapshot = await _paperTradingAppService.GetMySnapshotAsync();
                return (
                    snapshot?.Account != null,
                    snapshot?.Account?.Equity,
                    snapshot?.Positions?.Any(x => string.Equals(x.Symbol, goal.MarketSymbol, StringComparison.OrdinalIgnoreCase)) == true);
            }

            var connection = goal.ExternalConnectionId.HasValue
                ? await _externalBrokerConnectionRepository.GetByIdForUserAsync(goal.ExternalConnectionId.Value, goal.UserId)
                : null;
            var openTrades = await _tradeRepository.GetUserOpenTradesAsync(goal.UserId);
            return (
                connection != null && connection.IsActive,
                connection?.LastKnownEquity ?? connection?.LastKnownBalance,
                openTrades.Any(x => string.Equals(x.Symbol, "BTCUSD", StringComparison.OrdinalIgnoreCase) || string.Equals(x.Symbol, goal.MarketSymbol, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
