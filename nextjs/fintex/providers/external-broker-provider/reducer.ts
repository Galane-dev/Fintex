import type { ExternalBrokerState } from "@/types/external-broker";
import {
  createInitialExternalBrokerState,
  type ExternalBrokerReducerAction,
} from "./actions";

export const initialExternalBrokerState: ExternalBrokerState =
  createInitialExternalBrokerState();

export const externalBrokerReducer = (
  state: ExternalBrokerState,
  action: ExternalBrokerReducerAction,
): ExternalBrokerState => {
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
        connections: action.payload,
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
        connections: action.payload,
        lastHydratedAt: new Date().toISOString(),
      };
    case "CLEAR_ERROR":
      return {
        ...state,
        error: null,
      };
    default:
      return state;
  }
};
