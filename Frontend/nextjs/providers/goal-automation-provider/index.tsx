"use client";

import { PropsWithChildren } from "react";
import { GoalAutomationActionContext, GoalAutomationStateContext } from "./context";
import { useGoalAutomationProvider } from "./use-goal-automation-provider";

export function GoalAutomationProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = useGoalAutomationProvider();

  return (
    <GoalAutomationStateContext.Provider value={state}>
      <GoalAutomationActionContext.Provider value={actionValues}>
        {children}
      </GoalAutomationActionContext.Provider>
    </GoalAutomationStateContext.Provider>
  );
}
