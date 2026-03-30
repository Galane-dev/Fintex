"use client";

import { useCallback, useEffect, useMemo, useReducer, useRef } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import type { MarketDataProviderActions, MarketSelection } from "@/types/market-data";
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
const STREAM_STALE_MS = 10_000;
const STREAM_WATCHDOG_CHECK_MS = 4_000;
const buildMarketHubUrl = (encryptedToken: string) =>
  `${getApiBaseUrl()}/signalr/market-data?enc_auth_token=${encodeURIComponent(encryptedToken)}`;
const MARKET_DATA_EVENT_NAMES = ["marketDataUpdated", "marketdataupdated"] as const;
const MARKET_VERDICT_EVENT_NAMES = ["marketVerdictUpdated", "marketverdictupdated"] as const;

const hasHealthyVerdictSnapshot = (
  verdict: ReturnType<typeof normalizeMarketVerdict> | null,
) =>
  verdict != null &&
  verdict.verdictState !== "fallback" &&
  verdict.confidenceScore != null &&
  verdict.adx != null &&
  verdict.nextOneMinuteProjection != null &&
  verdict.nextFiveMinuteProjection != null;

export const useMarketDataProvider = () => {
  const [state, dispatch] = useReducer(marketDataReducer, initialMarketDataState);
  const connectionRef = useRef<HubConnection | null>(null);
  const latestVerdictRef = useRef(state.verdict);
  const latestTimeframeRsiRef = useRef(state.timeframeRsi);
  const lastMarketEventAtRef = useRef(0);
  const lastVerdictEventAtRef = useRef(0);
  const lastDerivedRefreshAtRef = useRef(0);
  const isRefreshingDerivedRef = useRef(false);

  useEffect(() => {
    latestVerdictRef.current = state.verdict;
    latestTimeframeRsiRef.current = state.timeframeRsi;
  }, [state.timeframeRsi, state.verdict]);

  const refreshDerivedData = useCallback(async (selection: MarketSelection) => {
    if (isRefreshingDerivedRef.current) {
      return;
    }

    isRefreshingDerivedRef.current = true;
    lastDerivedRefreshAtRef.current = Date.now();

    try {
      const [verdictResult, timeframeRsiResult] = await Promise.allSettled([
        getRealtimeVerdict(selection),
        getRelativeStrengthIndexTimeframes(selection),
      ]);

      const verdict =
        verdictResult.status === "fulfilled"
          ? verdictResult.value
          : latestVerdictRef.current;
      const timeframeRsi =
        timeframeRsiResult.status === "fulfilled"
          ? timeframeRsiResult.value
          : latestTimeframeRsiRef.current;

      if (verdictResult.status === "fulfilled" && hasHealthyVerdictSnapshot(verdictResult.value)) {
        lastVerdictEventAtRef.current = Date.now();
      }

      dispatch(marketDataActions.derivedDataRefreshed({ verdict, timeframeRsi }));
    } finally {
      isRefreshingDerivedRef.current = false;
    }
  }, []);

  const refreshSnapshot = useCallback(async () => {
    dispatch(marketDataActions.loadStart());

    try {
      const history = await getMarketHistory(state.selection, 80);
      const [verdictResult, timeframeRsiResult] = await Promise.allSettled([
        getRealtimeVerdict(state.selection),
        getRelativeStrengthIndexTimeframes(state.selection),
      ]);

      const verdict =
        verdictResult.status === "fulfilled" ? verdictResult.value : null;
      const timeframeRsi =
        timeframeRsiResult.status === "fulfilled" ? timeframeRsiResult.value : [];

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

  useEffect(() => {
    void refreshSnapshot();
  }, [refreshSnapshot]);

  useEffect(() => {
    lastMarketEventAtRef.current = 0;
    lastVerdictEventAtRef.current = 0;
    lastDerivedRefreshAtRef.current = 0;
  }, [state.selection]);

  useEffect(() => {
    const selection = state.selection;
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

    const handleMarketDataUpdated = (payload: Record<string, unknown>) => {
      const point = normalizeMarketDataPoint(payload);
      if (
        point.symbol.toUpperCase() !== selection.symbol.toUpperCase() ||
        point.provider !== selection.provider
      ) {
        return;
      }

      lastMarketEventAtRef.current = Date.now();
      dispatch(marketDataActions.marketDataUpdated(point));
    };

    const handleMarketVerdictUpdated = (payload: Record<string, unknown>) => {
      const verdictPayload = payload.verdict ?? payload.Verdict;
      const timeframePayload = payload.timeframeRsi ?? payload.TimeframeRsi;
      const verdict =
        verdictPayload && typeof verdictPayload === "object"
          ? normalizeMarketVerdict(verdictPayload as Record<string, unknown>)
          : null;

      if (
        verdict &&
        (verdict.symbol.toUpperCase() !== selection.symbol.toUpperCase() ||
          verdict.provider !== selection.provider)
      ) {
        return;
      }

      const timeframeRsi = Array.isArray(timeframePayload)
        ? timeframePayload.map((item) =>
            normalizeTimeframeRsi(item as Record<string, unknown>),
          )
        : [];

      if (hasHealthyVerdictSnapshot(verdict)) {
        lastVerdictEventAtRef.current = Date.now();
      }

      dispatch(marketDataActions.liveVerdictUpdated({ verdict, timeframeRsi }));
    };

    MARKET_DATA_EVENT_NAMES.forEach((eventName) => {
      connection.on(eventName, handleMarketDataUpdated);
    });

    MARKET_VERDICT_EVENT_NAMES.forEach((eventName) => {
      connection.on(eventName, handleMarketVerdictUpdated);
    });

    connection.onreconnecting(() => {
      dispatch(marketDataActions.connectionStatusChanged("reconnecting"));
    });

    connection.onreconnected(async () => {
      dispatch(marketDataActions.connectionStatusChanged("connected"));
      await connection.invoke("SubscribeSymbol", selection.symbol);
      await refreshDerivedData(selection);
    });

    connection.onclose(() => {
      dispatch(marketDataActions.connectionStatusChanged("disconnected"));
    });

    const startConnection = async () => {
      try {
        await connection.start();
        await connection.invoke("SubscribeSymbol", selection.symbol);
        dispatch(marketDataActions.connectionStatusChanged("connected"));
        await refreshDerivedData(selection);
      } catch {
        dispatch(marketDataActions.connectionStatusChanged("error"));
      }
    };

    void startConnection();

    return () => {
      MARKET_DATA_EVENT_NAMES.forEach((eventName) => {
        connection.off(eventName, handleMarketDataUpdated);
      });

      MARKET_VERDICT_EVENT_NAMES.forEach((eventName) => {
        connection.off(eventName, handleMarketVerdictUpdated);
      });

      const activeConnection = connectionRef.current;
      connectionRef.current = null;

      if (activeConnection) {
        void activeConnection.stop();
      }
    };
  }, [refreshDerivedData, state.selection]);

  useEffect(() => {
    if (state.connectionStatus !== "connected") {
      return;
    }

    const interval = window.setInterval(() => {
      const now = Date.now();
      const marketStreamIsHot =
        lastMarketEventAtRef.current > 0 &&
        now - lastMarketEventAtRef.current <= STREAM_STALE_MS;
      const verdictStreamIsStale =
        lastVerdictEventAtRef.current === 0 ||
        now - lastVerdictEventAtRef.current > STREAM_STALE_MS;
      const refreshCooldownElapsed =
        now - lastDerivedRefreshAtRef.current >= STREAM_STALE_MS;

      if (marketStreamIsHot && verdictStreamIsStale && refreshCooldownElapsed) {
        void refreshDerivedData(state.selection);
      }
    }, STREAM_WATCHDOG_CHECK_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshDerivedData, state.connectionStatus, state.selection]);

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
