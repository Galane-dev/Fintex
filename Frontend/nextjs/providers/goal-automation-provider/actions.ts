import type { GoalAutomationState, GoalTarget } from "@/types/goal-automation";

export type GoalAutomationReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: GoalTarget[] }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SAVE_START" }
  | { type: "SAVE_DONE" }
  | { type: "CLEAR_ERROR" };

export const initialGoalAutomationState: GoalAutomationState = {
  isLoading: true,
  isSaving: false,
  error: null,
  goals: [],
};

export const goalAutomationActions = {
  loadStart: (): GoalAutomationReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (payload: GoalTarget[]): GoalAutomationReducerAction => ({ type: "LOAD_SUCCESS", payload }),
  loadFailure: (payload: string): GoalAutomationReducerAction => ({ type: "LOAD_FAILURE", payload }),
  saveStart: (): GoalAutomationReducerAction => ({ type: "SAVE_START" }),
  saveDone: (): GoalAutomationReducerAction => ({ type: "SAVE_DONE" }),
  clearError: (): GoalAutomationReducerAction => ({ type: "CLEAR_ERROR" }),
};
