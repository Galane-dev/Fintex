export type TradeAutomationTriggerType =
  | "PriceTarget"
  | "RelativeStrengthIndex"
  | "MacdHistogram"
  | "Momentum"
  | "TrendScore"
  | "ConfidenceScore"
  | "Verdict";

export type TradeAutomationDestination = "PaperTrading" | "ExternalBroker";

export type TradeAutomationRule = {
  id: number;
  name: string;
  symbol: string;
  provider: string;
  triggerType: TradeAutomationTriggerType;
  createdMetricValue: number | null;
  lastObservedMetricValue: number | null;
  triggerValue: number | null;
  targetVerdict: "Buy" | "Sell" | "Hold" | null;
  minimumConfidenceScore: number | null;
  destination: TradeAutomationDestination;
  externalConnectionId: number | null;
  tradeDirection: "Buy" | "Sell";
  quantity: number;
  stopLoss: number | null;
  takeProfit: number | null;
  notifyInApp: boolean;
  notifyEmail: boolean;
  isActive: boolean;
  notes: string | null;
  creationTime: string;
  lastTriggeredAt: string | null;
  lastTradeId: number | null;
};

export type CreateTradeAutomationRuleInput = {
  name: string;
  symbol: string;
  provider: number;
  triggerType: number;
  triggerValue?: number | null;
  targetVerdict?: number | null;
  minimumConfidenceScore?: number | null;
  destination: number;
  externalConnectionId?: number | null;
  tradeDirection: number;
  quantity: number;
  stopLoss?: number | null;
  takeProfit?: number | null;
  notifyInApp: boolean;
  notifyEmail: boolean;
  notes?: string;
};

export type TradeAutomationState = {
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
  rules: TradeAutomationRule[];
};

export type TradeAutomationActions = {
  refreshRules: () => Promise<void>;
  createRule: (input: CreateTradeAutomationRuleInput) => Promise<boolean>;
  deleteRule: (ruleId: number) => Promise<void>;
  clearError: () => void;
};
