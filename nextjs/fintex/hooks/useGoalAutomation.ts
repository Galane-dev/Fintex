"use client";

import { useContext } from "react";
import {
  GoalAutomationActionContext,
  GoalAutomationStateContext,
} from "@/providers/goal-automation-provider/context";

export const useGoalAutomation = () => {
  const stateContext = useContext(GoalAutomationStateContext);
  const actionContext = useContext(GoalAutomationActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("useGoalAutomation must be used within a GoalAutomationProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
