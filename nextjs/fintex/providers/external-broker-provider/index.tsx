"use client";

import {
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useReducer,
} from "react";
import type { ExternalBrokerProviderActions } from "@/types/external-broker";
import {
  connectExternalBrokerAccount,
  disconnectExternalBrokerAccount,
  getExternalBrokerConnections,
} from "@/utils/external-broker-api";
import {
  ExternalBrokerActionContext,
  ExternalBrokerStateContext,
} from "./context";
import { externalBrokerActions } from "./actions";
import {
  externalBrokerReducer,
  initialExternalBrokerState,
} from "./reducer";

const CONNECTION_REFRESH_MS = 30_000;

export function ExternalBrokerProvider({ children }: PropsWithChildren) {
  const [state, dispatch] = useReducer(
    externalBrokerReducer,
    initialExternalBrokerState,
  );

  const refreshConnections = useCallback(async () => {
    dispatch(externalBrokerActions.loadStart());

    try {
      const connections = await getExternalBrokerConnections();
      dispatch(externalBrokerActions.loadSuccess(connections));
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "We could not load your external broker connections.";
      dispatch(externalBrokerActions.loadFailure(message));
    }
  }, []);

  useEffect(() => {
    void refreshConnections();
  }, [refreshConnections]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshConnections();
    }, CONNECTION_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshConnections]);

  const connectAccount = useCallback(
    async (input: Parameters<typeof connectExternalBrokerAccount>[0]) => {
      dispatch(externalBrokerActions.submitStart());

      try {
        const connection = await connectExternalBrokerAccount(input);
        const connections = await getExternalBrokerConnections();
        dispatch(externalBrokerActions.submitSuccess(connections));
        return connection;
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "We could not connect the external broker account.";
        dispatch(externalBrokerActions.loadFailure(message));
        return null;
      }
    },
    [],
  );

  const disconnectAccount = useCallback(async (input: { id: number }) => {
    dispatch(externalBrokerActions.submitStart());

    try {
      await disconnectExternalBrokerAccount(input.id);
      const connections = await getExternalBrokerConnections();
      dispatch(externalBrokerActions.submitSuccess(connections));
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "We could not disconnect the external broker account.";
      dispatch(externalBrokerActions.loadFailure(message));
    }
  }, []);

  const clearError = useCallback(() => {
    dispatch(externalBrokerActions.clearError());
  }, []);

  const actionValues = useMemo<ExternalBrokerProviderActions>(
    () => ({
      refreshConnections,
      connectAccount,
      disconnectAccount,
      clearError,
    }),
    [clearError, connectAccount, disconnectAccount, refreshConnections],
  );

  return (
    <ExternalBrokerStateContext.Provider value={state}>
      <ExternalBrokerActionContext.Provider value={actionValues}>
        {children}
      </ExternalBrokerActionContext.Provider>
    </ExternalBrokerStateContext.Provider>
  );
}
