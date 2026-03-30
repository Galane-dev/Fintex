import type { MarketDataState } from "@/types/market-data";
import { upsertHistoryPoint } from "@/utils/market-data";
import { createInitialMarketDataState, type MarketDataReducerAction } from "./actions";

export const initialMarketDataState: MarketDataState = createInitialMarketDataState();

const getIsoTime = (value: string | null | undefined) => {
  if (!value) {
    return 0;
  }

  const parsed = Date.parse(value);
  return Number.isNaN(parsed) ? 0 : parsed;
};

const getVerdictVersion = (verdict: MarketDataState["verdict"]) =>
  verdict == null
    ? 0
    : Math.max(getIsoTime(verdict.evaluatedAtUtc), getIsoTime(verdict.timestamp));

const pickNewerVerdict = (
  current: MarketDataState["verdict"],
  incoming: MarketDataState["verdict"],
) => {
  if (incoming == null) {
    return current;
  }

  if (current == null) {
    return incoming;
  }

  return getVerdictVersion(incoming) >= getVerdictVersion(current) ? incoming : current;
};

const getLatestTimeframeRsiVersion = (items: MarketDataState["timeframeRsi"]) =>
  items.reduce((latest, item) => Math.max(latest, getIsoTime(item.candleTimestamp)), 0);

const pickNewerTimeframeRsi = (
  current: MarketDataState["timeframeRsi"],
  incoming: MarketDataState["timeframeRsi"],
) => {
  if (incoming.length === 0) {
    return current;
  }

  if (current.length === 0) {
    return incoming;
  }

  return getLatestTimeframeRsiVersion(incoming) >= getLatestTimeframeRsiVersion(current)
    ? incoming
    : current;
};

export const marketDataReducer = (
  state: MarketDataState,
  action: MarketDataReducerAction,
): MarketDataState => {
  switch (action.type) {
    case "SELECT_MARKET":
      return {
        ...state,
        selection: action.payload,
        isLoading: true,
        error: null,
        latest: null,
        history: [],
        verdict: null,
        timeframeRsi: [],
        connectionStatus: "idle",
      };
    case "LOAD_START":
      return {
        ...state,
        isLoading: true,
        error: null,
      };
    case "LOAD_SUCCESS": {
      const latest = action.payload.history.at(-1) ?? null;
      const verdict = pickNewerVerdict(state.verdict, action.payload.verdict);
      const timeframeRsi = pickNewerTimeframeRsi(state.timeframeRsi, action.payload.timeframeRsi);

      return {
        ...state,
        isLoading: false,
        error: null,
        history: action.payload.history,
        latest,
        verdict,
        timeframeRsi,
        lastHydratedAt: new Date().toISOString(),
      };
    }
    case "LOAD_FAILURE":
      return {
        ...state,
        isLoading: false,
        error: action.payload,
      };
    case "DERIVED_DATA_REFRESHED":
      return {
        ...state,
        error: null,
        verdict: pickNewerVerdict(state.verdict, action.payload.verdict),
        timeframeRsi: pickNewerTimeframeRsi(state.timeframeRsi, action.payload.timeframeRsi),
        lastHydratedAt: new Date().toISOString(),
      };
    case "LIVE_VERDICT_UPDATED":
      return {
        ...state,
        error: null,
        verdict: pickNewerVerdict(state.verdict, action.payload.verdict),
        timeframeRsi: pickNewerTimeframeRsi(state.timeframeRsi, action.payload.timeframeRsi),
        lastHydratedAt: new Date().toISOString(),
      };
    case "CONNECTION_STATUS_CHANGED":
      return {
        ...state,
        connectionStatus: action.payload,
      };
    case "MARKET_DATA_UPDATED": {
      const history = upsertHistoryPoint(state.history, action.payload);
      return {
        ...state,
        history,
        latest: action.payload,
      };
    }
    default:
      return state;
  }
};
