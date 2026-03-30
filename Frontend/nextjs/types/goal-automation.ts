export type GoalAccountType = "PaperTrading" | "ExternalBroker";
export type GoalTargetType = "PercentGrowth" | "TargetAmount";
export type GoalTradingSession = "AnyTime" | "Europe" | "Us" | "EuropeUsOverlap";
export type GoalStatus =
  | "Draft"
  | "Accepted"
  | "Rejected"
  | "Active"
  | "Paused"
  | "Completed"
  | "Expired"
  | "Canceled";

export type GoalEvaluationRun = {
  id: number;
  goalStatus: GoalStatus;
  currentEquity: number;
  requiredGrowthPercent: number;
  requiredDailyGrowthPercent: number;
  feasibilityScore: number;
  summary: string;
  counterProposalTargetEquity: number | null;
  counterProposalTargetPercent: number | null;
  occurredAtUtc: string;
};

export type GoalExecutionPlan = {
  id: number;
  executionSymbol: string;
  suggestedDirection: "Buy" | "Sell" | null;
  suggestedQuantity: number | null;
  suggestedStopLoss: number | null;
  suggestedTakeProfit: number | null;
  riskScore: number | null;
  summary: string;
  nextAction: string;
  generatedAtUtc: string;
};

export type GoalExecutionEvent = {
  id: number;
  eventType: string;
  status: string;
  summary: string;
  tradeId: number | null;
  equityAfterExecution: number | null;
  occurredAtUtc: string;
};

export type GoalTarget = {
  id: number;
  name: string;
  accountType: GoalAccountType;
  externalConnectionId: number | null;
  externalConnectionName: string | null;
  marketSymbol: string;
  allowedSymbols: string;
  targetType: GoalTargetType;
  startEquity: number;
  currentEquity: number;
  targetEquity: number;
  targetPercent: number;
  deadlineUtc: string;
  maxAcceptableRisk: number;
  maxDrawdownPercent: number;
  maxPositionSizePercent: number;
  tradingSession: GoalTradingSession;
  allowOvernightPositions: boolean;
  status: GoalStatus;
  statusReason: string | null;
  progressPercent: number;
  requiredDailyGrowthPercent: number;
  latestPlanSummary: string | null;
  latestNextAction: string | null;
  lastEvaluatedAtUtc: string | null;
  lastExecutedAtUtc: string | null;
  lastExecutionAttemptAtUtc: string | null;
  executedTradesCount: number;
  lastTradeId: number | null;
  lastError: string | null;
  latestEvaluation: GoalEvaluationRun | null;
  latestPlan: GoalExecutionPlan | null;
  events: GoalExecutionEvent[];
};

export type CreateGoalTargetInput = {
  name?: string;
  accountType: number;
  externalConnectionId?: number | null;
  targetType: number;
  targetPercent?: number | null;
  targetAmount?: number | null;
  deadlineUtc: string;
  maxAcceptableRisk: number;
  maxDrawdownPercent: number;
  maxPositionSizePercent: number;
  tradingSession: number;
  allowOvernightPositions: boolean;
};

export type GoalAutomationState = {
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
  goals: GoalTarget[];
};

export type GoalAutomationActions = {
  refreshGoals: () => Promise<void>;
  createGoal: (input: CreateGoalTargetInput) => Promise<boolean>;
  pauseGoal: (goalId: number) => Promise<void>;
  resumeGoal: (goalId: number) => Promise<void>;
  cancelGoal: (goalId: number) => Promise<void>;
  clearError: () => void;
};
