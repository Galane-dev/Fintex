"use client";

import { useContext } from "react";
import {
  LiveTradingActionContext,
  LiveTradingStateContext,
} from "@/providers/live-trading-provider/context";

export const useLiveTrading = () => {
  const stateContext = useContext(LiveTradingStateContext);
  const actionContext = useContext(LiveTradingActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("useLiveTrading must be used within a LiveTradingProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
