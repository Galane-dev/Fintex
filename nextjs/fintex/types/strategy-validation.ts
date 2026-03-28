export type StrategyValidationOutcome = "Fail" | "Caution" | "Validated";

export type ValidateStrategyInput = {
  strategyName?: string;
  symbol: string;
  provider: number;
  timeframe?: string;
  directionPreference?: string;
  strategyText: string;
};

export type StrategyValidationResult = {
  id: number;
  strategyName: string | null;
  symbol: string;
  timeframe: string | null;
  directionPreference: string | null;
  strategyText: string;
  marketPrice: number | null;
  marketTrendScore: number | null;
  marketConfidenceScore: number | null;
  marketVerdict: string | null;
  newsSummary: string | null;
  validationScore: number;
  outcome: StrategyValidationOutcome;
  summary: string;
  strengths: string[];
  risks: string[];
  improvements: string[];
  suggestedAction: string | null;
  suggestedEntryPrice: number | null;
  suggestedStopLoss: number | null;
  suggestedTakeProfit: number | null;
  aiProvider: string | null;
  aiModel: string | null;
  creationTime: string;
};
