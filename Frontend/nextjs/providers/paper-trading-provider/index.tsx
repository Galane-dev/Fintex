"use client";

import { PropsWithChildren } from "react";
import { PaperTradingActionContext, PaperTradingStateContext } from "./context";
import { usePaperTradingProvider } from "./use-paper-trading-provider";

export function PaperTradingProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = usePaperTradingProvider();

  return (
    <PaperTradingStateContext.Provider value={state}>
      <PaperTradingActionContext.Provider value={actionValues}>
        {children}
      </PaperTradingActionContext.Provider>
    </PaperTradingStateContext.Provider>
  );
}
