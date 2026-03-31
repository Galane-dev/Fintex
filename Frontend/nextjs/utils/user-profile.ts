import type { UserProfile } from "@/types/user-profile";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

export const normalizeUserProfile = (
  value: Record<string, unknown>,
): UserProfile => ({
  id: getNumber(value.id ?? value.Id),
  userId: getNumber(value.userId ?? value.UserId),
  preferredBaseCurrency: getString(
    value.preferredBaseCurrency ?? value.PreferredBaseCurrency,
  ),
  favoriteSymbols: getString(value.favoriteSymbols ?? value.FavoriteSymbols),
  riskTolerance: getNumber(value.riskTolerance ?? value.RiskTolerance),
  isAiInsightsEnabled: Boolean(value.isAiInsightsEnabled ?? value.IsAiInsightsEnabled),
  behavioralRiskScore: getNumber(
    value.behavioralRiskScore ?? value.BehavioralRiskScore,
  ),
  behavioralSummary: getString(
    value.behavioralSummary ?? value.BehavioralSummary,
  ),
  strategyNotes: getString(value.strategyNotes ?? value.StrategyNotes),
  lastAiProvider: getString(value.lastAiProvider ?? value.LastAiProvider),
  lastAiModel: getString(value.lastAiModel ?? value.LastAiModel),
  lastBehavioralAnalysisTime:
    value.lastBehavioralAnalysisTime == null &&
    value.LastBehavioralAnalysisTime == null
      ? null
      : getString(
          value.lastBehavioralAnalysisTime ?? value.LastBehavioralAnalysisTime,
        ),
});
