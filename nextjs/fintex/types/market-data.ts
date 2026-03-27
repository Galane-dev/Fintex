export type MarketDataProvider = 1 | 2 | 3;
export type AssetClass = 1 | 2;
export type MarketVerdict = "Hold" | "Buy" | "Sell";
export type MarketConnectionStatus =
  | "idle"
  | "connecting"
  | "connected"
  | "reconnecting"
  | "disconnected"
  | "error";

export interface MarketSelection {
  key: string;
  label: string;
  symbol: string;
  provider: MarketDataProvider;
  venue: string;
}

export interface MarketDataPoint {
  id: number;
  symbol: string;
  provider: MarketDataProvider;
  assetClass: AssetClass | null;
  price: number;
  bid: number | null;
  ask: number | null;
  volume: number | null;
  timestamp: string;
  sma: number | null;
  ema: number | null;
  rsi: number | null;
  stdDev: number | null;
  macd: number | null;
  macdSignal: number | null;
  macdHistogram: number | null;
  momentum: number | null;
  rateOfChange: number | null;
  bollingerUpper: number | null;
  bollingerLower: number | null;
  trendScore: number | null;
  confidenceScore: number | null;
  verdict: MarketVerdict;
}

export interface IndicatorScore {
  name: string;
  value: number;
  score: number;
  signal: string;
}

export interface MarketVerdictTimeframeSignal {
  timeframe: string;
  biasScore: number | null;
  signal: string;
}

export interface MarketPriceProjection {
  horizon: string;
  minutesAhead: number;
  targetTimestamp: string;
  consensusPrice: number | null;
  smaPrice: number | null;
  emaPrice: number | null;
  smmaPrice: number | null;
}

export interface MarketTimeframeRsi {
  timeframe: string;
  value: number | null;
  candleTimestamp: string | null;
}

export interface MarketVerdictSnapshot {
  marketDataPointId: number;
  symbol: string;
  provider: MarketDataProvider;
  price: number;
  trendScore: number | null;
  confidenceScore: number | null;
  verdict: MarketVerdict;
  timestamp: string;
  sma: number | null;
  ema: number | null;
  rsi: number | null;
  macd: number | null;
  macdSignal: number | null;
  macdHistogram: number | null;
  momentum: number | null;
  rateOfChange: number | null;
  atr: number | null;
  atrPercent: number | null;
  adx: number | null;
  structureScore: number | null;
  structureLabel: string;
  timeframeAlignmentScore: number | null;
  nextOneMinuteProjection: MarketPriceProjection | null;
  nextFiveMinuteProjection: MarketPriceProjection | null;
  indicatorScores: IndicatorScore[];
  timeframeSignals: MarketVerdictTimeframeSignal[];
}

export interface MarketInsight {
  title: string;
  tag: string;
  tone: "green" | "gold" | "blue" | "red";
  copy: string;
}

export interface MarketDataState {
  isLoading: boolean;
  error: string | null;
  selection: MarketSelection;
  latest: MarketDataPoint | null;
  history: MarketDataPoint[];
  verdict: MarketVerdictSnapshot | null;
  timeframeRsi: MarketTimeframeRsi[];
  connectionStatus: MarketConnectionStatus;
  lastHydratedAt: string | null;
}

export interface MarketDataProviderActions {
  selectMarket: (marketKey: string) => void;
  refreshSnapshot: () => Promise<void>;
}
