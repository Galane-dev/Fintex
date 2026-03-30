"use client";

import { useCallback, useEffect, useMemo, useReducer } from "react";
import type { ExternalBrokerProviderActions } from "@/types/external-broker";
import {
  connectExternalBrokerAccount,
  disconnectExternalBrokerAccount,
  getExternalBrokerConnections,
} from "@/utils/external-broker-api";
import { externalBrokerActions } from "./actions";
import { externalBrokerReducer, initialExternalBrokerState } from "./reducer";

const CONNECTION_REFRESH_MS = 30_000;

export const useExternalBrokerProvider = () => {
  const [state, dispatch] = useReducer(externalBrokerReducer, initialExternalBrokerState);

  const refreshConnections = useCallback(async () => {
    dispatch(externalBrokerActions.loadStart());

    try {
      dispatch(externalBrokerActions.loadSuccess(await getExternalBrokerConnections()));
    } catch (error) {
      dispatch(
        externalBrokerActions.loadFailure(
          error instanceof Error
            ? error.message
            : "We could not load your external broker connections.",
        ),
      );
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
        dispatch(externalBrokerActions.submitSuccess(await getExternalBrokerConnections()));
        return connection;
      } catch (error) {
        dispatch(
          externalBrokerActions.loadFailure(
            error instanceof Error
              ? error.message
              : "We could not connect the external broker account.",
          ),
        );
        return null;
      }
    },
    [],
  );

  const disconnectAccount = useCallback(async (input: { id: number }) => {
    dispatch(externalBrokerActions.submitStart());

    try {
      await disconnectExternalBrokerAccount(input.id);
      dispatch(externalBrokerActions.submitSuccess(await getExternalBrokerConnections()));
    } catch (error) {
      dispatch(
        externalBrokerActions.loadFailure(
          error instanceof Error
            ? error.message
            : "We could not disconnect the external broker account.",
        ),
      );
    }
  }, []);

  const actionValues = useMemo<ExternalBrokerProviderActions>(
    () => ({
      refreshConnections,
      connectAccount,
      disconnectAccount,
      clearError: () => dispatch(externalBrokerActions.clearError()),
    }),
    [connectAccount, disconnectAccount, refreshConnections],
  );

  return { state, actionValues };
};
