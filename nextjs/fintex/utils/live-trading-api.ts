import type { LiveTrade, LiveTradeExecution, PlaceLiveOrderInput } from "@/types/live-trading";
import { apiClient } from "./api-client";
import { unwrapAbpResponse } from "./abp-response";
import {
  buildPlaceLiveOrderInput,
  normalizeLiveTrade,
  normalizeLiveTradeExecution,
} from "./live-trading";

interface ListResult<T> {
  items?: T[];
}

export const getMyLiveTrades = async (): Promise<LiveTrade[]> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    apiClient.get("/api/services/app/Trade/GetMyTrades"),
    "We could not load your live trades.",
  );

  return Array.isArray(result?.items) ? result.items.map(normalizeLiveTrade) : [];
};

export const syncExternalBrokerTrades = async (): Promise<void> => {
  await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.post("/api/services/app/ExternalBrokerTrading/SyncMyConnections", {}),
    "We could not sync your external broker trades.",
  );
};

export const placeLiveOrder = async (
  input: PlaceLiveOrderInput,
): Promise<LiveTradeExecution> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.post(
      "/api/services/app/ExternalBrokerTrading/PlaceMarketOrder",
      buildPlaceLiveOrderInput(input),
    ),
    "We could not place the live broker order.",
  );

  return normalizeLiveTradeExecution(result);
};
