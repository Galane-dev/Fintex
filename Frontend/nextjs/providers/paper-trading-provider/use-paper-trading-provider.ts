"use client";

import { useCallback, useEffect, useMemo, useReducer, useRef } from "react";
import type { PaperTradingProviderActions } from "@/types/paper-trading";
import {
  closePaperTradingPosition,
  createPaperTradingAccount,
  getPaperTradeRecommendation,
  getPaperTradingSnapshot,
  placePaperTradingOrder,
} from "@/utils/paper-trading-api";
import { paperTradingActions } from "./actions";
import { initialPaperTradingState, paperTradingReducer } from "./reducer";

const SNAPSHOT_REFRESH_MS = 15_000;
const PAPER_TRADE_RECONCILE_DELAYS_MS = [500, 1_500, 4_000] as const;
const EMPTY_SNAPSHOT = {
  account: null,
  positions: [],
  recentOrders: [],
  recentFills: [],
};

type SnapshotSuccessMode = "load" | "submit";
type TradeExecutedEventDetail = {
  PositionId?: number | string | null;
  positionId?: number | string | null;
  Source?: string | null;
  source?: string | null;
  Status?: string | null;
  status?: string | null;
};

const isMissingAccountMessage = (message: string) =>
  message.toLowerCase().includes("create a paper trading account");

const readEventString = (value: unknown) =>
  typeof value === "string" && value.trim().length > 0 ? value : null;

const readEventNumber = (value: unknown) => {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string" && value.trim().length > 0) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  return null;
};

export const usePaperTradingProvider = () => {
  const [state, dispatch] = useReducer(paperTradingReducer, initialPaperTradingState);
  const latestSnapshotRequestIdRef = useRef(0);
  const scheduledRefreshTimeoutsRef = useRef<number[]>([]);

  const clearScheduledRefreshes = useCallback(() => {
    scheduledRefreshTimeoutsRef.current.forEach((timeoutId) => {
      window.clearTimeout(timeoutId);
    });

    scheduledRefreshTimeoutsRef.current = [];
  }, []);

  const fetchLatestSnapshot = useCallback(
    async ({
      mode = "load",
      markLoading = false,
    }: {
      mode?: SnapshotSuccessMode;
      markLoading?: boolean;
    } = {}) => {
      const requestId = ++latestSnapshotRequestIdRef.current;

      if (markLoading) {
        dispatch(paperTradingActions.loadStart());
      }

      try {
        const snapshot = await getPaperTradingSnapshot();

        if (requestId !== latestSnapshotRequestIdRef.current) {
          return;
        }

        dispatch(
          mode === "submit"
            ? paperTradingActions.submitSuccess(snapshot)
            : paperTradingActions.loadSuccess(snapshot),
        );
      } catch (error) {
        if (requestId !== latestSnapshotRequestIdRef.current) {
          return;
        }

        const message =
          error instanceof Error
            ? error.message
            : "We could not refresh the paper trading snapshot.";

        if (isMissingAccountMessage(message)) {
          dispatch(
            mode === "submit"
              ? paperTradingActions.submitSuccess(EMPTY_SNAPSHOT)
              : paperTradingActions.loadSuccess(EMPTY_SNAPSHOT),
          );
          return;
        }

        dispatch(paperTradingActions.loadFailure(message));
      }
    },
    [],
  );

  const refreshSnapshot = useCallback(async () => {
    await fetchLatestSnapshot({ mode: "load", markLoading: true });
  }, [fetchLatestSnapshot]);

  useEffect(() => {
    void refreshSnapshot();
  }, [refreshSnapshot]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshSnapshot();
    }, SNAPSHOT_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshSnapshot]);

  useEffect(() => {
    const scheduleSnapshotReconcile = (delayMs: number) => {
      const timeoutId = window.setTimeout(() => {
        scheduledRefreshTimeoutsRef.current = scheduledRefreshTimeoutsRef.current.filter(
          (activeTimeoutId) => activeTimeoutId !== timeoutId,
        );
        void fetchLatestSnapshot();
      }, delayMs);

      scheduledRefreshTimeoutsRef.current.push(timeoutId);
    };

    const handleTradeExecuted = (event: Event) => {
      const detail = (event as CustomEvent<TradeExecutedEventDetail>).detail;
      const source = readEventString(detail?.Source ?? detail?.source)?.toLowerCase();
      const status = readEventString(detail?.Status ?? detail?.status)?.toLowerCase();
      const positionId = readEventNumber(detail?.PositionId ?? detail?.positionId);
      const isPaperTrade = source?.includes("paper") ?? positionId != null;

      if (isPaperTrade && status === "closed" && positionId != null) {
        dispatch(paperTradingActions.positionClosed(positionId));
      }

      clearScheduledRefreshes();
      PAPER_TRADE_RECONCILE_DELAYS_MS.forEach(scheduleSnapshotReconcile);
    };

    window.addEventListener("fintex:trade-executed", handleTradeExecuted);

    return () => {
      clearScheduledRefreshes();
      window.removeEventListener("fintex:trade-executed", handleTradeExecuted);
    };
  }, [clearScheduledRefreshes, fetchLatestSnapshot]);

  const createAccount = useCallback(async (input: { name: string; baseCurrency: string; startingBalance: number }) => {
    dispatch(paperTradingActions.submitStart());

    try {
      await createPaperTradingAccount(input);
      await fetchLatestSnapshot({ mode: "submit" });
    } catch (error) {
      dispatch(paperTradingActions.loadFailure(error instanceof Error ? error.message : "We could not create the paper trading account."));
    }
  }, [fetchLatestSnapshot]);

  const placeOrder = useCallback(async (input: Parameters<typeof placePaperTradingOrder>[0]) => {
    dispatch(paperTradingActions.submitStart());

    try {
      const result = await placePaperTradingOrder(input);
      dispatch(paperTradingActions.assessmentUpdated(result));

      if (result.wasExecuted) {
        await fetchLatestSnapshot({ mode: "submit" });
      }

      return result;
    } catch (error) {
      dispatch(paperTradingActions.loadFailure(error instanceof Error ? error.message : "We could not place the paper trade."));
      return null;
    }
  }, [fetchLatestSnapshot]);

  const getRecommendation = useCallback(async (input: Parameters<typeof getPaperTradeRecommendation>[0]) => {
    dispatch(paperTradingActions.submitStart());

    try {
      const recommendation = await getPaperTradeRecommendation(input);
      dispatch(paperTradingActions.recommendationUpdated(recommendation));
      return recommendation;
    } catch (error) {
      dispatch(paperTradingActions.loadFailure(error instanceof Error ? error.message : "We could not load a recommendation."));
      return null;
    }
  }, []);

  const closePosition = useCallback(async (input: Parameters<typeof closePaperTradingPosition>[0]) => {
    dispatch(paperTradingActions.submitStart());

    try {
      await closePaperTradingPosition(input);
      await fetchLatestSnapshot({ mode: "submit" });
    } catch (error) {
      dispatch(paperTradingActions.loadFailure(error instanceof Error ? error.message : "We could not close the paper position."));
    }
  }, [fetchLatestSnapshot]);

  const actionValues = useMemo<PaperTradingProviderActions>(
    () => ({
      refreshSnapshot,
      createAccount,
      placeOrder,
      getRecommendation,
      closePosition,
      clearError: () => dispatch(paperTradingActions.clearError()),
      clearFeedback: () => dispatch(paperTradingActions.clearFeedback()),
    }),
    [closePosition, createAccount, getRecommendation, placeOrder, refreshSnapshot],
  );

  return { state, actionValues };
};
