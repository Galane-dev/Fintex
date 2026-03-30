using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Brokers;
using Fintex.Investments.Goals.Dto;
using Fintex.Investments.PaperTrading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals.Services
{
    [AbpAuthorize]
    public class GoalAutomationAppService : FintexAppServiceBase, IGoalAutomationAppService
    {
        private readonly IGoalTargetRepository _goalTargetRepository;
        private readonly IRepository<GoalEvaluationRun, long> _goalEvaluationRepository;
        private readonly IRepository<GoalExecutionPlan, long> _goalPlanRepository;
        private readonly IRepository<GoalExecutionEvent, long> _goalEventRepository;
        private readonly IGoalFeasibilityService _goalFeasibilityService;
        private readonly IGoalProgressService _goalProgressService;
        private readonly IPaperTradingAppService _paperTradingAppService;
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;

        public GoalAutomationAppService(
            IGoalTargetRepository goalTargetRepository,
            IRepository<GoalEvaluationRun, long> goalEvaluationRepository,
            IRepository<GoalExecutionPlan, long> goalPlanRepository,
            IRepository<GoalExecutionEvent, long> goalEventRepository,
            IGoalFeasibilityService goalFeasibilityService,
            IGoalProgressService goalProgressService,
            IPaperTradingAppService paperTradingAppService,
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository)
        {
            _goalTargetRepository = goalTargetRepository;
            _goalEvaluationRepository = goalEvaluationRepository;
            _goalPlanRepository = goalPlanRepository;
            _goalEventRepository = goalEventRepository;
            _goalFeasibilityService = goalFeasibilityService;
            _goalProgressService = goalProgressService;
            _paperTradingAppService = paperTradingAppService;
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
        }

        public async Task<ListResultDto<GoalTargetDto>> GetMyGoalsAsync()
        {
            var userId = AbpSession.GetUserId();
            var goals = await _goalTargetRepository.GetUserGoalsAsync(userId);
            var connectionMap = await BuildConnectionMapAsync(userId);
            var goalIds = goals.Select(x => x.Id).ToList();
            var evaluations = await _goalEvaluationRepository.GetAllListAsync(x => goalIds.Contains(x.GoalTargetId));
            var plans = await _goalPlanRepository.GetAllListAsync(x => goalIds.Contains(x.GoalTargetId));
            var events = await _goalEventRepository.GetAllListAsync(x => goalIds.Contains(x.GoalTargetId));

            return new ListResultDto<GoalTargetDto>(goals.Select(goal =>
            {
                var latestEvaluation = evaluations.Where(x => x.GoalTargetId == goal.Id).OrderByDescending(x => x.OccurredAtUtc).FirstOrDefault();
                var latestPlan = plans.Where(x => x.GoalTargetId == goal.Id).OrderByDescending(x => x.GeneratedAtUtc).FirstOrDefault();
                var recentEvents = events.Where(x => x.GoalTargetId == goal.Id).OrderByDescending(x => x.OccurredAtUtc).Take(8).ToList();
                return MapGoal(goal, connectionMap, latestEvaluation, latestPlan, recentEvents);
            }).ToList());
        }

        public async Task<GoalTargetDto> CreateGoalAsync(CreateGoalTargetInput input)
        {
            var accountContext = await ResolveAccountContextAsync(input);
            var name = string.IsNullOrWhiteSpace(input.Name)
                ? BuildDefaultName(input)
                : input.Name.Trim();
            var result = _goalFeasibilityService.Evaluate(new GoalFeasibilityRequest
            {
                TargetType = input.TargetType,
                CurrentEquity = accountContext.CurrentEquity,
                TargetAmount = input.TargetAmount,
                TargetPercent = input.TargetPercent,
                DeadlineUtc = input.DeadlineUtc,
                MaxAcceptableRisk = input.MaxAcceptableRisk
            });

            var goal = new GoalTarget(
                AbpSession.TenantId,
                AbpSession.GetUserId(),
                name,
                input.AccountType,
                input.ExternalConnectionId,
                "BTCUSDT",
                "BTCUSDT,BTCUSD",
                input.TargetType,
                accountContext.CurrentEquity,
                result.TargetEquity,
                result.TargetPercent,
                input.DeadlineUtc,
                input.MaxAcceptableRisk,
                input.MaxDrawdownPercent,
                input.MaxPositionSizePercent,
                input.TradingSession,
                input.AllowOvernightPositions);

            var progress = _goalProgressService.Calculate(goal, accountContext.CurrentEquity, DateTime.UtcNow);
            goal.ApplyInitialDecision(result.IsAccepted, accountContext.CurrentEquity, progress.ProgressPercent, result.RequiredDailyGrowthPercent, result.Summary);

            await _goalTargetRepository.InsertAsync(goal);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _goalEvaluationRepository.InsertAsync(new GoalEvaluationRun(
                goal.TenantId,
                goal.UserId,
                goal.Id,
                goal.Status,
                accountContext.CurrentEquity,
                result.RequiredGrowthPercent,
                result.RequiredDailyGrowthPercent,
                result.FeasibilityScore,
                result.Summary,
                result.CounterProposalTargetEquity,
                result.CounterProposalTargetPercent,
                DateTime.UtcNow));

            await _goalEventRepository.InsertAsync(new GoalExecutionEvent(
                goal.TenantId,
                goal.UserId,
                goal.Id,
                result.IsAccepted ? "goal-accepted" : "goal-rejected",
                result.IsAccepted ? "accepted" : "rejected",
                result.Summary,
                null,
                accountContext.CurrentEquity,
                DateTime.UtcNow));

            if (result.IsAccepted)
            {
                await _goalPlanRepository.InsertAsync(new GoalExecutionPlan(
                    goal.TenantId,
                    goal.UserId,
                    goal.Id,
                    input.AccountType == GoalAccountType.ExternalBroker ? "BTCUSD" : "BTCUSDT",
                    null,
                    null,
                    null,
                    null,
                    null,
                    "Goal accepted on a best-effort basis. Fintex will watch BTC and only act when recommendation, session, and risk filters align.",
                    "Wait for a disciplined BTC setup before attempting the first execution.",
                    DateTime.UtcNow));
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return (await GetMyGoalsAsync()).Items.First(x => x.Id == goal.Id);
        }

        public Task<GoalTargetDto> PauseGoalAsync(EntityDto<long> input)
        {
            return UpdateStatusAsync(input.Id, goal =>
            {
                goal.Pause("Goal paused by the user.");
                return ("goal-paused", "paused", "Goal paused by the user.");
            });
        }

        public Task<GoalTargetDto> ResumeGoalAsync(EntityDto<long> input)
        {
            return UpdateStatusAsync(input.Id, goal =>
            {
                goal.Resume("Goal resumed and back on best-effort monitoring.");
                return ("goal-resumed", "resumed", "Goal resumed and monitoring restarted.");
            });
        }

        public Task<GoalTargetDto> CancelGoalAsync(EntityDto<long> input)
        {
            return UpdateStatusAsync(input.Id, goal =>
            {
                goal.Cancel("Goal canceled by the user.");
                return ("goal-canceled", "canceled", "Goal canceled by the user.");
            });
        }

        private async Task<GoalTargetDto> UpdateStatusAsync(long goalId, Func<GoalTarget, (string EventType, string Status, string Summary)> apply)
        {
            var userId = AbpSession.GetUserId();
            var goal = await _goalTargetRepository.GetUserGoalAsync(userId, goalId);
            if (goal == null)
            {
                throw new UserFriendlyException("We could not find that goal target.");
            }

            var update = apply(goal);
            await _goalTargetRepository.UpdateAsync(goal);
            await _goalEventRepository.InsertAsync(new GoalExecutionEvent(
                goal.TenantId,
                goal.UserId,
                goal.Id,
                update.EventType,
                update.Status,
                update.Summary,
                null,
                goal.CurrentEquity,
                DateTime.UtcNow));
            await CurrentUnitOfWork.SaveChangesAsync();

            return (await GetMyGoalsAsync()).Items.First(x => x.Id == goal.Id);
        }

        private async Task<(decimal CurrentEquity, string ExternalConnectionName)> ResolveAccountContextAsync(CreateGoalTargetInput input)
        {
            if (input.AccountType == GoalAccountType.PaperTrading)
            {
                var snapshot = await _paperTradingAppService.GetMySnapshotAsync();
                if (snapshot?.Account?.Equity == null || snapshot.Account.Equity <= 0m)
                {
                    throw new UserFriendlyException("Create a paper account first so Fintex has equity to target.");
                }

                return (snapshot.Account.Equity, "Paper academy");
            }

            if (!input.ExternalConnectionId.HasValue)
            {
                throw new UserFriendlyException("Choose the connected external broker account for this goal.");
            }

            var connection = await _externalBrokerConnectionRepository.GetByIdForUserAsync(input.ExternalConnectionId.Value, AbpSession.GetUserId());
            if (connection == null || !connection.IsActive)
            {
                throw new UserFriendlyException("The selected external broker account is not available.");
            }

            var equity = connection.LastKnownEquity ?? connection.LastKnownBalance;
            if (!equity.HasValue || equity.Value <= 0m)
            {
                throw new UserFriendlyException("Fintex could not read a usable balance from that broker connection yet.");
            }

            return (equity.Value, connection.DisplayName);
        }

        private async Task<Dictionary<long, string>> BuildConnectionMapAsync(long userId)
        {
            var connections = await _externalBrokerConnectionRepository.GetForUserAsync(userId);
            return connections.ToDictionary(x => x.Id, x => x.DisplayName);
        }

        private static string BuildDefaultName(CreateGoalTargetInput input)
        {
            return input.TargetType == GoalTargetType.TargetAmount
                ? $"BTC target to {input.TargetAmount?.ToString("0.##") ?? "-"} by {input.DeadlineUtc:yyyy-MM-dd HH:mm}"
                : $"BTC growth target {input.TargetPercent?.ToString("0.##") ?? "-"}% by {input.DeadlineUtc:yyyy-MM-dd HH:mm}";
        }

        private static GoalTargetDto MapGoal(
            GoalTarget goal,
            IReadOnlyDictionary<long, string> connectionMap,
            GoalEvaluationRun latestEvaluation,
            GoalExecutionPlan latestPlan,
            List<GoalExecutionEvent> events)
        {
            return new GoalTargetDto
            {
                Id = goal.Id,
                Name = goal.Name,
                AccountType = goal.AccountType.ToString(),
                ExternalConnectionId = goal.ExternalConnectionId,
                ExternalConnectionName = goal.ExternalConnectionId.HasValue && connectionMap.TryGetValue(goal.ExternalConnectionId.Value, out var connectionName) ? connectionName : null,
                MarketSymbol = goal.MarketSymbol,
                AllowedSymbols = goal.AllowedSymbols,
                TargetType = goal.TargetType.ToString(),
                StartEquity = goal.StartEquity,
                CurrentEquity = goal.CurrentEquity,
                TargetEquity = goal.TargetEquity,
                TargetPercent = goal.TargetPercent,
                DeadlineUtc = goal.DeadlineUtc.ToString("O"),
                MaxAcceptableRisk = goal.MaxAcceptableRisk,
                MaxDrawdownPercent = goal.MaxDrawdownPercent,
                MaxPositionSizePercent = goal.MaxPositionSizePercent,
                TradingSession = goal.TradingSession.ToString(),
                AllowOvernightPositions = goal.AllowOvernightPositions,
                Status = goal.Status.ToString(),
                StatusReason = goal.StatusReason,
                ProgressPercent = goal.ProgressPercent,
                RequiredDailyGrowthPercent = goal.RequiredDailyGrowthPercent,
                LatestPlanSummary = goal.LatestPlanSummary,
                LatestNextAction = goal.LatestNextAction,
                LastEvaluatedAtUtc = goal.LastEvaluatedAtUtc?.ToString("O"),
                LastExecutedAtUtc = goal.LastExecutedAtUtc?.ToString("O"),
                LastExecutionAttemptAtUtc = goal.LastExecutionAttemptAtUtc?.ToString("O"),
                ExecutedTradesCount = goal.ExecutedTradesCount,
                LastTradeId = goal.LastTradeId,
                LastError = goal.LastError,
                LatestEvaluation = latestEvaluation == null ? null : new GoalEvaluationRunDto
                {
                    Id = latestEvaluation.Id,
                    GoalStatus = latestEvaluation.GoalStatus.ToString(),
                    CurrentEquity = latestEvaluation.CurrentEquity,
                    RequiredGrowthPercent = latestEvaluation.RequiredGrowthPercent,
                    RequiredDailyGrowthPercent = latestEvaluation.RequiredDailyGrowthPercent,
                    FeasibilityScore = latestEvaluation.FeasibilityScore,
                    Summary = latestEvaluation.Summary,
                    CounterProposalTargetEquity = latestEvaluation.CounterProposalTargetEquity,
                    CounterProposalTargetPercent = latestEvaluation.CounterProposalTargetPercent,
                    OccurredAtUtc = latestEvaluation.OccurredAtUtc.ToString("O")
                },
                LatestPlan = latestPlan == null ? null : new GoalExecutionPlanDto
                {
                    Id = latestPlan.Id,
                    ExecutionSymbol = latestPlan.ExecutionSymbol,
                    SuggestedDirection = latestPlan.SuggestedDirection?.ToString(),
                    SuggestedQuantity = latestPlan.SuggestedQuantity,
                    SuggestedStopLoss = latestPlan.SuggestedStopLoss,
                    SuggestedTakeProfit = latestPlan.SuggestedTakeProfit,
                    RiskScore = latestPlan.RiskScore,
                    Summary = latestPlan.Summary,
                    NextAction = latestPlan.NextAction,
                    GeneratedAtUtc = latestPlan.GeneratedAtUtc.ToString("O")
                },
                Events = events.Select(item => new GoalExecutionEventDto
                {
                    Id = item.Id,
                    EventType = item.EventType,
                    Status = item.Status,
                    Summary = item.Summary,
                    TradeId = item.TradeId,
                    EquityAfterExecution = item.EquityAfterExecution,
                    OccurredAtUtc = item.OccurredAtUtc.ToString("O")
                }).ToList()
            };
        }
    }
}
