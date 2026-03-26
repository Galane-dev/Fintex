import type { MarketDataState } from "@/types/market-data";
import { upsertHistoryPoint } from "@/utils/market-data";
import { createInitialMarketDataState, type MarketDataReducerAction } from "./actions";

export const initialMarketDataState: MarketDataState = createInitialMarketDataState();

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

      return {
        ...state,
        isLoading: false,
        error: null,
        history: action.payload.history,
        latest,
        verdict: action.payload.verdict,
        timeframeRsi: action.payload.timeframeRsi,
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
        verdict: action.payload.verdict,
        timeframeRsi: action.payload.timeframeRsi,
        lastHydratedAt: new Date().toISOString(),
      };
    case "LIVE_VERDICT_UPDATED":
      return {
        ...state,
        error: null,
        verdict: action.payload.verdict,
        timeframeRsi: action.payload.timeframeRsi,
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
        verdict: state.verdict
          ? {
              ...state.verdict,
              marketDataPointId: action.payload.id,
              price: action.payload.price,
              trendScore: action.payload.trendScore,
              confidenceScore: action.payload.confidenceScore,
              verdict: action.payload.verdict,
              timestamp: action.payload.timestamp,
            }
          : {
              marketDataPointId: action.payload.id,
              symbol: action.payload.symbol,
              provider: action.payload.provider,
              price: action.payload.price,
              sma: action.payload.sma,
              ema: action.payload.ema,
              rsi: action.payload.rsi,
              macd: action.payload.macd,
              macdSignal: action.payload.macdSignal,
              macdHistogram: action.payload.macdHistogram,
              momentum: action.payload.momentum,
              rateOfChange: action.payload.rateOfChange,
              atr: null,
              atrPercent: null,
              adx: null,
              structureScore: 0,
              structureLabel: "Waiting",
              timeframeAlignmentScore: 0,
              trendScore: action.payload.trendScore,
              confidenceScore: action.payload.confidenceScore,
              verdict: action.payload.verdict,
              timestamp: action.payload.timestamp,
              indicatorScores: [],
              timeframeSignals: [],
            },
      };
    }
    default:
      return state;
  }
};
