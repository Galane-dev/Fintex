import type {
  LiveTrade,
  LiveTradeExecution,
  LiveTradingProviderActions,
  LiveTradingState,
  PlaceLiveOrderInput,
} from "@/types/live-trading";

export type LiveTradingReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: LiveTrade[] }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SUBMIT_START" }
  | { type: "SUBMIT_SUCCESS"; payload: { trades: LiveTrade[]; execution: LiveTradeExecution } }
  | { type: "CLEAR_ERROR" }
  | { type: "CLEAR_EXECUTION" };

export const createInitialLiveTradingState = (): LiveTradingState => ({
  isLoading: true,
  isSubmitting: false,
  error: null,
  trades: [],
  lastExecution: null,
  lastHydratedAt: null,
});

export const liveTradingActions = {
  loadStart: (): LiveTradingReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (payload: LiveTrade[]): LiveTradingReducerAction => ({
    type: "LOAD_SUCCESS",
    payload,
  }),
  loadFailure: (payload: string): LiveTradingReducerAction => ({
    type: "LOAD_FAILURE",
    payload,
  }),
  submitStart: (): LiveTradingReducerAction => ({ type: "SUBMIT_START" }),
  submitSuccess: (
    trades: LiveTrade[],
    execution: LiveTradeExecution,
  ): LiveTradingReducerAction => ({
    type: "SUBMIT_SUCCESS",
    payload: {
      trades,
      execution,
    },
  }),
  clearError: (): LiveTradingReducerAction => ({ type: "CLEAR_ERROR" }),
  clearExecution: (): LiveTradingReducerAction => ({ type: "CLEAR_EXECUTION" }),
};

export type LiveTradingActionMethods = LiveTradingProviderActions & {
  placeOrder: (input: PlaceLiveOrderInput) => Promise<LiveTradeExecution | null>;
};
