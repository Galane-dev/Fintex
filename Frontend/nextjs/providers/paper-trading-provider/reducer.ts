import type { PaperTradingState } from "@/types/paper-trading";
import {
  createInitialPaperTradingState,
  type PaperTradingReducerAction,
} from "./actions";

export const initialPaperTradingState: PaperTradingState =
  createInitialPaperTradingState();

export const paperTradingReducer = (
  state: PaperTradingState,
  action: PaperTradingReducerAction,
): PaperTradingState => {
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
        snapshot: action.payload,
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
        snapshot: action.payload,
        lastHydratedAt: new Date().toISOString(),
      };
    case "ASSESSMENT_UPDATED":
      return {
        ...state,
        isSubmitting: false,
        latestAssessment: action.payload.assessment,
        recommendation: null,
      };
    case "RECOMMENDATION_UPDATED":
      return {
        ...state,
        isSubmitting: false,
        recommendation: action.payload,
        latestAssessment: null,
      };
    case "CLEAR_ERROR":
      return {
        ...state,
        error: null,
      };
    case "CLEAR_FEEDBACK":
      return {
        ...state,
        latestAssessment: null,
        recommendation: null,
      };
    default:
      return state;
  }
};
