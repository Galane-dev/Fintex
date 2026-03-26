import type {
  ClosePaperPositionInput,
  CreatePaperTradingAccountInput,
  PaperOrder,
  PaperPosition,
  PaperTradeExecutionResult,
  PaperTradeRecommendation,
  PaperTradeFill,
  PaperTradingSnapshot,
  GetPaperTradeRecommendationInput,
  PlacePaperOrderInput,
} from "@/types/paper-trading";
import { apiClient } from "./api-client";
import { unwrapAbpResponse } from "./abp-response";
import {
  buildClosePaperPositionInput,
  buildPaperTradeRecommendationInput,
  buildPaperTradingAccountInput,
  buildPlacePaperOrderInput,
  normalizePaperOrder,
  normalizePaperPosition,
  normalizePaperTradeExecutionResult,
  normalizePaperTradeRecommendation,
  normalizePaperTradeFill,
  normalizePaperTradingSnapshot,
} from "./paper-trading";

interface ListResult<T> {
  items?: T[];
}

export const getPaperTradingSnapshot = async (): Promise<PaperTradingSnapshot> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.get("/api/services/app/PaperTrading/GetMySnapshot"),
    "We could not load your paper trading snapshot.",
  );

  return normalizePaperTradingSnapshot(result);
};

export const createPaperTradingAccount = async (
  input: CreatePaperTradingAccountInput,
): Promise<void> => {
  await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.post(
      "/api/services/app/PaperTrading/CreateMyAccount",
      buildPaperTradingAccountInput(input),
    ),
    "We could not create your paper trading account.",
  );
};

export const placePaperTradingOrder = async (
  input: PlacePaperOrderInput,
): Promise<PaperTradeExecutionResult> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.post(
      "/api/services/app/PaperTrading/PlaceMarketOrder",
      buildPlacePaperOrderInput(input),
    ),
    "We could not place the paper trade.",
  );

  return normalizePaperTradeExecutionResult(result);
};

export const closePaperTradingPosition = async (
  input: ClosePaperPositionInput,
): Promise<PaperOrder> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.post(
      "/api/services/app/PaperTrading/ClosePosition",
      buildClosePaperPositionInput(input),
    ),
    "We could not close the paper position.",
  );

  return normalizePaperOrder(result);
};

export const getPaperTradingOrders = async (): Promise<PaperOrder[]> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    apiClient.get("/api/services/app/PaperTrading/GetMyOrders"),
    "We could not load your paper orders.",
  );

  return Array.isArray(result?.items) ? result.items.map(normalizePaperOrder) : [];
};

export const getPaperTradingPositions = async (): Promise<PaperPosition[]> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    apiClient.get("/api/services/app/PaperTrading/GetMyPositions"),
    "We could not load your paper positions.",
  );

  return Array.isArray(result?.items) ? result.items.map(normalizePaperPosition) : [];
};

export const getPaperTradingFills = async (): Promise<PaperTradeFill[]> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    apiClient.get("/api/services/app/PaperTrading/GetMyFills"),
    "We could not load your paper fills.",
  );

  return Array.isArray(result?.items) ? result.items.map(normalizePaperTradeFill) : [];
};

export const getPaperTradeRecommendation = async (
  input: GetPaperTradeRecommendationInput,
): Promise<PaperTradeRecommendation> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.get(
      "/api/services/app/PaperTrading/GetRecommendation",
      {
        params: buildPaperTradeRecommendationInput(input),
      },
    ),
    "We could not load a paper-trading recommendation.",
  );

  return normalizePaperTradeRecommendation(result);
};
