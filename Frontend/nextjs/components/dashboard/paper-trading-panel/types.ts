"use client";

import type { FormInstance } from "antd";
import type {
  AcademyStatus,
} from "@/types/academy";
import type {
  ExternalBrokerConnection,
  ExternalBrokerConnectionStatus,
} from "@/types/external-broker";
import type {
  LiveTrade,
  LiveTradeExecution,
} from "@/types/live-trading";
import type {
  PaperTradeAssessment,
  PaperTradeRecommendation,
  PaperTradingAccount,
  PaperTradingSnapshot,
} from "@/types/paper-trading";

export interface PaperTradingPanelProps {
  currentPrice: number | null;
  registerDashboardActions?: (actions: DashboardPaperTradingActions) => void;
  displayMode?: "full" | "support";
}

export interface DashboardPaperTradingActions {
  hasAccount: boolean;
  openAccounts: () => void;
  openRecommendation: () => void;
  openTrade: (direction: "Buy" | "Sell") => void;
}

export interface ExecutionTargetOption {
  label: string;
  value: string;
}

export interface AccountMetric {
  label: string;
  value: string;
  tone: "positive" | "negative" | "neutral";
}

export interface AccountFormValues {
  name: string;
  baseCurrency: string;
  startingBalance: number;
}

export interface ExternalBrokerFormValues {
  displayName: string;
  apiKey: string;
  apiSecret: string;
  environment: "paper" | "live";
}

export interface TradeFormValues {
  executionTarget?: string;
  quantity?: number;
  stopLoss?: number;
  takeProfit?: number;
  notes?: string;
}

export interface PaperTradingPanelController {
  account: PaperTradingAccount | null;
  accountForm: FormInstance<AccountFormValues>;
  accountMetrics: AccountMetric[];
  activeFeedback: PaperTradeAssessment | null;
  academyStatus: AcademyStatus | null;
  availableExecutionTargets: ExecutionTargetOption[];
  canConnectExternalBrokers: boolean;
  combinedError: string | null;
  externalBrokerForm: FormInstance<ExternalBrokerFormValues>;
  fills: PaperTradingSnapshot["recentFills"];
  isAccountsOpen: boolean;
  isAssessmentOpen: boolean;
  isBusy: boolean;
  isAcademyLoading: boolean;
  isExternalLoading: boolean;
  isExternalSubmitting: boolean;
  isLoading: boolean;
  isRecommendationLoading: boolean;
  isRecommendationOpen: boolean;
  isSubmitting: boolean;
  isTradeOpen: boolean;
  latestLiveExecution: LiveTradeExecution | null;
  liveTrades: LiveTrade[];
  orders: PaperTradingSnapshot["recentOrders"];
  positions: PaperTradingSnapshot["positions"];
  recommendation: PaperTradeRecommendation | null;
  recommendationRequestError: string | null;
  sortedConnections: ExternalBrokerConnection[];
  tradeDirection: "Buy" | "Sell";
  tradeForm: FormInstance<TradeFormValues>;
  effectiveQuantity: number;
  feedbackTone: "error" | "warning" | "success" | null;
  recommendationTone: "error" | "warning" | "success" | null;
  openAccountsModal: () => void;
  closeAccountsModal: () => void;
  openTradeModal: (direction: "Buy" | "Sell") => void;
  closeTradeModal: () => void;
  openRecommendationModal: () => Promise<void>;
  closeRecommendationModal: () => void;
  closeAssessmentModal: () => void;
  handleAccountCreate: (values: AccountFormValues) => Promise<void>;
  handleConnectExternalBroker: (values: ExternalBrokerFormValues) => Promise<void>;
  handleDisconnectExternalBroker: (id: number) => Promise<void>;
  submitTrade: (direction: "Buy" | "Sell") => Promise<void>;
  handleApplyAssessmentSuggestions: () => Promise<void>;
  handlePlaceSuggestedTrade: () => Promise<void>;
  handleClosePaperPosition: (positionId: number) => Promise<void>;
  handleClearAnyError: () => void;
  getConnectionStatusTone: (
    status: ExternalBrokerConnectionStatus,
  ) => "green" | "gold" | "red" | "default";
}
