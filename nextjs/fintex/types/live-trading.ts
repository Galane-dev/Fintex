export type TradeDirection = "Buy" | "Sell";
export type TradeStatus = "Open" | "Closed" | "Cancelled";

export interface LiveTrade {
  id: number;
  userId: number;
  symbol: string;
  assetClass: number;
  provider: number;
  direction: TradeDirection;
  status: TradeStatus;
  quantity: number;
  entryPrice: number;
  exitPrice: number | null;
  stopLoss: number | null;
  takeProfit: number | null;
  realizedProfitLoss: number;
  unrealizedProfitLoss: number;
  lastMarketPrice: number;
  currentRiskScore: number;
  currentRecommendation: string;
  currentAnalysisSummary: string;
  externalOrderId: string;
  notes: string;
  executedAt: string;
  closedAt: string | null;
}

export interface PlaceLiveOrderInput {
  connectionId: number;
  symbol: string;
  assetClass: number;
  provider: number;
  direction: TradeDirection;
  quantity: number;
  stopLoss?: number | null;
  takeProfit?: number | null;
  notes?: string;
}

export interface LiveTradeExecution {
  connectionId: number;
  brokerName: string;
  brokerEnvironment: string;
  brokerSymbol: string;
  brokerOrderId: string;
  brokerOrderStatus: string;
  filledAveragePrice: number | null;
  headline: string;
  summary: string;
  trade: LiveTrade;
}

export interface LiveTradingState {
  isLoading: boolean;
  isSubmitting: boolean;
  error: string | null;
  trades: LiveTrade[];
  lastExecution: LiveTradeExecution | null;
  lastHydratedAt: string | null;
}

export interface LiveTradingProviderActions {
  refreshTrades: () => Promise<void>;
  placeOrder: (input: PlaceLiveOrderInput) => Promise<LiveTradeExecution | null>;
  clearError: () => void;
  clearExecution: () => void;
}
