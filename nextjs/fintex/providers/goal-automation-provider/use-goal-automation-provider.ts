"use client";

import { useCallback, useEffect, useMemo, useReducer } from "react";
import type { GoalAutomationActions } from "@/types/goal-automation";
import {
  cancelGoalTarget,
  createGoalTarget,
  getMyGoalTargets,
  pauseGoalTarget,
  resumeGoalTarget,
} from "@/utils/goal-automation-api";
import { goalAutomationActions } from "./actions";
import { goalAutomationReducer, initialGoalAutomationState } from "./reducer";

const GOAL_REFRESH_MS = 60_000;

export const useGoalAutomationProvider = () => {
  const [state, dispatch] = useReducer(goalAutomationReducer, initialGoalAutomationState);

  const refreshGoals = useCallback(async () => {
    dispatch(goalAutomationActions.loadStart());

    try {
      dispatch(goalAutomationActions.loadSuccess(await getMyGoalTargets()));
    } catch (error) {
      dispatch(
        goalAutomationActions.loadFailure(
          error instanceof Error ? error.message : "We could not load your BTC goal targets.",
        ),
      );
    }
  }, []);

  const runSavingAction = useCallback(async (action: () => Promise<void>) => {
    dispatch(goalAutomationActions.saveStart());

    try {
      await action();
      await refreshGoals();
      dispatch(goalAutomationActions.saveDone());
    } catch (error) {
      dispatch(
        goalAutomationActions.loadFailure(
          error instanceof Error ? error.message : "We could not update your BTC goal targets.",
        ),
      );
      throw error;
    }
  }, [refreshGoals]);

  useEffect(() => {
    void refreshGoals();
  }, [refreshGoals]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshGoals();
    }, GOAL_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshGoals]);

  const actionValues = useMemo<GoalAutomationActions>(
    () => ({
      refreshGoals,
      createGoal: async (input) => {
        try {
          await runSavingAction(async () => {
            await createGoalTarget(input);
          });
          return true;
        } catch {
          return false;
        }
      },
      pauseGoal: async (goalId) => {
        await runSavingAction(async () => {
          await pauseGoalTarget(goalId);
        });
      },
      resumeGoal: async (goalId) => {
        await runSavingAction(async () => {
          await resumeGoalTarget(goalId);
        });
      },
      cancelGoal: async (goalId) => {
        await runSavingAction(async () => {
          await cancelGoalTarget(goalId);
        });
      },
      clearError: () => dispatch(goalAutomationActions.clearError()),
    }),
    [refreshGoals, runSavingAction],
  );

  return { state, actionValues };
};
