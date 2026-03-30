import type { LiveTradingState } from "@/types/live-trading";
import {
  createInitialLiveTradingState,
  type LiveTradingReducerAction,
} from "./actions";

export const initialLiveTradingState: LiveTradingState =
  createInitialLiveTradingState();

export const liveTradingReducer = (
  state: LiveTradingState,
  action: LiveTradingReducerAction,
): LiveTradingState => {
  switch (action.type) {
    case "LOAD_START":
      return {
        ...state,
        isLoading: true,
        error: null,
      };
    case "LOAD_SUCCESS":
      return {
        ...state,
        isLoading: false,
        isSubmitting: false,
        error: null,
        trades: action.payload,
        lastHydratedAt: new Date().toISOString(),
      };
    case "LOAD_FAILURE":
      return {
        ...state,
        isLoading: false,
        isSubmitting: false,
        error: action.payload,
      };
    case "SUBMIT_START":
      return {
        ...state,
        isSubmitting: true,
        error: null,
      };
    case "SUBMIT_SUCCESS":
      return {
        ...state,
        isLoading: false,
        isSubmitting: false,
        error: null,
        trades: action.payload.trades,
        lastExecution: action.payload.execution,
        lastHydratedAt: new Date().toISOString(),
      };
    case "CLEAR_ERROR":
      return {
        ...state,
        error: null,
      };
    case "CLEAR_EXECUTION":
      return {
        ...state,
        lastExecution: null,
      };
    default:
      return state;
  }
};
