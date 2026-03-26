import { DEFAULT_MARKET, MARKET_OPTIONS } from "@/constants/market-data";
import type {
  MarketConnectionStatus,
  MarketDataPoint,
  MarketDataState,
  MarketSelection,
  MarketTimeframeRsi,
  MarketVerdictSnapshot,
} from "@/types/market-data";

export type MarketDataReducerAction =
  | { type: "SELECT_MARKET"; payload: MarketSelection }
  | { type: "LOAD_START" }
  | {
      type: "LOAD_SUCCESS";
      payload: {
        history: MarketDataPoint[];
        verdict: MarketVerdictSnapshot | null;
        timeframeRsi: MarketTimeframeRsi[];
      };
    }
  | {
      type: "DERIVED_DATA_REFRESHED";
      payload: {
        verdict: MarketVerdictSnapshot | null;
        timeframeRsi: MarketTimeframeRsi[];
      };
    }
  | {
      type: "LIVE_VERDICT_UPDATED";
      payload: {
        verdict: MarketVerdictSnapshot | null;
        timeframeRsi: MarketTimeframeRsi[];
      };
    }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "CONNECTION_STATUS_CHANGED"; payload: MarketConnectionStatus }
  | { type: "MARKET_DATA_UPDATED"; payload: MarketDataPoint };

export const createInitialMarketDataState = (): MarketDataState => ({
  isLoading: true,
  error: null,
  selection: DEFAULT_MARKET,
  latest: null,
  history: [],
  verdict: null,
  timeframeRsi: [],
  connectionStatus: "idle",
  lastHydratedAt: null,
});

export const getMarketSelectionByKey = (marketKey: string) =>
  MARKET_OPTIONS.find((item) => item.key === marketKey) ?? DEFAULT_MARKET;

export const marketDataActions = {
  selectMarket: (selection: MarketSelection): MarketDataReducerAction => ({
    type: "SELECT_MARKET",
    payload: selection,
  }),
  loadStart: (): MarketDataReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (payload: {
    history: MarketDataPoint[];
    verdict: MarketVerdictSnapshot | null;
    timeframeRsi: MarketTimeframeRsi[];
  }): MarketDataReducerAction => ({
    type: "LOAD_SUCCESS",
    payload,
  }),
  derivedDataRefreshed: (payload: {
    verdict: MarketVerdictSnapshot | null;
    timeframeRsi: MarketTimeframeRsi[];
  }): MarketDataReducerAction => ({
    type: "DERIVED_DATA_REFRESHED",
    payload,
  }),
  liveVerdictUpdated: (payload: {
    verdict: MarketVerdictSnapshot | null;
    timeframeRsi: MarketTimeframeRsi[];
  }): MarketDataReducerAction => ({
    type: "LIVE_VERDICT_UPDATED",
    payload,
  }),
  loadFailure: (message: string): MarketDataReducerAction => ({
    type: "LOAD_FAILURE",
    payload: message,
  }),
  connectionStatusChanged: (status: MarketConnectionStatus): MarketDataReducerAction => ({
    type: "CONNECTION_STATUS_CHANGED",
    payload: status,
  }),
  marketDataUpdated: (point: MarketDataPoint): MarketDataReducerAction => ({
    type: "MARKET_DATA_UPDATED",
    payload: point,
  }),
};
