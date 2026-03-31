import type {
  IndicatorScore,
  MarketDataPoint,
  MarketDataProvider,
  MarketPriceProjection,
  MarketProjectionMaturity,
  MarketTimeframeRsi,
  MarketVerdictState,
  MarketVerdictSnapshot,
} from "@/types/market-data";
import { getVerdictLabel } from "./formatters";

const getNumberOrNull = (value: unknown) =>
  typeof value === "number" ? value : value == null ? null : Number(value);

const getStringOrEmpty = (value: unknown) =>
  typeof value === "string" ? value : "";

const normalizeVerdictState = (value: unknown): MarketVerdictState => {
  if (value === 1 || value === "WarmingUp" || value === "warming_up") {
    return "warming_up";
  }

  if (value === 3 || value === "Degraded" || value === "degraded") {
    return "degraded";
  }

  if (value === 4 || value === "Stale" || value === "stale") {
    return "stale";
  }

  if (value === 5 || value === "Fallback" || value === "fallback") {
    return "fallback";
  }

  return "live";
};

const normalizeProjectionMaturity = (value: unknown): MarketProjectionMaturity => {
  if (value === 1 || value === "WarmingUp" || value === "warming_up") {
    return "warming_up";
  }

  if (value === 2 || value === "Forming" || value === "forming") {
    return "forming";
  }

  return "mature";
};

export const normalizeMarketDataPoint = (
  value: Record<string, unknown>,
): MarketDataPoint => ({
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

const normalizeMarketPriceProjection = (
  value: Record<string, unknown>,
): MarketPriceProjection => ({
  horizon: getStringOrEmpty(value.horizon ?? value.Horizon),
  minutesAhead: Number(value.minutesAhead ?? value.MinutesAhead ?? 0),
  targetTimestamp: getStringOrEmpty(value.targetTimestamp ?? value.TargetTimestamp),
  modelName: getStringOrEmpty(value.modelName ?? value.ModelName),
  consensusPrice: getNumberOrNull(value.consensusPrice ?? value.ConsensusPrice),
  smaPrice: getNumberOrNull(value.smaPrice ?? value.SmaPrice),
  emaPrice: getNumberOrNull(value.emaPrice ?? value.EmaPrice),
  smmaPrice: getNumberOrNull(value.smmaPrice ?? value.SmmaPrice),
  confidenceScore: getNumberOrNull(value.confidenceScore ?? value.ConfidenceScore),
  maturity: normalizeProjectionMaturity(value.maturity ?? value.Maturity),
  barsUsed: Number(value.barsUsed ?? value.BarsUsed ?? 0),
  effectivePeriod: Number(value.effectivePeriod ?? value.EffectivePeriod ?? 0),
});

export const normalizeTimeframeRsi = (
  item: Record<string, unknown>,
): MarketTimeframeRsi => ({
  timeframe: getStringOrEmpty(item.timeframe ?? item.Timeframe),
  value:
    item.value == null && item.Value == null ? null : Number(item.value ?? item.Value),
  candleTimestamp:
    item.candleTimestamp == null && item.CandleTimestamp == null
      ? null
      : String(item.candleTimestamp ?? item.CandleTimestamp),
});

export const normalizeMarketVerdict = (
  value: Record<string, unknown>,
): MarketVerdictSnapshot => ({
  marketDataPointId: Number(value.marketDataPointId ?? value.MarketDataPointId ?? 0),
  symbol: getStringOrEmpty(value.symbol ?? value.Symbol),
  provider: Number(value.provider ?? value.Provider ?? 1) as MarketDataProvider,
  price: Number(value.price ?? value.Price ?? 0),
  trendScore: getNumberOrNull(value.trendScore ?? value.TrendScore),
  confidenceScore: getNumberOrNull(value.confidenceScore ?? value.ConfidenceScore),
  verdict: getVerdictLabel(value.verdict ?? value.Verdict),
  verdictState: normalizeVerdictState(value.verdictState ?? value.VerdictState),
  verdictStateReason: getStringOrEmpty(value.verdictStateReason ?? value.VerdictStateReason),
  timestamp: getStringOrEmpty(value.timestamp ?? value.Timestamp),
  evaluatedAtUtc: getStringOrEmpty(value.evaluatedAtUtc ?? value.EvaluatedAtUtc),
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
  nextOneMinuteProjection:
    value.nextOneMinuteProjection && typeof value.nextOneMinuteProjection === "object"
      ? normalizeMarketPriceProjection(value.nextOneMinuteProjection as Record<string, unknown>)
      : value.NextOneMinuteProjection && typeof value.NextOneMinuteProjection === "object"
        ? normalizeMarketPriceProjection(value.NextOneMinuteProjection as Record<string, unknown>)
        : null,
  nextFiveMinuteProjection:
    value.nextFiveMinuteProjection && typeof value.nextFiveMinuteProjection === "object"
      ? normalizeMarketPriceProjection(value.nextFiveMinuteProjection as Record<string, unknown>)
      : value.NextFiveMinuteProjection && typeof value.NextFiveMinuteProjection === "object"
        ? normalizeMarketPriceProjection(value.NextFiveMinuteProjection as Record<string, unknown>)
        : null,
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
