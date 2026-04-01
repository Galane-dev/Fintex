"use client";

import { useCallback, useEffect, useMemo, useReducer } from "react";
import { useNotifications } from "@/hooks/useNotifications";
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

const GOAL_REFRESH_MS = 15_000;

export const useGoalAutomationProvider = () => {
  const [state, dispatch] = useReducer(goalAutomationReducer, initialGoalAutomationState);
  const notifications = useNotifications();

  const loadGoals = useCallback(async (showLoading: boolean) => {
    if (showLoading) {
      dispatch(goalAutomationActions.loadStart());
    }

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

  const refreshGoals = useCallback(async () => {
    await loadGoals(true);
  }, [loadGoals]);

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
      void loadGoals(false);
    }, GOAL_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [loadGoals]);

  const latestGoalNotificationId = useMemo(
    () => notifications.notifications.find((item) => item.type === "GoalAutomation")?.id ?? null,
    [notifications.notifications],
  );

  useEffect(() => {
    if (latestGoalNotificationId == null) {
      return;
    }

    void loadGoals(false);
  }, [latestGoalNotificationId, loadGoals]);

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
