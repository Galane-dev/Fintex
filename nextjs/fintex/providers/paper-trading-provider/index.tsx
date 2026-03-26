"use client";

import {
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useReducer,
} from "react";
import type { PaperTradingProviderActions } from "@/types/paper-trading";
import {
  closePaperTradingPosition,
  createPaperTradingAccount,
  getPaperTradeRecommendation,
  getPaperTradingSnapshot,
  placePaperTradingOrder,
} from "@/utils/paper-trading-api";
import { PaperTradingActionContext, PaperTradingStateContext } from "./context";
import { paperTradingActions } from "./actions";
import { initialPaperTradingState, paperTradingReducer } from "./reducer";

const SNAPSHOT_REFRESH_MS = 15_000;
const EMPTY_SNAPSHOT = {
  account: null,
  positions: [],
  recentOrders: [],
  recentFills: [],
};

const isMissingAccountMessage = (message: string) =>
  message.toLowerCase().includes("create a paper trading account");

export function PaperTradingProvider({ children }: PropsWithChildren) {
  const [state, dispatch] = useReducer(
    paperTradingReducer,
    initialPaperTradingState,
  );

  const refreshSnapshot = useCallback(async () => {
    dispatch(paperTradingActions.loadStart());

    try {
      const snapshot = await getPaperTradingSnapshot();
      dispatch(paperTradingActions.loadSuccess(snapshot));
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "We could not refresh the paper trading snapshot.";
      if (isMissingAccountMessage(message)) {
        dispatch(paperTradingActions.loadSuccess(EMPTY_SNAPSHOT));
        return;
      }

      dispatch(paperTradingActions.loadFailure(message));
    }
  }, []);

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

  const createAccount = useCallback(
    async (input: {
      name: string;
      baseCurrency: string;
      startingBalance: number;
    }) => {
      dispatch(paperTradingActions.submitStart());

      try {
        await createPaperTradingAccount(input);
        const snapshot = await getPaperTradingSnapshot();
        dispatch(paperTradingActions.submitSuccess(snapshot));
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "We could not create the paper trading account.";
        dispatch(paperTradingActions.loadFailure(message));
      }
    },
    [],
  );

  const placeOrder = useCallback(
    async (input: Parameters<typeof placePaperTradingOrder>[0]) => {
      dispatch(paperTradingActions.submitStart());

      try {
        const result = await placePaperTradingOrder(input);
        dispatch(paperTradingActions.assessmentUpdated(result));

        if (result.wasExecuted) {
          const snapshot = await getPaperTradingSnapshot();
          dispatch(paperTradingActions.submitSuccess(snapshot));
        }

        return result;
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "We could not place the paper trade.";
        dispatch(paperTradingActions.loadFailure(message));
        return null;
      }
    },
    [],
  );

  const getRecommendation = useCallback(
    async (input: Parameters<typeof getPaperTradeRecommendation>[0]) => {
      dispatch(paperTradingActions.submitStart());

      try {
        const recommendation = await getPaperTradeRecommendation(input);
        dispatch(paperTradingActions.recommendationUpdated(recommendation));
        return recommendation;
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "We could not load a recommendation.";
        dispatch(paperTradingActions.loadFailure(message));
        return null;
      }
    },
    [],
  );

  const closePosition = useCallback(
    async (input: Parameters<typeof closePaperTradingPosition>[0]) => {
      dispatch(paperTradingActions.submitStart());

      try {
        await closePaperTradingPosition(input);
        const snapshot = await getPaperTradingSnapshot();
        dispatch(paperTradingActions.submitSuccess(snapshot));
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "We could not close the paper position.";
        dispatch(paperTradingActions.loadFailure(message));
      }
    },
    [],
  );

  const clearError = useCallback(() => {
    dispatch(paperTradingActions.clearError());
  }, []);

  const clearFeedback = useCallback(() => {
    dispatch(paperTradingActions.clearFeedback());
  }, []);

  const actionValues = useMemo<PaperTradingProviderActions>(
    () => ({
      refreshSnapshot,
      createAccount,
      placeOrder,
      getRecommendation,
      closePosition,
      clearError,
      clearFeedback,
    }),
    [
      clearError,
      clearFeedback,
      closePosition,
      createAccount,
      getRecommendation,
      placeOrder,
      refreshSnapshot,
    ],
  );

  return (
    <PaperTradingStateContext.Provider value={state}>
      <PaperTradingActionContext.Provider value={actionValues}>
        {children}
      </PaperTradingActionContext.Provider>
    </PaperTradingStateContext.Provider>
  );
}
