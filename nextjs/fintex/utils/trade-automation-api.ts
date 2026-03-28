import type { CreateTradeAutomationRuleInput, TradeAutomationRule } from "@/types/trade-automation";
import { getAxiosInstance } from "./axios-instance";
import { normalizeTradeAutomationRule } from "./trade-automation";

export const getMyTradeAutomationRules = async (): Promise<TradeAutomationRule[]> => {
  const response = await getAxiosInstance().get("/api/services/app/TradeAutomation/GetMyRules");
  const payload = response.data.result ?? response.data;
  const items = Array.isArray(payload?.items)
    ? (payload.items as unknown[])
    : Array.isArray(payload)
      ? (payload as unknown[])
      : [];
  return items.map((item: unknown) => normalizeTradeAutomationRule(item as Record<string, unknown>));
};

export const createTradeAutomationRule = async (input: CreateTradeAutomationRuleInput) => {
  await getAxiosInstance().post("/api/services/app/TradeAutomation/CreateRule", input);
};

export const deleteTradeAutomationRule = async (ruleId: number) => {
  await getAxiosInstance().post("/api/services/app/TradeAutomation/DeleteRule", { id: ruleId });
};
