import type {
  ClosePaperPositionInput,
  CreatePaperTradingAccountInput,
  PaperOrder,
  PaperTradeFill,
  PaperTradingAccount,
  PaperTradingSnapshot,
  PaperPosition,
  PlacePaperOrderInput,
} from "@/types/paper-trading";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getNullableNumber = (value: unknown) =>
  value == null ? null : typeof value === "number" ? value : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

export const normalizePaperTradingAccount = (
  value: Record<string, unknown>,
): PaperTradingAccount => ({
  id: getNumber(value.id ?? value.Id),
  name: getString(value.name ?? value.Name),
  baseCurrency: getString(value.baseCurrency ?? value.BaseCurrency),
  startingBalance: getNumber(value.startingBalance ?? value.StartingBalance),
  cashBalance: getNumber(value.cashBalance ?? value.CashBalance),
  equity: getNumber(value.equity ?? value.Equity),
  realizedProfitLoss: getNumber(value.realizedProfitLoss ?? value.RealizedProfitLoss),
  unrealizedProfitLoss: getNumber(value.unrealizedProfitLoss ?? value.UnrealizedProfitLoss),
  isActive: Boolean(value.isActive ?? value.IsActive),
  lastMarkedToMarketAt: getString(
    value.lastMarkedToMarketAt ?? value.LastMarkedToMarketAt,
  ),
});

export const normalizePaperOrder = (value: Record<string, unknown>): PaperOrder => ({
  id: getNumber(value.id ?? value.Id),
  accountId: getNumber(value.accountId ?? value.AccountId),
  positionId: getNullableNumber(value.positionId ?? value.PositionId),
  symbol: getString(value.symbol ?? value.Symbol),
  assetClass: getNumber(value.assetClass ?? value.AssetClass),
  provider: getNumber(value.provider ?? value.Provider),
  direction: getString(value.direction ?? value.Direction) as PaperOrder["direction"],
  orderType: getString(value.orderType ?? value.OrderType) as PaperOrder["orderType"],
  status: getString(value.status ?? value.Status) as PaperOrder["status"],
  quantity: getNumber(value.quantity ?? value.Quantity),
  requestedPrice: getNullableNumber(value.requestedPrice ?? value.RequestedPrice),
  executedPrice: getNullableNumber(value.executedPrice ?? value.ExecutedPrice),
  stopLoss: getNullableNumber(value.stopLoss ?? value.StopLoss),
  takeProfit: getNullableNumber(value.takeProfit ?? value.TakeProfit),
  notes: getString(value.notes ?? value.Notes),
  submittedAt: getString(value.submittedAt ?? value.SubmittedAt),
  executedAt:
    value.executedAt == null && value.ExecutedAt == null
      ? null
      : getString(value.executedAt ?? value.ExecutedAt),
});

export const normalizePaperPosition = (
  value: Record<string, unknown>,
): PaperPosition => ({
  id: getNumber(value.id ?? value.Id),
  accountId: getNumber(value.accountId ?? value.AccountId),
  symbol: getString(value.symbol ?? value.Symbol),
  assetClass: getNumber(value.assetClass ?? value.AssetClass),
  provider: getNumber(value.provider ?? value.Provider),
  direction: getString(value.direction ?? value.Direction) as PaperPosition["direction"],
  status: getString(value.status ?? value.Status) as PaperPosition["status"],
  quantity: getNumber(value.quantity ?? value.Quantity),
  averageEntryPrice: getNumber(value.averageEntryPrice ?? value.AverageEntryPrice),
  currentMarketPrice: getNumber(value.currentMarketPrice ?? value.CurrentMarketPrice),
  realizedProfitLoss: getNumber(value.realizedProfitLoss ?? value.RealizedProfitLoss),
  unrealizedProfitLoss: getNumber(value.unrealizedProfitLoss ?? value.UnrealizedProfitLoss),
  stopLoss: getNullableNumber(value.stopLoss ?? value.StopLoss),
  takeProfit: getNullableNumber(value.takeProfit ?? value.TakeProfit),
  openedAt: getString(value.openedAt ?? value.OpenedAt),
  lastUpdatedAt: getString(value.lastUpdatedAt ?? value.LastUpdatedAt),
  closedAt:
    value.closedAt == null && value.ClosedAt == null
      ? null
      : getString(value.closedAt ?? value.ClosedAt),
});

export const normalizePaperTradeFill = (
  value: Record<string, unknown>,
): PaperTradeFill => ({
  id: getNumber(value.id ?? value.Id),
  accountId: getNumber(value.accountId ?? value.AccountId),
  orderId: getNumber(value.orderId ?? value.OrderId),
  positionId: getNullableNumber(value.positionId ?? value.PositionId),
  symbol: getString(value.symbol ?? value.Symbol),
  assetClass: getNumber(value.assetClass ?? value.AssetClass),
  provider: getNumber(value.provider ?? value.Provider),
  direction: getString(value.direction ?? value.Direction) as PaperTradeFill["direction"],
  quantity: getNumber(value.quantity ?? value.Quantity),
  price: getNumber(value.price ?? value.Price),
  realizedProfitLoss: getNumber(value.realizedProfitLoss ?? value.RealizedProfitLoss),
  executedAt: getString(value.executedAt ?? value.ExecutedAt),
});

export const normalizePaperTradingSnapshot = (
  value: Record<string, unknown>,
): PaperTradingSnapshot => ({
  account:
    value.account && typeof value.account === "object"
      ? normalizePaperTradingAccount(value.account as Record<string, unknown>)
      : null,
  positions: Array.isArray(value.positions ?? value.Positions)
    ? ((value.positions ?? value.Positions) as Record<string, unknown>[]).map(
        normalizePaperPosition,
      )
    : [],
  recentOrders: Array.isArray(value.recentOrders ?? value.RecentOrders)
    ? ((value.recentOrders ?? value.RecentOrders) as Record<string, unknown>[]).map(
        normalizePaperOrder,
      )
    : [],
  recentFills: Array.isArray(value.recentFills ?? value.RecentFills)
    ? ((value.recentFills ?? value.RecentFills) as Record<string, unknown>[]).map(
        normalizePaperTradeFill,
      )
    : [],
});

export const buildPaperTradingAccountInput = (
  input: CreatePaperTradingAccountInput,
): Record<string, unknown> => ({
  name: input.name,
  baseCurrency: input.baseCurrency,
  startingBalance: input.startingBalance,
});

export const buildPlacePaperOrderInput = (
  input: PlacePaperOrderInput,
): Record<string, unknown> => ({
  symbol: input.symbol,
  assetClass: input.assetClass,
  provider: input.provider,
  direction: input.direction,
  quantity: input.quantity,
  stopLoss: input.stopLoss ?? null,
  takeProfit: input.takeProfit ?? null,
  notes: input.notes ?? "",
});

export const buildClosePaperPositionInput = (
  input: ClosePaperPositionInput,
): Record<string, unknown> => ({
  positionId: input.positionId,
  quantity: input.quantity ?? null,
  exitPrice: input.exitPrice ?? null,
});
