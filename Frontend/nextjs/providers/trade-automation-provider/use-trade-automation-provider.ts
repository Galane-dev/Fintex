"use client";

import { useCallback, useEffect, useMemo, useReducer } from "react";
import type { TradeAutomationActions } from "@/types/trade-automation";
import {
  createTradeAutomationRule,
  deleteTradeAutomationRule,
  getMyTradeAutomationRules,
} from "@/utils/trade-automation-api";
import { tradeAutomationActions } from "./actions";
import { initialTradeAutomationState, tradeAutomationReducer } from "./reducer";

const AUTOMATION_REFRESH_MS = 60_000;

export const useTradeAutomationProvider = () => {
  const [state, dispatch] = useReducer(tradeAutomationReducer, initialTradeAutomationState);

  const refreshRules = useCallback(async () => {
    dispatch(tradeAutomationActions.loadStart());

    try {
      dispatch(tradeAutomationActions.loadSuccess(await getMyTradeAutomationRules()));
    } catch (error) {
      dispatch(
        tradeAutomationActions.loadFailure(
          error instanceof Error ? error.message : "We could not load your auto-execution rules.",
        ),
      );
    }
  }, []);

  const runSavingAction = useCallback(async (action: () => Promise<void>) => {
    dispatch(tradeAutomationActions.saveStart());

    try {
      await action();
      await refreshRules();
      dispatch(tradeAutomationActions.saveDone());
    } catch (error) {
      dispatch(
        tradeAutomationActions.loadFailure(
          error instanceof Error ? error.message : "We could not update your auto-execution rules.",
        ),
      );
      throw error;
    }
  }, [refreshRules]);

  useEffect(() => {
    void refreshRules();
  }, [refreshRules]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshRules();
    }, AUTOMATION_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshRules]);

  const actionValues = useMemo<TradeAutomationActions>(
    () => ({
      refreshRules,
      createRule: async (input) => {
        try {
          await runSavingAction(async () => {
            await createTradeAutomationRule(input);
          });
          return true;
        } catch {
          return false;
        }
      },
      deleteRule: async (ruleId) => {
        await runSavingAction(async () => {
          await deleteTradeAutomationRule(ruleId);
        });
      },
      clearError: () => dispatch(tradeAutomationActions.clearError()),
    }),
    [refreshRules, runSavingAction],
  );

  return { state, actionValues };
};
