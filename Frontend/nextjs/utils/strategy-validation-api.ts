import type {
  StrategyValidationResult,
  ValidateStrategyInput,
} from "@/types/strategy-validation";
import { getAxiosInstance } from "./axios-instance";

const toNumberOrNull = (value: unknown) =>
  typeof value === "number" ? value : null;

const toStringOrNull = (value: unknown) =>
  typeof value === "string" ? value : null;

const toStringList = (value: unknown) =>
  Array.isArray(value) ? value.map((item) => String(item)) : [];

const normalizeResult = (
  payload: Record<string, unknown>,
): StrategyValidationResult => {
  const strengths = payload.strengths ?? payload.Strengths;
  const risks = payload.risks ?? payload.Risks;
  const improvements = payload.improvements ?? payload.Improvements;

  return {
    id: Number(payload.id ?? payload.Id ?? 0),
    strategyName: toStringOrNull(payload.strategyName ?? payload.StrategyName),
    symbol: String(payload.symbol ?? payload.Symbol ?? "BTCUSDT"),
    timeframe: toStringOrNull(payload.timeframe ?? payload.Timeframe),
    directionPreference: toStringOrNull(
      payload.directionPreference ?? payload.DirectionPreference,
    ),
    strategyText: String(payload.strategyText ?? payload.StrategyText ?? ""),
    marketPrice: toNumberOrNull(payload.marketPrice ?? payload.MarketPrice),
    marketTrendScore: toNumberOrNull(
      payload.marketTrendScore ?? payload.MarketTrendScore,
    ),
    marketConfidenceScore: toNumberOrNull(
      payload.marketConfidenceScore ?? payload.MarketConfidenceScore,
    ),
    marketVerdict: toStringOrNull(payload.marketVerdict ?? payload.MarketVerdict),
    newsSummary: toStringOrNull(payload.newsSummary ?? payload.NewsSummary),
    validationScore: Number(payload.validationScore ?? payload.ValidationScore ?? 0),
    outcome: String(
      payload.outcome ?? payload.Outcome ?? "Caution",
    ) as StrategyValidationResult["outcome"],
    summary: String(payload.summary ?? payload.Summary ?? ""),
    strengths: toStringList(strengths),
    risks: toStringList(risks),
    improvements: toStringList(improvements),
    suggestedAction: toStringOrNull(
      payload.suggestedAction ?? payload.SuggestedAction,
    ),
    suggestedEntryPrice: toNumberOrNull(
      payload.suggestedEntryPrice ?? payload.SuggestedEntryPrice,
    ),
    suggestedStopLoss: toNumberOrNull(
      payload.suggestedStopLoss ?? payload.SuggestedStopLoss,
    ),
    suggestedTakeProfit: toNumberOrNull(
      payload.suggestedTakeProfit ?? payload.SuggestedTakeProfit,
    ),
    aiProvider: toStringOrNull(payload.aiProvider ?? payload.AiProvider),
    aiModel: toStringOrNull(payload.aiModel ?? payload.AiModel),
    creationTime: String(
      payload.creationTime ?? payload.CreationTime ?? new Date().toISOString(),
    ),
  };
};

export const validateMyStrategy = async (
  input: ValidateStrategyInput,
): Promise<StrategyValidationResult> => {
  const response = await getAxiosInstance().post(
    "/api/services/app/StrategyValidation/ValidateMyStrategy",
    input,
  );

  return normalizeResult(response.data.result ?? response.data);
};

export const getMyStrategyValidationHistory = async (): Promise<
  StrategyValidationResult[]
> => {
  const response = await getAxiosInstance().get(
    "/api/services/app/StrategyValidation/GetMyHistory",
  );

  const payload = response.data.result ?? response.data;
  const items = payload.items ?? payload.Items ?? [];

  return Array.isArray(items)
    ? items.map((item) => normalizeResult(item as Record<string, unknown>))
    : [];
};
