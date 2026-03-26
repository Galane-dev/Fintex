export interface UserProfile {
  id: number;
  userId: number;
  preferredBaseCurrency: string;
  favoriteSymbols: string;
  riskTolerance: number;
  isAiInsightsEnabled: boolean;
  behavioralRiskScore: number;
  behavioralSummary: string;
  strategyNotes: string;
  lastAiProvider: string;
  lastAiModel: string;
  lastBehavioralAnalysisTime: string | null;
}
