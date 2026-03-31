import type {
  ClosedTradeReview,
  LiveTrade,
  LiveTradeExecution,
  PlaceLiveOrderInput,
} from "@/types/live-trading";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getNullableNumber = (value: unknown) =>
  value == null ? null : typeof value === "number" ? value : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

const getNullableString = (value: unknown) =>
  value == null ? null : getString(value);

const normalizeClosedTradeReview = (
  value: unknown,
): ClosedTradeReview | null => {
  if (value == null || typeof value !== "object") {
    return null;
  }

  const payload = value as Record<string, unknown>;

  return {
    good: getString(payload.good ?? payload.Good),
    bad: getString(payload.bad ?? payload.Bad),
    repeatedPattern: getString(
      payload.repeatedPattern ?? payload.RepeatedPattern,
    ),
    provider: getString(payload.provider ?? payload.Provider),
    model: getString(payload.model ?? payload.Model),
    wasGenerated:
      typeof (payload.wasGenerated ?? payload.WasGenerated) === "boolean"
        ? Boolean(payload.wasGenerated ?? payload.WasGenerated)
        : false,
  };
};

const normalizeAssetClass = (value: unknown): LiveTrade["assetClass"] => {
  const raw = getNumber(value, 1);

  if (raw === 2) {
    return 2;
  }

  return 1;
};

const normalizeProvider = (value: unknown): LiveTrade["provider"] => {
  const raw = getNumber(value, 1);

  if (raw === 2 || raw === 3) {
    return raw;
  }

  return 1;
};

const normalizeDirection = (value: unknown): LiveTrade["direction"] => {
  if (typeof value === "string") {
    return value.toLowerCase() === "sell" ? "Sell" : "Buy";
  }

  return getNumber(value, 1) === 2 ? "Sell" : "Buy";
};

const normalizeStatus = (value: unknown): LiveTrade["status"] => {
  if (typeof value === "string") {
    const normalized = value.toLowerCase();

    if (normalized === "open") {
      return "Open";
    }

    if (normalized === "closed") {
      return "Closed";
    }

    if (normalized === "cancelled" || normalized === "canceled") {
      return "Cancelled";
    }
  }

  const raw = getNumber(value, 2);
  if (raw === 3) {
    return "Closed";
  }

  if (raw === 4) {
    return "Cancelled";
  }

  return "Open";
};

export const normalizeLiveTrade = (value: Record<string, unknown>): LiveTrade => ({
  id: getNumber(value.id ?? value.Id),
  userId: getNumber(value.userId ?? value.UserId),
  symbol: getString(value.symbol ?? value.Symbol),
  assetClass: normalizeAssetClass(value.assetClass ?? value.AssetClass),
  provider: normalizeProvider(value.provider ?? value.Provider),
  direction: normalizeDirection(value.direction ?? value.Direction),
  status: normalizeStatus(value.status ?? value.Status),
  quantity: getNumber(value.quantity ?? value.Quantity),
  entryPrice: getNumber(value.entryPrice ?? value.EntryPrice),
  exitPrice: getNullableNumber(value.exitPrice ?? value.ExitPrice),
  stopLoss: getNullableNumber(value.stopLoss ?? value.StopLoss),
  takeProfit: getNullableNumber(value.takeProfit ?? value.TakeProfit),
  realizedProfitLoss: getNumber(value.realizedProfitLoss ?? value.RealizedProfitLoss),
  unrealizedProfitLoss: getNumber(value.unrealizedProfitLoss ?? value.UnrealizedProfitLoss),
  lastMarketPrice: getNumber(value.lastMarketPrice ?? value.LastMarketPrice),
  currentRiskScore: getNumber(value.currentRiskScore ?? value.CurrentRiskScore),
  currentRecommendation: getString(
    value.currentRecommendation ?? value.CurrentRecommendation,
  ),
  currentAnalysisSummary: getString(
    value.currentAnalysisSummary ?? value.CurrentAnalysisSummary,
  ),
  externalOrderId: getString(value.externalOrderId ?? value.ExternalOrderId),
  notes: getString(value.notes ?? value.Notes),
  executedAt: getString(value.executedAt ?? value.ExecutedAt),
  closedAt: getNullableString(value.closedAt ?? value.ClosedAt),
  closedTradeReview: normalizeClosedTradeReview(
    value.closedTradeReview ?? value.ClosedTradeReview,
  ),
});

export const normalizeLiveTradeExecution = (
  value: Record<string, unknown>,
): LiveTradeExecution => ({
  connectionId: getNumber(value.connectionId ?? value.ConnectionId),
  brokerName: getString(value.brokerName ?? value.BrokerName),
  brokerEnvironment: getString(value.brokerEnvironment ?? value.BrokerEnvironment),
  brokerSymbol: getString(value.brokerSymbol ?? value.BrokerSymbol),
  brokerOrderId: getString(value.brokerOrderId ?? value.BrokerOrderId),
  brokerOrderStatus: getString(value.brokerOrderStatus ?? value.BrokerOrderStatus),
  filledAveragePrice: getNullableNumber(
    value.filledAveragePrice ?? value.FilledAveragePrice,
  ),
  headline: getString(value.headline ?? value.Headline),
  summary: getString(value.summary ?? value.Summary),
  trade: normalizeLiveTrade((value.trade ?? value.Trade) as Record<string, unknown>),
});

export const buildPlaceLiveOrderInput = (
  input: PlaceLiveOrderInput,
): Record<string, unknown> => ({
  connectionId: input.connectionId,
  symbol: input.symbol,
  assetClass: input.assetClass,
  provider: input.provider,
  direction: input.direction,
  quantity: input.quantity,
  stopLoss: input.stopLoss ?? null,
  takeProfit: input.takeProfit ?? null,
  notes: input.notes ?? "",
});
