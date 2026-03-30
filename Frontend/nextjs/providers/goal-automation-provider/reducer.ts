import type { GoalAutomationState } from "@/types/goal-automation";
import type { GoalAutomationReducerAction } from "./actions";
import { initialGoalAutomationState } from "./actions";

export const goalAutomationReducer = (
  state: GoalAutomationState,
  action: GoalAutomationReducerAction,
): GoalAutomationState => {
  switch (action.type) {
    case "LOAD_START":
      return { ...state, isLoading: true, error: null };
    case "LOAD_SUCCESS":
      return { ...state, isLoading: false, isSaving: false, error: null, goals: action.payload };
    case "LOAD_FAILURE":
      return { ...state, isLoading: false, isSaving: false, error: action.payload };
    case "SAVE_START":
      return { ...state, isSaving: true, error: null };
    case "SAVE_DONE":
      return { ...state, isSaving: false };
    case "CLEAR_ERROR":
      return { ...state, error: null };
    default:
      return state;
  }
};

export { initialGoalAutomationState };
