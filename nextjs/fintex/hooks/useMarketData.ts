"use client";

import { useContext } from "react";
import {
  MarketDataActionContext,
  MarketDataStateContext,
} from "@/providers/market-data-provider/context";

export const useMarketData = () => {
  const stateContext = useContext(MarketDataStateContext);
  const actionContext = useContext(MarketDataActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("useMarketData must be used within a MarketDataProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
