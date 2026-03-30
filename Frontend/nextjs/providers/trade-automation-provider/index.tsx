"use client";

import { PropsWithChildren } from "react";
import { TradeAutomationActionContext, TradeAutomationStateContext } from "./context";
import { useTradeAutomationProvider } from "./use-trade-automation-provider";

export function TradeAutomationProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = useTradeAutomationProvider();

  return (
    <TradeAutomationStateContext.Provider value={state}>
      <TradeAutomationActionContext.Provider value={actionValues}>
        {children}
      </TradeAutomationActionContext.Provider>
    </TradeAutomationStateContext.Provider>
  );
}
