import type {
  CreatePaperTradingAccountInput,
  PaperTradeExecutionResult,
  PaperTradeRecommendation,
  PaperTradingProviderActions,
  PaperTradingSnapshot,
  PaperTradingState,
  PlacePaperOrderInput,
  ClosePaperPositionInput,
  GetPaperTradeRecommendationInput,
} from "@/types/paper-trading";

export type PaperTradingReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: PaperTradingSnapshot }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SUBMIT_START" }
  | { type: "SUBMIT_SUCCESS"; payload: PaperTradingSnapshot }
  | { type: "ASSESSMENT_UPDATED"; payload: PaperTradeExecutionResult }
  | { type: "RECOMMENDATION_UPDATED"; payload: PaperTradeRecommendation }
  | { type: "CLEAR_ERROR" }
  | { type: "CLEAR_FEEDBACK" };

export const createInitialPaperTradingState = (): PaperTradingState => ({
  isLoading: true,
  isSubmitting: false,
  error: null,
  snapshot: null,
  latestAssessment: null,
  recommendation: null,
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
  assessmentUpdated: (
    payload: PaperTradeExecutionResult,
  ): PaperTradingReducerAction => ({
    type: "ASSESSMENT_UPDATED",
    payload,
  }),
  recommendationUpdated: (
    payload: PaperTradeRecommendation,
  ): PaperTradingReducerAction => ({
    type: "RECOMMENDATION_UPDATED",
    payload,
  }),
  clearError: (): PaperTradingReducerAction => ({ type: "CLEAR_ERROR" }),
  clearFeedback: (): PaperTradingReducerAction => ({ type: "CLEAR_FEEDBACK" }),
};

export type PaperTradingActionMethods = PaperTradingProviderActions & {
  createAccount: (input: CreatePaperTradingAccountInput) => Promise<void>;
  placeOrder: (input: PlacePaperOrderInput) => Promise<PaperTradeExecutionResult | null>;
  getRecommendation: (
    input: GetPaperTradeRecommendationInput,
  ) => Promise<PaperTradeRecommendation | null>;
  closePosition: (input: ClosePaperPositionInput) => Promise<void>;
};
