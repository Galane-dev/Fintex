"use client";

import {
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useReducer,
} from "react";
import type { LiveTradingProviderActions } from "@/types/live-trading";
import {
  getMyLiveTrades,
  placeLiveOrder,
  syncExternalBrokerTrades,
} from "@/utils/live-trading-api";
import { LiveTradingActionContext, LiveTradingStateContext } from "./context";
import { initialLiveTradingState, liveTradingReducer } from "./reducer";
import { liveTradingActions } from "./actions";

const LIVE_TRADES_REFRESH_MS = 15_000;

export function LiveTradingProvider({ children }: PropsWithChildren) {
  const [state, dispatch] = useReducer(
    liveTradingReducer,
    initialLiveTradingState,
  );

  const refreshTrades = useCallback(async () => {
    dispatch(liveTradingActions.loadStart());

    try {
      await syncExternalBrokerTrades();
      const trades = await getMyLiveTrades();
      dispatch(liveTradingActions.loadSuccess(trades));
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "We could not refresh your live trades.";
      dispatch(liveTradingActions.loadFailure(message));
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

  const submitOrder = useCallback(
    async (input: Parameters<typeof placeLiveOrder>[0]) => {
      dispatch(liveTradingActions.submitStart());

      try {
        const execution = await placeLiveOrder(input);
        await syncExternalBrokerTrades();
        const trades = await getMyLiveTrades();
        dispatch(liveTradingActions.submitSuccess(trades, execution));
        return execution;
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "We could not place the live broker order.";
        dispatch(liveTradingActions.loadFailure(message));
        return null;
      }
    },
    [],
  );

  const clearError = useCallback(() => {
    dispatch(liveTradingActions.clearError());
  }, []);

  const clearExecution = useCallback(() => {
    dispatch(liveTradingActions.clearExecution());
  }, []);

  const actionValues = useMemo<LiveTradingProviderActions>(
    () => ({
      refreshTrades,
      placeOrder: submitOrder,
      clearError,
      clearExecution,
    }),
    [clearError, clearExecution, refreshTrades, submitOrder],
  );

  return (
    <LiveTradingStateContext.Provider value={state}>
      <LiveTradingActionContext.Provider value={actionValues}>
        {children}
      </LiveTradingActionContext.Provider>
    </LiveTradingStateContext.Provider>
  );
}
