"use client";

import { PropsWithChildren } from "react";
import { LiveTradingActionContext, LiveTradingStateContext } from "./context";
import { useLiveTradingProvider } from "./use-live-trading-provider";

export function LiveTradingProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = useLiveTradingProvider();

  return (
    <LiveTradingStateContext.Provider value={state}>
      <LiveTradingActionContext.Provider value={actionValues}>
        {children}
      </LiveTradingActionContext.Provider>
    </LiveTradingStateContext.Provider>
  );
}
