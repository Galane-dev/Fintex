import { MARKET_PROVIDER_LABELS } from "@/constants/market-data";
import type {
  MarketConnectionStatus,
  MarketDataPoint,
  MarketDataProvider,
  MarketProjectionMaturity,
  MarketVerdict,
  MarketVerdictState,
} from "@/types/market-data";

const EMPTY = "—";

export const getVerdictLabel = (value: unknown): MarketVerdict => {
  if (value === 2 || value === "Buy") {
    return "Buy";
  }

  if (value === 3 || value === "Sell") {
    return "Sell";
  }

  return "Hold";
};

export const getVerdictStateLabel = (value: MarketVerdictState) => {
  switch (value) {
    case "warming_up":
      return "Warming up";
    case "degraded":
      return "Degraded";
    case "stale":
      return "Stale";
    case "fallback":
      return "Fallback";
    default:
      return "Live";
  }
};

export const getVerdictStateTone = (value: MarketVerdictState) => {
  switch (value) {
    case "live":
      return "green";
    case "warming_up":
      return "blue";
    case "degraded":
      return "gold";
    case "stale":
    case "fallback":
      return "red";
    default:
      return "default";
  }
};

export const getProjectionMaturityLabel = (value: MarketProjectionMaturity) => {
  switch (value) {
    case "warming_up":
      return "Warm-up";
    case "forming":
      return "Forming";
    default:
      return "Mature";
  }
};

export const getProviderLabel = (provider: MarketDataProvider) =>
  MARKET_PROVIDER_LABELS[provider] ?? "Provider";

export const sortHistoryAscending = (history: MarketDataPoint[]) =>
  [...history].sort(
    (left, right) =>
      new Date(left.timestamp).getTime() - new Date(right.timestamp).getTime(),
  );

export const upsertHistoryPoint = (
  history: MarketDataPoint[],
  point: MarketDataPoint,
  maxPoints = 120,
) => {
  const nextHistory = history.some((item) => item.id === point.id)
    ? history.map((item) => (item.id === point.id ? point : item))
    : [...history, point];

  return sortHistoryAscending(nextHistory).slice(-maxPoints);
};

export const formatPrice = (value: number | null | undefined) =>
  value == null
    ? EMPTY
    : value.toLocaleString(undefined, {
        minimumFractionDigits: value > 100 ? 2 : 4,
        maximumFractionDigits: value > 100 ? 2 : 4,
      });

export const formatCompact = (value: number | null | undefined) =>
  value == null
    ? EMPTY
    : new Intl.NumberFormat(undefined, {
        notation: "compact",
        maximumFractionDigits: 1,
      }).format(value);

export const formatSigned = (value: number | null | undefined, digits = 2) =>
  value == null ? EMPTY : `${value >= 0 ? "+" : ""}${value.toFixed(digits)}`;

export const formatPercent = (value: number | null | undefined, digits = 2) =>
  value == null ? EMPTY : `${value >= 0 ? "+" : ""}${value.toFixed(digits)}%`;

export const formatSignedPoints = (
  value: number | null | undefined,
  digits = 2,
) => (value == null ? EMPTY : `${value >= 0 ? "+" : ""}${value.toFixed(digits)}`);

export const formatTime = (value: string | null | undefined) =>
  value
    ? new Intl.DateTimeFormat(undefined, {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
      }).format(new Date(value))
    : EMPTY;

export const formatDateTime = (value: string | null | undefined) =>
  value
    ? new Intl.DateTimeFormat(undefined, {
        year: "numeric",
        month: "short",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      }).format(new Date(value))
    : EMPTY;

export const getConnectionTone = (status: MarketConnectionStatus) => {
  switch (status) {
    case "connected":
      return "green";
    case "reconnecting":
      return "gold";
    case "error":
      return "red";
    default:
      return "default";
  }
};
