import type {
  CreatePaperTradingAccountInput,
  PaperTradingProviderActions,
  PaperTradingSnapshot,
  PaperTradingState,
  PlacePaperOrderInput,
  ClosePaperPositionInput,
} from "@/types/paper-trading";

export type PaperTradingReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: PaperTradingSnapshot }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SUBMIT_START" }
  | { type: "SUBMIT_SUCCESS"; payload: PaperTradingSnapshot }
  | { type: "CLEAR_ERROR" };

export const createInitialPaperTradingState = (): PaperTradingState => ({
  isLoading: true,
  isSubmitting: false,
  error: null,
  snapshot: null,
  lastHydratedAt: null,
});

export const paperTradingActions = {
  loadStart: (): PaperTradingReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (payload: PaperTradingSnapshot): PaperTradingReducerAction => ({
    type: "LOAD_SUCCESS",
    payload,
  }),
  loadFailure: (payload: string): PaperTradingReducerAction => ({
    type: "LOAD_FAILURE",
    payload,
  }),
  submitStart: (): PaperTradingReducerAction => ({ type: "SUBMIT_START" }),
  submitSuccess: (payload: PaperTradingSnapshot): PaperTradingReducerAction => ({
    type: "SUBMIT_SUCCESS",
    payload,
  }),
  clearError: (): PaperTradingReducerAction => ({ type: "CLEAR_ERROR" }),
};

export type PaperTradingActionMethods = PaperTradingProviderActions & {
  createAccount: (input: CreatePaperTradingAccountInput) => Promise<void>;
  placeOrder: (input: PlacePaperOrderInput) => Promise<void>;
  closePosition: (input: ClosePaperPositionInput) => Promise<void>;
};
