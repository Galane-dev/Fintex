import type { TradeAutomationRule, TradeAutomationState } from "@/types/trade-automation";

export type TradeAutomationReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: TradeAutomationRule[] }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SAVE_START" }
  | { type: "SAVE_DONE" }
  | { type: "CLEAR_ERROR" };

export const initialTradeAutomationState: TradeAutomationState = {
  isLoading: true,
  isSaving: false,
  error: null,
  rules: [],
};

export const tradeAutomationActions = {
  loadStart: (): TradeAutomationReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (payload: TradeAutomationRule[]): TradeAutomationReducerAction => ({
    type: "LOAD_SUCCESS",
    payload,
  }),
  loadFailure: (payload: string): TradeAutomationReducerAction => ({
    type: "LOAD_FAILURE",
    payload,
  }),
  saveStart: (): TradeAutomationReducerAction => ({ type: "SAVE_START" }),
  saveDone: (): TradeAutomationReducerAction => ({ type: "SAVE_DONE" }),
  clearError: (): TradeAutomationReducerAction => ({ type: "CLEAR_ERROR" }),
};
