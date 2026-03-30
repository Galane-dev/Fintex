import type {
  GoalExecutionEvent,
  GoalExecutionPlan,
  GoalEvaluationRun,
  GoalTarget,
} from "@/types/goal-automation";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getNullableNumber = (value: unknown) =>
  value == null ? null : typeof value === "number" ? value : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

const normalizeGoalEvaluation = (value: Record<string, unknown>): GoalEvaluationRun => ({
  id: getNumber(value.id ?? value.Id),
  goalStatus: getString(value.goalStatus ?? value.GoalStatus) as GoalEvaluationRun["goalStatus"],
  currentEquity: getNumber(value.currentEquity ?? value.CurrentEquity),
  requiredGrowthPercent: getNumber(value.requiredGrowthPercent ?? value.RequiredGrowthPercent),
  requiredDailyGrowthPercent: getNumber(value.requiredDailyGrowthPercent ?? value.RequiredDailyGrowthPercent),
  feasibilityScore: getNumber(value.feasibilityScore ?? value.FeasibilityScore),
  summary: getString(value.summary ?? value.Summary),
  counterProposalTargetEquity: getNullableNumber(value.counterProposalTargetEquity ?? value.CounterProposalTargetEquity),
  counterProposalTargetPercent: getNullableNumber(value.counterProposalTargetPercent ?? value.CounterProposalTargetPercent),
  occurredAtUtc: getString(value.occurredAtUtc ?? value.OccurredAtUtc),
});

const normalizeGoalPlan = (value: Record<string, unknown>): GoalExecutionPlan => ({
  id: getNumber(value.id ?? value.Id),
  executionSymbol: getString(value.executionSymbol ?? value.ExecutionSymbol),
  suggestedDirection:
    value.suggestedDirection == null && value.SuggestedDirection == null
      ? null
      : (getString(value.suggestedDirection ?? value.SuggestedDirection) as GoalExecutionPlan["suggestedDirection"]),
  suggestedQuantity: getNullableNumber(value.suggestedQuantity ?? value.SuggestedQuantity),
  suggestedStopLoss: getNullableNumber(value.suggestedStopLoss ?? value.SuggestedStopLoss),
  suggestedTakeProfit: getNullableNumber(value.suggestedTakeProfit ?? value.SuggestedTakeProfit),
  riskScore: getNullableNumber(value.riskScore ?? value.RiskScore),
  summary: getString(value.summary ?? value.Summary),
  nextAction: getString(value.nextAction ?? value.NextAction),
  generatedAtUtc: getString(value.generatedAtUtc ?? value.GeneratedAtUtc),
});

const normalizeGoalEvent = (value: Record<string, unknown>): GoalExecutionEvent => ({
  id: getNumber(value.id ?? value.Id),
  eventType: getString(value.eventType ?? value.EventType),
  status: getString(value.status ?? value.Status),
  summary: getString(value.summary ?? value.Summary),
  tradeId: getNullableNumber(value.tradeId ?? value.TradeId),
  equityAfterExecution: getNullableNumber(value.equityAfterExecution ?? value.EquityAfterExecution),
  occurredAtUtc: getString(value.occurredAtUtc ?? value.OccurredAtUtc),
});

export const normalizeGoalTarget = (value: Record<string, unknown>): GoalTarget => ({
  id: getNumber(value.id ?? value.Id),
  name: getString(value.name ?? value.Name),
  accountType: getString(value.accountType ?? value.AccountType) as GoalTarget["accountType"],
  externalConnectionId: getNullableNumber(value.externalConnectionId ?? value.ExternalConnectionId),
  externalConnectionName:
    value.externalConnectionName == null && value.ExternalConnectionName == null
      ? null
      : getString(value.externalConnectionName ?? value.ExternalConnectionName),
  marketSymbol: getString(value.marketSymbol ?? value.MarketSymbol),
  allowedSymbols: getString(value.allowedSymbols ?? value.AllowedSymbols),
  targetType: getString(value.targetType ?? value.TargetType) as GoalTarget["targetType"],
  startEquity: getNumber(value.startEquity ?? value.StartEquity),
  currentEquity: getNumber(value.currentEquity ?? value.CurrentEquity),
  targetEquity: getNumber(value.targetEquity ?? value.TargetEquity),
  targetPercent: getNumber(value.targetPercent ?? value.TargetPercent),
  deadlineUtc: getString(value.deadlineUtc ?? value.DeadlineUtc),
  maxAcceptableRisk: getNumber(value.maxAcceptableRisk ?? value.MaxAcceptableRisk),
  maxDrawdownPercent: getNumber(value.maxDrawdownPercent ?? value.MaxDrawdownPercent),
  maxPositionSizePercent: getNumber(value.maxPositionSizePercent ?? value.MaxPositionSizePercent),
  tradingSession: getString(value.tradingSession ?? value.TradingSession) as GoalTarget["tradingSession"],
  allowOvernightPositions: Boolean(value.allowOvernightPositions ?? value.AllowOvernightPositions),
  status: getString(value.status ?? value.Status) as GoalTarget["status"],
  statusReason:
    value.statusReason == null && value.StatusReason == null ? null : getString(value.statusReason ?? value.StatusReason),
  progressPercent: getNumber(value.progressPercent ?? value.ProgressPercent),
  requiredDailyGrowthPercent: getNumber(value.requiredDailyGrowthPercent ?? value.RequiredDailyGrowthPercent),
  latestPlanSummary:
    value.latestPlanSummary == null && value.LatestPlanSummary == null ? null : getString(value.latestPlanSummary ?? value.LatestPlanSummary),
  latestNextAction:
    value.latestNextAction == null && value.LatestNextAction == null ? null : getString(value.latestNextAction ?? value.LatestNextAction),
  lastEvaluatedAtUtc:
    value.lastEvaluatedAtUtc == null && value.LastEvaluatedAtUtc == null ? null : getString(value.lastEvaluatedAtUtc ?? value.LastEvaluatedAtUtc),
  lastExecutedAtUtc:
    value.lastExecutedAtUtc == null && value.LastExecutedAtUtc == null ? null : getString(value.lastExecutedAtUtc ?? value.LastExecutedAtUtc),
  lastExecutionAttemptAtUtc:
    value.lastExecutionAttemptAtUtc == null && value.LastExecutionAttemptAtUtc == null ? null : getString(value.lastExecutionAttemptAtUtc ?? value.LastExecutionAttemptAtUtc),
  executedTradesCount: getNumber(value.executedTradesCount ?? value.ExecutedTradesCount),
  lastTradeId: getNullableNumber(value.lastTradeId ?? value.LastTradeId),
  lastError:
    value.lastError == null && value.LastError == null ? null : getString(value.lastError ?? value.LastError),
  latestEvaluation:
    value.latestEvaluation && typeof value.latestEvaluation === "object"
      ? normalizeGoalEvaluation(value.latestEvaluation as Record<string, unknown>)
      : value.LatestEvaluation && typeof value.LatestEvaluation === "object"
        ? normalizeGoalEvaluation(value.LatestEvaluation as Record<string, unknown>)
        : null,
  latestPlan:
    value.latestPlan && typeof value.latestPlan === "object"
      ? normalizeGoalPlan(value.latestPlan as Record<string, unknown>)
      : value.LatestPlan && typeof value.LatestPlan === "object"
        ? normalizeGoalPlan(value.LatestPlan as Record<string, unknown>)
        : null,
  events: Array.isArray(value.events ?? value.Events)
    ? ((value.events ?? value.Events) as Record<string, unknown>[]).map(normalizeGoalEvent)
    : [],
});
