import type { TradeAutomationState } from "@/types/trade-automation";
import type { TradeAutomationReducerAction } from "./actions";
import { initialTradeAutomationState } from "./actions";

export const tradeAutomationReducer = (
  state: TradeAutomationState,
  action: TradeAutomationReducerAction,
): TradeAutomationState => {
  switch (action.type) {
    case "LOAD_START":
      return { ...state, isLoading: true, error: null };
    case "LOAD_SUCCESS":
      return { ...state, isLoading: false, isSaving: false, error: null, rules: action.payload };
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

export { initialTradeAutomationState };
