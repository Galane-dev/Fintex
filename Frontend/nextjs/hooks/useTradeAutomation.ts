"use client";

import { useContext } from "react";
import {
  TradeAutomationActionContext,
  TradeAutomationStateContext,
} from "@/providers/trade-automation-provider/context";

export const useTradeAutomation = () => {
  const stateContext = useContext(TradeAutomationStateContext);
  const actionContext = useContext(TradeAutomationActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("useTradeAutomation must be used within a TradeAutomationProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
