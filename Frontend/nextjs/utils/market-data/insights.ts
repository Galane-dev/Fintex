import type { MarketInsight, MarketVerdictSnapshot } from "@/types/market-data";
import { formatSignedPoints } from "./formatters";

export const buildMarketInsights = (
  verdict: MarketVerdictSnapshot | null,
): MarketInsight[] => {
  if (!verdict) {
    return [
      {
        title: "Snapshot pending",
        tag: "Waiting",
        tone: "blue",
        copy: "Fintex is waiting for the first enriched verdict snapshot before it scores structure, confidence, and forward estimates.",
      },
    ];
  }

  const effectiveVerdict = verdict.verdict;
  const effectiveRsi = verdict.rsi;
  const effectiveMomentum = verdict.momentum;
  const effectiveMacd = verdict.macd;
  const effectiveMacdSignal = verdict.macdSignal;
  const effectiveAtrPercent = verdict.atrPercent;
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
        verdict.confidenceScore != null
          ? `${Math.round(verdict.confidenceScore)} / 100`
          : "Pending",
      tone: "blue",
      copy:
        verdict.trendScore != null
          ? `Trend score sits at ${Math.round(verdict.trendScore)}, which helps summarize the current market-only posture without leaning on user behavior or external signals.`
          : "Trend and confidence are loading from the latest backend snapshot.",
    },
  ];
};
