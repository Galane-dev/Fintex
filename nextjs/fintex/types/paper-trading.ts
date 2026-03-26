export type PaperOrderStatus = "Pending" | "Filled" | "Cancelled" | "Rejected";
export type PaperOrderType = "Market" | "Limit" | "Stop";
export type PaperPositionStatus = "Open" | "Closed";
export type TradeDirection = "Buy" | "Sell";

export interface PaperTradingAccount {
  id: number;
  name: string;
  baseCurrency: string;
  startingBalance: number;
  cashBalance: number;
  equity: number;
  realizedProfitLoss: number;
  unrealizedProfitLoss: number;
  isActive: boolean;
  lastMarkedToMarketAt: string;
}

export interface PaperOrder {
  id: number;
  accountId: number;
  positionId: number | null;
  symbol: string;
  assetClass: number;
  provider: number;
  direction: TradeDirection;
  orderType: PaperOrderType;
  status: PaperOrderStatus;
  quantity: number;
  requestedPrice: number | null;
  executedPrice: number | null;
  stopLoss: number | null;
  takeProfit: number | null;
  notes: string;
  submittedAt: string;
  executedAt: string | null;
}

export interface PaperPosition {
  id: number;
  accountId: number;
  symbol: string;
  assetClass: number;
  provider: number;
  direction: TradeDirection;
  status: PaperPositionStatus;
  quantity: number;
  averageEntryPrice: number;
  currentMarketPrice: number;
  realizedProfitLoss: number;
  unrealizedProfitLoss: number;
  stopLoss: number | null;
  takeProfit: number | null;
  openedAt: string;
  lastUpdatedAt: string;
  closedAt: string | null;
}

export interface PaperTradeFill {
  id: number;
  accountId: number;
  orderId: number;
  positionId: number | null;
  symbol: string;
  assetClass: number;
  provider: number;
  direction: TradeDirection;
  quantity: number;
  price: number;
  realizedProfitLoss: number;
  executedAt: string;
}

export interface PaperTradingSnapshot {
  account: PaperTradingAccount | null;
  positions: PaperPosition[];
  recentOrders: PaperOrder[];
  recentFills: PaperTradeFill[];
}

export interface CreatePaperTradingAccountInput {
  name: string;
  baseCurrency: string;
  startingBalance: number;
}

export interface PlacePaperOrderInput {
  symbol: string;
  assetClass: number;
  provider: number;
  direction: TradeDirection;
  quantity: number;
  stopLoss?: number | null;
  takeProfit?: number | null;
  notes?: string;
}

export interface ClosePaperPositionInput {
  positionId: number;
  quantity?: number | null;
  exitPrice?: number | null;
}

export interface PaperTradingState {
  isLoading: boolean;
  isSubmitting: boolean;
  error: string | null;
  snapshot: PaperTradingSnapshot | null;
  lastHydratedAt: string | null;
}

export interface PaperTradingProviderActions {
  refreshSnapshot: () => Promise<void>;
  createAccount: (input: CreatePaperTradingAccountInput) => Promise<void>;
  placeOrder: (input: PlacePaperOrderInput) => Promise<void>;
  closePosition: (input: ClosePaperPositionInput) => Promise<void>;
  clearError: () => void;
}
