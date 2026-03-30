"use client";

import { PropsWithChildren } from "react";
import { MarketDataActionContext, MarketDataStateContext } from "./context";
import { useMarketDataProvider } from "./use-market-data-provider";

export function MarketDataProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = useMarketDataProvider();

  return (
    <MarketDataStateContext.Provider value={state}>
      <MarketDataActionContext.Provider value={actionValues}>
        {children}
      </MarketDataActionContext.Provider>
    </MarketDataStateContext.Provider>
  );
}
