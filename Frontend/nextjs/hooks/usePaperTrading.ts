"use client";

import { useContext } from "react";
import {
  PaperTradingActionContext,
  PaperTradingStateContext,
} from "@/providers/paper-trading-provider/context";

export const usePaperTrading = () => {
  const stateContext = useContext(PaperTradingStateContext);
  const actionContext = useContext(PaperTradingActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("usePaperTrading must be used within a PaperTradingProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
