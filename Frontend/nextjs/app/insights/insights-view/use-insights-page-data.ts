"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useLiveTrading } from "@/hooks/useLiveTrading";
import { useMarketData } from "@/hooks/useMarketData";
import { useNotifications } from "@/hooks/useNotifications";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import type { StrategyValidationResult } from "@/types/strategy-validation";
import type { UserProfile } from "@/types/user-profile";
import { isLiveTradeClosed, isLiveTradeOpen } from "@/utils/live-trading";
import { getMyStrategyValidationHistory } from "@/utils/strategy-validation-api";
import { getMyUserProfile } from "@/utils/user-profile-api";
import { buildInsightsDataset } from "./insights-metrics";

export const useInsightsPageData = () => {
  const marketData = useMarketData();
  const paperTrading = usePaperTrading();
  const liveTrading = useLiveTrading();
  const notifications = useNotifications();
  const { refreshSnapshot: refreshMarketSnapshot, latest, verdict, connectionStatus } = marketData;
  const { refreshSnapshot: refreshPaperSnapshot, snapshot } = paperTrading;
  const { refreshTrades, trades } = liveTrading;
  const { refreshInbox, notifications: inboxNotifications } = notifications;
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [strategyHistory, setStrategyHistory] = useState<StrategyValidationResult[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const [nextProfile, nextStrategyHistory] = await Promise.all([
        getMyUserProfile(),
        getMyStrategyValidationHistory(),
        refreshPaperSnapshot(),
        refreshTrades(),
        refreshInbox(),
        refreshMarketSnapshot(),
      ]);

      setProfile(nextProfile);
      setStrategyHistory(nextStrategyHistory);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "We could not load your insights right now.",
      );
    } finally {
      setIsLoading(false);
    }
  }, [refreshInbox, refreshMarketSnapshot, refreshPaperSnapshot, refreshTrades]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const dataset = useMemo(
    () =>
      buildInsightsDataset({
        profile,
        openPaperPositions: snapshot?.positions ?? [],
        closedPaperFills: snapshot?.recentFills ?? [],
        openLiveTrades: trades.filter((trade) => isLiveTradeOpen(trade)),
        closedLiveTrades: trades.filter((trade) => isLiveTradeClosed(trade)),
        strategyHistory,
        notifications: inboxNotifications,
      }),
    [inboxNotifications, profile, snapshot, strategyHistory, trades],
  );

  return {
    dataset,
    latestMarketPrice: latest?.price ?? null,
    latestVerdict: verdict?.verdict ?? latest?.verdict ?? "Hold",
    marketConfidence: verdict?.confidenceScore ?? latest?.confidenceScore ?? null,
    connectionStatus,
    isLoading,
    error,
    refresh,
  };
};
