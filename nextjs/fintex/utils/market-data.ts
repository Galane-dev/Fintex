import { MARKET_PROVIDER_LABELS } from "@/constants/market-data";
import type {
  IndicatorScore,
  MarketConnectionStatus,
  MarketDataPoint,
  MarketDataProvider,
  MarketInsight,
  MarketTimeframeRsi,
  MarketVerdict,
  MarketVerdictSnapshot,
} from "@/types/market-data";

const EMPTY = "—";

const getNumberOrNull = (value: unknown) =>
  typeof value === "number" ? value : value == null ? null : Number(value);

const getStringOrEmpty = (value: unknown) => (typeof value === "string" ? value : "");

export const getVerdictLabel = (value: unknown): MarketVerdict => {
  if (value === 2 || value === "Buy") {
    return "Buy";
  }

  if (value === 3 || value === "Sell") {
    return "Sell";
  }

  return "Hold";
};

export const getProviderLabel = (provider: MarketDataProvider) =>
  MARKET_PROVIDER_LABELS[provider] ?? "Provider";

export const normalizeMarketDataPoint = (value: Record<string, unknown>): MarketDataPoint => ({
  id: Number(value.id ?? value.Id ?? value.marketDataPointId ?? value.MarketDataPointId ?? 0),
  symbol: getStringOrEmpty(value.symbol ?? value.Symbol),
  provider: Number(value.provider ?? value.Provider ?? 1) as MarketDataProvider,
  assetClass:
    value.assetClass == null && value.AssetClass == null
      ? null
      : (Number(value.assetClass ?? value.AssetClass) as 1 | 2),
  price: Number(value.price ?? value.Price ?? 0),
  bid: getNumberOrNull(value.bid ?? value.Bid),
  ask: getNumberOrNull(value.ask ?? value.Ask),
  volume: getNumberOrNull(value.volume ?? value.Volume),
  timestamp: getStringOrEmpty(value.timestamp ?? value.Timestamp),
  sma: getNumberOrNull(value.sma ?? value.Sma),
  ema: getNumberOrNull(value.ema ?? value.Ema),
  rsi: getNumberOrNull(value.rsi ?? value.Rsi),
  stdDev: getNumberOrNull(value.stdDev ?? value.StdDev),
  macd: getNumberOrNull(value.macd ?? value.Macd),
  macdSignal: getNumberOrNull(value.macdSignal ?? value.MacdSignal),
  macdHistogram: getNumberOrNull(value.macdHistogram ?? value.MacdHistogram),
  momentum: getNumberOrNull(value.momentum ?? value.Momentum),
  rateOfChange: getNumberOrNull(value.rateOfChange ?? value.RateOfChange),
  bollingerUpper: getNumberOrNull(value.bollingerUpper ?? value.BollingerUpper),
  bollingerLower: getNumberOrNull(value.bollingerLower ?? value.BollingerLower),
  trendScore: getNumberOrNull(value.trendScore ?? value.TrendScore),
  confidenceScore: getNumberOrNull(value.confidenceScore ?? value.ConfidenceScore),
  verdict: getVerdictLabel(value.verdict ?? value.Verdict),
});

const normalizeIndicatorScore = (value: Record<string, unknown>): IndicatorScore => ({
  name: getStringOrEmpty(value.name ?? value.Name),
  value: Number(value.value ?? value.Value ?? 0),
  score: Number(value.score ?? value.Score ?? 0),
  signal: getStringOrEmpty(value.signal ?? value.Signal),
});

export const normalizeTimeframeRsi = (item: Record<string, unknown>): MarketTimeframeRsi => ({
  timeframe: getStringOrEmpty(item.timeframe ?? item.Timeframe),
  value:
    item.value == null && item.Value == null
      ? null
      : Number(item.value ?? item.Value),
  candleTimestamp:
    item.candleTimestamp == null && item.CandleTimestamp == null
      ? null
      : String(item.candleTimestamp ?? item.CandleTimestamp),
});

export const normalizeMarketVerdict = (value: Record<string, unknown>): MarketVerdictSnapshot => ({
  marketDataPointId: Number(value.marketDataPointId ?? value.MarketDataPointId ?? 0),
  symbol: getStringOrEmpty(value.symbol ?? value.Symbol),
  provider: Number(value.provider ?? value.Provider ?? 1) as MarketDataProvider,
  price: Number(value.price ?? value.Price ?? 0),
  trendScore: getNumberOrNull(value.trendScore ?? value.TrendScore),
  confidenceScore: getNumberOrNull(value.confidenceScore ?? value.ConfidenceScore),
  verdict: getVerdictLabel(value.verdict ?? value.Verdict),
  timestamp: getStringOrEmpty(value.timestamp ?? value.Timestamp),
  sma: getNumberOrNull(value.sma ?? value.Sma),
  ema: getNumberOrNull(value.ema ?? value.Ema),
  rsi: getNumberOrNull(value.rsi ?? value.Rsi),
  macd: getNumberOrNull(value.macd ?? value.Macd),
  macdSignal: getNumberOrNull(value.macdSignal ?? value.MacdSignal),
  macdHistogram: getNumberOrNull(value.macdHistogram ?? value.MacdHistogram),
  momentum: getNumberOrNull(value.momentum ?? value.Momentum),
  rateOfChange: getNumberOrNull(value.rateOfChange ?? value.RateOfChange),
  atr: getNumberOrNull(value.atr ?? value.Atr),
  atrPercent: getNumberOrNull(value.atrPercent ?? value.AtrPercent),
  adx: getNumberOrNull(value.adx ?? value.Adx),
  structureScore: getNumberOrNull(value.structureScore ?? value.StructureScore),
  structureLabel: getStringOrEmpty(value.structureLabel ?? value.StructureLabel),
  timeframeAlignmentScore: getNumberOrNull(
    value.timeframeAlignmentScore ?? value.TimeframeAlignmentScore,
  ),
  indicatorScores: Array.isArray(value.indicatorScores ?? value.IndicatorScores)
    ? ((value.indicatorScores ?? value.IndicatorScores) as Record<string, unknown>[]).map(
        normalizeIndicatorScore,
      )
    : [],
  timeframeSignals: Array.isArray(value.timeframeSignals ?? value.TimeframeSignals)
    ? ((value.timeframeSignals ?? value.TimeframeSignals) as Record<string, unknown>[]).map(
        (item) => ({
          timeframe: getStringOrEmpty(item.timeframe ?? item.Timeframe),
          biasScore: getNumberOrNull(item.biasScore ?? item.BiasScore),
          signal: getStringOrEmpty(item.signal ?? item.Signal),
        }),
      )
    : [],
});

export const sortHistoryAscending = (history: MarketDataPoint[]) =>
  [...history].sort((left, right) => new Date(left.timestamp).getTime() - new Date(right.timestamp).getTime());

export const upsertHistoryPoint = (history: MarketDataPoint[], point: MarketDataPoint, maxPoints = 120) => {
  const nextHistory = history.some((item) => item.id === point.id)
    ? history.map((item) => (item.id === point.id ? point : item))
    : [...history, point];

  return sortHistoryAscending(nextHistory).slice(-maxPoints);
};

export const formatPrice = (value: number | null | undefined) =>
  value == null
    ? EMPTY
    : value.toLocaleString(undefined, {
        minimumFractionDigits: value > 100 ? 2 : 4,
        maximumFractionDigits: value > 100 ? 2 : 4,
      });

export const formatCompact = (value: number | null | undefined) =>
  value == null
    ? EMPTY
    : new Intl.NumberFormat(undefined, {
        notation: "compact",
        maximumFractionDigits: 1,
      }).format(value);

export const formatSigned = (value: number | null | undefined, digits = 2) =>
  value == null ? EMPTY : `${value >= 0 ? "+" : ""}${value.toFixed(digits)}`;

export const formatPercent = (value: number | null | undefined, digits = 2) =>
  value == null ? EMPTY : `${value >= 0 ? "+" : ""}${value.toFixed(digits)}%`;

export const formatSignedPoints = (value: number | null | undefined, digits = 2) =>
  value == null ? EMPTY : `${value >= 0 ? "+" : ""}${value.toFixed(digits)}`;

export const formatTime = (value: string | null | undefined) =>
  value
    ? new Intl.DateTimeFormat(undefined, {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
      }).format(new Date(value))
    : EMPTY;

export const getConnectionTone = (status: MarketConnectionStatus) => {
  switch (status) {
    case "connected":
      return "green";
    case "reconnecting":
      return "gold";
    case "error":
      return "red";
    default:
      return "default";
  }
};

export const buildMarketInsights = (
  latest: MarketDataPoint | null,
  verdict: MarketVerdictSnapshot | null,
): MarketInsight[] => {
  if (!latest) {
    return [
      {
        title: "Snapshot pending",
        tag: "Waiting",
        tone: "blue",
        copy: "We are waiting for the first live market snapshot before calculating structure, momentum, and conviction.",
      },
    ];
  }

  const effectiveVerdict = verdict?.verdict ?? latest.verdict;
  const effectiveRsi = verdict?.rsi ?? latest.rsi;
  const effectiveMomentum = verdict?.momentum ?? latest.momentum;
  const effectiveMacd = verdict?.macd ?? latest.macd;
  const effectiveMacdSignal = verdict?.macdSignal ?? latest.macdSignal;
  const effectiveAtrPercent = verdict?.atrPercent;
  const isBullish = effectiveVerdict === "Buy";

  const rsiState =
    effectiveRsi == null
      ? "RSI is still loading from the feed."
      : effectiveRsi >= 70
        ? "RSI is elevated, so upside is strong but increasingly stretched."
        : effectiveRsi <= 35
          ? "RSI is compressed, which keeps mean-reversion risk in focus."
          : "RSI remains balanced enough to support continuation without flashing exhaustion.";

  const macdState =
    effectiveMacd == null || effectiveMacdSignal == null
      ? "MACD is still loading."
      : effectiveMacd >= effectiveMacdSignal
        ? "MACD remains above its signal line, so momentum structure is constructive."
        : "MACD is below its signal line, which weakens continuation quality.";

  const volatilityState =
    effectiveAtrPercent == null
      ? "Volatility is still loading."
      : effectiveAtrPercent >= 0.65
        ? "ATR volatility is elevated relative to price, so entries need tighter confirmation."
        : "ATR volatility is controlled enough for cleaner directional reads.";

  return [
    {
      title: "Realtime verdict",
      tag: `${effectiveVerdict} bias`,
      tone: isBullish ? "green" : effectiveVerdict === "Sell" ? "red" : "blue",
      copy: `${macdState} ${volatilityState}`,
    },
    {
      title: "Momentum read",
      tag: effectiveMomentum != null ? formatSignedPoints(effectiveMomentum) : "Loading",
      tone: effectiveMomentum != null && effectiveMomentum >= 0 ? "green" : "gold",
      copy: rsiState,
    },
    {
      title: "Confidence context",
      tag:
        verdict?.confidenceScore != null
          ? `${Math.round(verdict.confidenceScore)} / 100`
          : "Pending",
      tone: "blue",
      copy:
        verdict?.trendScore != null
          ? `Trend score sits at ${Math.round(verdict.trendScore)}, which helps summarize the current market-only posture without leaning on user behavior or external signals.`
          : "Trend and confidence are loading from the latest backend snapshot.",
    },
  ];
};
