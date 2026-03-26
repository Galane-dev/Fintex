import type {
  ClosePaperPositionInput,
  CreatePaperTradingAccountInput,
  PaperOrder,
  PaperPosition,
  PaperTradeFill,
  PaperTradingSnapshot,
  PlacePaperOrderInput,
} from "@/types/paper-trading";
import { apiClient } from "./api-client";
import { unwrapAbpResponse } from "./abp-response";
import {
  buildClosePaperPositionInput,
  buildPaperTradingAccountInput,
  buildPlacePaperOrderInput,
  normalizePaperOrder,
  normalizePaperPosition,
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
): Promise<PaperOrder> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.post(
      "/api/services/app/PaperTrading/PlaceMarketOrder",
      buildPlacePaperOrderInput(input),
    ),
    "We could not place the paper trade.",
  );

  return normalizePaperOrder(result);
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
