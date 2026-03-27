"use client";

import { useCallback, useEffect, useMemo, useReducer } from "react";
import type { LiveTradingProviderActions } from "@/types/live-trading";
import {
  getMyLiveTrades,
  placeLiveOrder,
  syncExternalBrokerTrades,
} from "@/utils/live-trading-api";
import { liveTradingActions } from "./actions";
import { initialLiveTradingState, liveTradingReducer } from "./reducer";

const LIVE_TRADES_REFRESH_MS = 15_000;

export const useLiveTradingProvider = () => {
  const [state, dispatch] = useReducer(liveTradingReducer, initialLiveTradingState);

  const refreshTrades = useCallback(async () => {
    dispatch(liveTradingActions.loadStart());

    try {
      await syncExternalBrokerTrades();
      dispatch(liveTradingActions.loadSuccess(await getMyLiveTrades()));
    } catch (error) {
      dispatch(
        liveTradingActions.loadFailure(
          error instanceof Error ? error.message : "We could not refresh your live trades.",
        ),
      );
    }
  }, []);

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

  const placeOrder = useCallback(async (input: Parameters<typeof placeLiveOrder>[0]) => {
    dispatch(liveTradingActions.submitStart());

    try {
      const execution = await placeLiveOrder(input);
      await syncExternalBrokerTrades();
      dispatch(liveTradingActions.submitSuccess(await getMyLiveTrades(), execution));
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
  }, []);

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
