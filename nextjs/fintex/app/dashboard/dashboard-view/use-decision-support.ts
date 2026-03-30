"use client";

import { useMemo } from "react";
import type {
  MarketConnectionStatus,
  MarketSelection,
  MarketTimeframeRsi,
  MarketVerdictSnapshot,
} from "@/types/market-data";

interface UseDecisionSupportInput {
  selection: MarketSelection;
  streamedVerdict: MarketVerdictSnapshot | null;
  streamedTimeframeRsi: MarketTimeframeRsi[];
  connectionStatus: MarketConnectionStatus;
}

const hasAuthoritativeVerdict = (
  verdict: MarketVerdictSnapshot | null,
): verdict is MarketVerdictSnapshot =>
  verdict != null &&
  verdict.verdictState !== "fallback" &&
  verdict.confidenceScore != null &&
  verdict.trendScore != null &&
  verdict.adx != null &&
  verdict.nextOneMinuteProjection != null &&
  verdict.nextFiveMinuteProjection != null;

export const useDecisionSupport = ({
  selection,
  streamedVerdict,
  streamedTimeframeRsi,
  connectionStatus,
}: UseDecisionSupportInput) => {
  return useMemo(() => {
    const verdict = hasAuthoritativeVerdict(streamedVerdict)
      ? streamedVerdict
      : null;
    const timeframeRsi = verdict ? streamedTimeframeRsi : [];
    const isLoading = verdict == null;

    let error: string | null = null;
    if (connectionStatus === "error") {
      error = `Decision support is waiting for the ${selection.symbol} verdict stream to recover.`;
    } else if (connectionStatus === "disconnected") {
      error = `Decision support is waiting for the ${selection.symbol} market stream to reconnect.`;
    }

    return {
      verdict,
      timeframeRsi,
      error,
      isLoading,
      connectionStatus,
    };
  }, [connectionStatus, selection.symbol, streamedTimeframeRsi, streamedVerdict]);
};
