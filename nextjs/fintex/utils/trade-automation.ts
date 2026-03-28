import type { TradeAutomationRule, TradeAutomationTriggerType } from "@/types/trade-automation";

const parseNumber = (value: unknown) =>
  typeof value === "number"
    ? value
    : typeof value === "string" && value.trim() !== ""
      ? Number(value)
      : null;

const mapTriggerType = (value: unknown): TradeAutomationTriggerType => {
  if (value === 2 || value === "RelativeStrengthIndex") return "RelativeStrengthIndex";
  if (value === 3 || value === "MacdHistogram") return "MacdHistogram";
  if (value === 4 || value === "Momentum") return "Momentum";
  if (value === 5 || value === "TrendScore") return "TrendScore";
  if (value === 6 || value === "ConfidenceScore") return "ConfidenceScore";
  if (value === 7 || value === "Verdict") return "Verdict";
  return "PriceTarget";
};

export const normalizeTradeAutomationRule = (
  payload: Record<string, unknown>,
): TradeAutomationRule => ({
  id: Number(payload.id ?? payload.Id ?? 0),
  name: String(payload.name ?? payload.Name ?? "Automation rule"),
  symbol: String(payload.symbol ?? payload.Symbol ?? "BTCUSDT"),
  provider: String(payload.provider ?? payload.Provider ?? "Binance"),
  triggerType: mapTriggerType(payload.triggerType ?? payload.TriggerType),
  createdMetricValue: parseNumber(payload.createdMetricValue ?? payload.CreatedMetricValue),
  lastObservedMetricValue: parseNumber(payload.lastObservedMetricValue ?? payload.LastObservedMetricValue),
  triggerValue: parseNumber(payload.triggerValue ?? payload.TriggerValue),
  targetVerdict:
    typeof (payload.targetVerdict ?? payload.TargetVerdict) === "string"
      ? (String(payload.targetVerdict ?? payload.TargetVerdict) as "Buy" | "Sell" | "Hold")
      : null,
  minimumConfidenceScore: parseNumber(payload.minimumConfidenceScore ?? payload.MinimumConfidenceScore),
  destination:
    payload.destination === 2 || payload.destination === "ExternalBroker"
      ? "ExternalBroker"
      : "PaperTrading",
  externalConnectionId: parseNumber(payload.externalConnectionId ?? payload.ExternalConnectionId),
  tradeDirection:
    payload.tradeDirection === 2 || payload.tradeDirection === "Sell" ? "Sell" : "Buy",
  quantity: Number(payload.quantity ?? payload.Quantity ?? 0),
  stopLoss: parseNumber(payload.stopLoss ?? payload.StopLoss),
  takeProfit: parseNumber(payload.takeProfit ?? payload.TakeProfit),
  notifyInApp: Boolean(payload.notifyInApp ?? payload.NotifyInApp),
  notifyEmail: Boolean(payload.notifyEmail ?? payload.NotifyEmail),
  isActive: Boolean(payload.isActive ?? payload.IsActive),
  notes:
    typeof (payload.notes ?? payload.Notes) === "string"
      ? String(payload.notes ?? payload.Notes)
      : null,
  creationTime: String(payload.creationTime ?? payload.CreationTime ?? new Date().toISOString()),
  lastTriggeredAt:
    typeof (payload.lastTriggeredAt ?? payload.LastTriggeredAt) === "string"
      ? String(payload.lastTriggeredAt ?? payload.LastTriggeredAt)
      : null,
  lastTradeId: parseNumber(payload.lastTradeId ?? payload.LastTradeId),
});
