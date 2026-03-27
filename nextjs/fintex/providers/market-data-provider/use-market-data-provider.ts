"use client";

import { useCallback, useEffect, useMemo, useReducer, useRef } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import type { MarketDataProviderActions } from "@/types/market-data";
import { readStoredSession } from "@/utils/auth-storage";
import { getApiBaseUrl } from "@/utils/api-config";
import {
  getMarketHistory,
  getRealtimeVerdict,
  getRelativeStrengthIndexTimeframes,
} from "@/utils/market-data-api";
import {
  normalizeMarketDataPoint,
  normalizeMarketVerdict,
  normalizeTimeframeRsi,
} from "@/utils/market-data";
import { getMarketSelectionByKey, marketDataActions } from "./actions";
import { initialMarketDataState, marketDataReducer } from "./reducer";

const FALLBACK_REFRESH_MS = 45_000;
const DERIVED_REFRESH_MS = 60_000;

const buildMarketHubUrl = (encryptedToken: string) =>
  `${getApiBaseUrl()}/signalr/market-data?enc_auth_token=${encodeURIComponent(encryptedToken)}`;

export const useMarketDataProvider = () => {
  const [state, dispatch] = useReducer(marketDataReducer, initialMarketDataState);
  const connectionRef = useRef<HubConnection | null>(null);

  const refreshSnapshot = useCallback(async () => {
    dispatch(marketDataActions.loadStart());

    try {
      const [history, verdict, timeframeRsi] = await Promise.all([
        getMarketHistory(state.selection, 80),
        getRealtimeVerdict(state.selection),
        getRelativeStrengthIndexTimeframes(state.selection),
      ]);

      dispatch(marketDataActions.loadSuccess({ history, verdict, timeframeRsi }));
    } catch (error) {
      dispatch(
        marketDataActions.loadFailure(
          error instanceof Error
            ? error.message
            : "We could not refresh the market snapshot.",
        ),
      );
    }
  }, [state.selection]);

  const refreshDerivedData = useCallback(async () => {
    try {
      const [verdict, timeframeRsi] = await Promise.all([
        getRealtimeVerdict(state.selection),
        getRelativeStrengthIndexTimeframes(state.selection),
      ]);

      dispatch(marketDataActions.derivedDataRefreshed({ verdict, timeframeRsi }));
    } catch {
      // Keep live market updates flowing even when the periodic derived refresh fails.
    }
  }, [state.selection]);

  useEffect(() => {
    void refreshSnapshot();
  }, [refreshSnapshot]);

  useEffect(() => {
    const session = readStoredSession();
    if (!session?.encryptedToken) {
      dispatch(marketDataActions.connectionStatusChanged("disconnected"));
      return;
    }

    dispatch(marketDataActions.connectionStatusChanged("connecting"));

    const connection = new HubConnectionBuilder()
      .withUrl(buildMarketHubUrl(session.encryptedToken))
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on("marketDataUpdated", (payload: Record<string, unknown>) => {
      const point = normalizeMarketDataPoint(payload);
      if (
        point.symbol.toUpperCase() !== state.selection.symbol.toUpperCase() ||
        point.provider !== state.selection.provider
      ) {
        return;
      }

      dispatch(marketDataActions.marketDataUpdated(point));
    });

    connection.on("marketVerdictUpdated", (payload: Record<string, unknown>) => {
      const verdictPayload = payload.verdict ?? payload.Verdict;
      const timeframePayload = payload.timeframeRsi ?? payload.TimeframeRsi;
      const verdict =
        verdictPayload && typeof verdictPayload === "object"
          ? normalizeMarketVerdict(verdictPayload as Record<string, unknown>)
          : null;

      if (
        verdict &&
        (verdict.symbol.toUpperCase() !== state.selection.symbol.toUpperCase() ||
          verdict.provider !== state.selection.provider)
      ) {
        return;
      }

      const timeframeRsi = Array.isArray(timeframePayload)
        ? timeframePayload.map((item) =>
            normalizeTimeframeRsi(item as Record<string, unknown>),
          )
        : [];

      dispatch(marketDataActions.liveVerdictUpdated({ verdict, timeframeRsi }));
    });

    connection.onreconnecting(() => {
      dispatch(marketDataActions.connectionStatusChanged("reconnecting"));
    });

    connection.onreconnected(async () => {
      dispatch(marketDataActions.connectionStatusChanged("connected"));
      await connection.invoke("SubscribeSymbol", state.selection.symbol);
    });

    connection.onclose(() => {
      dispatch(marketDataActions.connectionStatusChanged("disconnected"));
    });

    const startConnection = async () => {
      try {
        await connection.start();
        await connection.invoke("SubscribeSymbol", state.selection.symbol);
        dispatch(marketDataActions.connectionStatusChanged("connected"));
      } catch {
        dispatch(marketDataActions.connectionStatusChanged("error"));
      }
    };

    void startConnection();

    return () => {
      const activeConnection = connectionRef.current;
      connectionRef.current = null;

      if (activeConnection) {
        void activeConnection.stop();
      }
    };
  }, [state.selection.provider, state.selection.symbol]);

  useEffect(() => {
    if (state.connectionStatus === "connected") {
      return;
    }

    const interval = window.setInterval(() => {
      void refreshSnapshot();
    }, FALLBACK_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshSnapshot, state.connectionStatus]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshDerivedData();
    }, DERIVED_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshDerivedData]);

  const actionValues = useMemo<MarketDataProviderActions>(
    () => ({
      selectMarket: (marketKey: string) =>
        dispatch(marketDataActions.selectMarket(getMarketSelectionByKey(marketKey))),
      refreshSnapshot,
    }),
    [refreshSnapshot],
  );

  return { state, actionValues };
};
