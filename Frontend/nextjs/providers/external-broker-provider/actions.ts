import type {
  ConnectExternalBrokerAccountInput,
  ExternalBrokerConnection,
  ExternalBrokerProviderActions,
  ExternalBrokerState,
} from "@/types/external-broker";

export type ExternalBrokerReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: ExternalBrokerConnection[] }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SUBMIT_START" }
  | { type: "SUBMIT_SUCCESS"; payload: ExternalBrokerConnection[] }
  | { type: "CLEAR_ERROR" };

export const createInitialExternalBrokerState = (): ExternalBrokerState => ({
  isLoading: true,
  isSubmitting: false,
  error: null,
  connections: [],
  lastHydratedAt: null,
});

export const externalBrokerActions = {
  loadStart: (): ExternalBrokerReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (
    payload: ExternalBrokerConnection[],
  ): ExternalBrokerReducerAction => ({
    type: "LOAD_SUCCESS",
    payload,
  }),
  loadFailure: (payload: string): ExternalBrokerReducerAction => ({
    type: "LOAD_FAILURE",
    payload,
  }),
  submitStart: (): ExternalBrokerReducerAction => ({ type: "SUBMIT_START" }),
  submitSuccess: (
    payload: ExternalBrokerConnection[],
  ): ExternalBrokerReducerAction => ({
    type: "SUBMIT_SUCCESS",
    payload,
  }),
  clearError: (): ExternalBrokerReducerAction => ({ type: "CLEAR_ERROR" }),
};

export type ExternalBrokerActionMethods = ExternalBrokerProviderActions & {
  connectAccount: (
    input: ConnectExternalBrokerAccountInput,
  ) => Promise<ExternalBrokerConnection | null>;
  disconnectAccount: (input: { id: number }) => Promise<void>;
};
