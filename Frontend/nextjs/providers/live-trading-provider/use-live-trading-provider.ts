"use client";

import { useCallback, useEffect, useMemo, useReducer, useRef } from "react";
import type {
  LiveTradeExecution,
  LiveTradingProviderActions,
} from "@/types/live-trading";
import {
  getMyLiveTrades,
  placeLiveOrder,
  syncExternalBrokerTrades,
} from "@/utils/live-trading-api";
import { liveTradingActions } from "./actions";
import { initialLiveTradingState, liveTradingReducer } from "./reducer";

const LIVE_TRADES_REFRESH_MS = 15_000;
const LIVE_TRADE_RECONCILE_DELAYS_MS = [500, 1_500, 4_000] as const;

type LiveTradingSuccessMode = "load" | "submit";
type TradeExecutedEventDetail = {
  Source?: string | null;
  source?: string | null;
};

const readEventString = (value: unknown) =>
  typeof value === "string" && value.trim().length > 0 ? value : null;

export const useLiveTradingProvider = () => {
  const [state, dispatch] = useReducer(liveTradingReducer, initialLiveTradingState);
  const latestTradesRequestIdRef = useRef(0);
  const scheduledRefreshTimeoutsRef = useRef<number[]>([]);

  const clearScheduledRefreshes = useCallback(() => {
    scheduledRefreshTimeoutsRef.current.forEach((timeoutId) => {
      window.clearTimeout(timeoutId);
    });

    scheduledRefreshTimeoutsRef.current = [];
  }, []);

  const fetchLatestTrades = useCallback(
    async ({
      mode = "load",
      markLoading = false,
      execution = null,
    }: {
      mode?: LiveTradingSuccessMode;
      markLoading?: boolean;
      execution?: LiveTradeExecution | null;
    } = {}) => {
      const requestId = ++latestTradesRequestIdRef.current;

      if (markLoading) {
        dispatch(liveTradingActions.loadStart());
      }

      try {
        await syncExternalBrokerTrades();
        const trades = await getMyLiveTrades();

        if (requestId !== latestTradesRequestIdRef.current) {
          return;
        }

        if (mode === "submit" && execution != null) {
          dispatch(liveTradingActions.submitSuccess(trades, execution));
          return;
        }

        dispatch(liveTradingActions.loadSuccess(trades));
      } catch (error) {
        if (requestId !== latestTradesRequestIdRef.current) {
          return;
        }

        dispatch(
          liveTradingActions.loadFailure(
            error instanceof Error
              ? error.message
              : "We could not refresh your live trades.",
          ),
        );
      }
    },
    [],
  );

  const refreshTrades = useCallback(async () => {
    await fetchLatestTrades({ mode: "load", markLoading: true });
  }, [fetchLatestTrades]);

  useEffect(() => {
    void refreshTrades();
  }, [refreshTrades]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshTrades();
    }, LIVE_TRADES_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshTrades]);

  useEffect(() => {
    const scheduleTradesReconcile = (delayMs: number) => {
      const timeoutId = window.setTimeout(() => {
        scheduledRefreshTimeoutsRef.current = scheduledRefreshTimeoutsRef.current.filter(
          (activeTimeoutId) => activeTimeoutId !== timeoutId,
        );
        void fetchLatestTrades();
      }, delayMs);

      scheduledRefreshTimeoutsRef.current.push(timeoutId);
    };

    const handleTradeExecuted = (event: Event) => {
      const detail = (event as CustomEvent<TradeExecutedEventDetail>).detail;
      const source = readEventString(detail?.Source ?? detail?.source)?.toLowerCase();
      const isPaperTrade = source?.includes("paper") ?? false;

      if (isPaperTrade) {
        return;
      }

      clearScheduledRefreshes();
      LIVE_TRADE_RECONCILE_DELAYS_MS.forEach(scheduleTradesReconcile);
    };

    window.addEventListener("fintex:trade-executed", handleTradeExecuted);

    return () => {
      clearScheduledRefreshes();
      window.removeEventListener("fintex:trade-executed", handleTradeExecuted);
    };
  }, [clearScheduledRefreshes, fetchLatestTrades]);

  const placeOrder = useCallback(async (input: Parameters<typeof placeLiveOrder>[0]) => {
    dispatch(liveTradingActions.submitStart());

    try {
      const execution = await placeLiveOrder(input);
      await fetchLatestTrades({ mode: "submit", execution });
      return execution;
    } catch (error) {
      dispatch(
        liveTradingActions.loadFailure(
          error instanceof Error
            ? error.message
            : "We could not place the live broker order.",
        ),
      );
      return null;
    }
  }, [fetchLatestTrades]);

  const actionValues = useMemo<LiveTradingProviderActions>(
    () => ({
      refreshTrades,
      placeOrder,
      clearError: () => dispatch(liveTradingActions.clearError()),
      clearExecution: () => dispatch(liveTradingActions.clearExecution()),
    }),
    [placeOrder, refreshTrades],
  );

  return { state, actionValues };
};
