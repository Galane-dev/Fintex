import type { UserProfile } from "@/types/user-profile";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

type ParsedBehaviorInsight = {
  riskScore: number | null;
  summary: string | null;
  wasStructured: boolean;
};

const normalizeStructuredSummary = (value: string) => {
  const trimmed = value.trim();

  if (!trimmed.startsWith("```")) {
    return trimmed;
  }

  const lines = trimmed.split("\n");
  if (lines.length <= 2) {
    return trimmed;
  }

  return lines.slice(1, -1).join("\n").trim();
};

const parseBehaviorInsight = (value: string): ParsedBehaviorInsight => {
  if (!value.trim()) {
    return { riskScore: null, summary: null, wasStructured: false };
  }

  const normalized = normalizeStructuredSummary(value);
  const candidates = [normalized];
  const jsonStart = normalized.indexOf("{");
  const jsonEnd = normalized.lastIndexOf("}");

  if (jsonStart >= 0 && jsonEnd > jsonStart) {
    candidates.push(normalized.slice(jsonStart, jsonEnd + 1));
  }

  for (const candidate of candidates) {
    try {
      const parsed = JSON.parse(candidate) as {
        riskScore?: unknown;
        summary?: unknown;
      };
      const riskScore =
        typeof parsed.riskScore === "number"
          ? parsed.riskScore
          : parsed.riskScore == null
            ? null
            : Number(parsed.riskScore);
      const summary =
        typeof parsed.summary === "string" ? parsed.summary.trim() : null;

      return {
        riskScore: Number.isFinite(riskScore ?? Number.NaN) ? riskScore : null,
        summary,
        wasStructured: true,
      };
    } catch {
      continue;
    }
  }

  const riskMatch = normalized.match(/"riskScore"\s*:\s*(-?\d+(?:\.\d+)?)/i);
  const summaryMatch = normalized.match(/"summary"\s*:\s*"((?:\\.|[\s\S])*?)(?:"|$)/i);

  return {
    riskScore: riskMatch ? Number(riskMatch[1]) : null,
    summary: summaryMatch ? summaryMatch[1].replace(/\\"/g, "\"").trim() : null,
    wasStructured:
      normalized.startsWith("{") ||
      normalized.startsWith("```") ||
      normalized.includes("\"riskScore\"") ||
      normalized.includes("\"summary\""),
  };
};

export const normalizeUserProfile = (
  value: Record<string, unknown>,
): UserProfile => {
  const rawBehavioralSummary = getString(
    value.behavioralSummary ?? value.BehavioralSummary,
  );
  const parsedBehaviorInsight = parseBehaviorInsight(rawBehavioralSummary);
  const behavioralRiskScore = getNumber(
    value.behavioralRiskScore ?? value.BehavioralRiskScore,
  );

  return {
    id: getNumber(value.id ?? value.Id),
    userId: getNumber(value.userId ?? value.UserId),
    preferredBaseCurrency: getString(
      value.preferredBaseCurrency ?? value.PreferredBaseCurrency,
    ),
    favoriteSymbols: getString(value.favoriteSymbols ?? value.FavoriteSymbols),
    riskTolerance: getNumber(value.riskTolerance ?? value.RiskTolerance),
    isAiInsightsEnabled: Boolean(value.isAiInsightsEnabled ?? value.IsAiInsightsEnabled),
    behavioralRiskScore:
      parsedBehaviorInsight.wasStructured && parsedBehaviorInsight.riskScore != null
        ? parsedBehaviorInsight.riskScore
        : behavioralRiskScore,
    behavioralSummary:
      parsedBehaviorInsight.wasStructured && parsedBehaviorInsight.summary
        ? parsedBehaviorInsight.summary
        : rawBehavioralSummary,
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
  };
};
