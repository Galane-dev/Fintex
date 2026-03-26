import type {
  MarketDataPoint,
  MarketSelection,
  MarketTimeframeRsi,
  MarketVerdictSnapshot,
} from "@/types/market-data";
import { apiClient } from "./api-client";
import { unwrapAbpResponse } from "./abp-response";
import {
  normalizeMarketDataPoint,
  normalizeTimeframeRsi,
  normalizeMarketVerdict,
  sortHistoryAscending,
} from "./market-data";

interface ListResult<T> {
  items?: T[];
}

const buildInput = (selection: MarketSelection, take = 80) => ({
  symbol: selection.symbol,
  provider: selection.provider,
  take,
});

export const getMarketHistory = async (
  selection: MarketSelection,
  take = 80,
): Promise<MarketDataPoint[]> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    apiClient.get("/api/services/app/MarketData/GetHistory", {
      params: buildInput(selection, take),
    }),
    "We could not load the market history.",
  );

  const history = Array.isArray(result?.items) ? result.items.map(normalizeMarketDataPoint) : [];
  return sortHistoryAscending(history);
};

export const getRealtimeVerdict = async (
  selection: MarketSelection,
): Promise<MarketVerdictSnapshot | null> => {
  const result = await unwrapAbpResponse<Record<string, unknown> | null>(
    apiClient.get("/api/services/app/MarketData/GetRealtimeVerdict", {
      params: buildInput(selection),
    }),
    "We could not load the realtime verdict.",
  );

  return result ? normalizeMarketVerdict(result) : null;
};

export const getRelativeStrengthIndexTimeframes = async (
  selection: MarketSelection,
): Promise<MarketTimeframeRsi[]> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    apiClient.get("/api/services/app/MarketData/GetRelativeStrengthIndexTimeframes", {
      params: buildInput(selection),
    }),
    "We could not load timeframe RSI values.",
  );

  return Array.isArray(result?.items)
    ? result.items.map(normalizeTimeframeRsi)
    : [];
};
