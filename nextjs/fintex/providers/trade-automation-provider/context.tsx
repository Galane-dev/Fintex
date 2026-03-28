"use client";

import { createContext } from "react";
import type { TradeAutomationActions, TradeAutomationState } from "@/types/trade-automation";

export const TradeAutomationStateContext = createContext<TradeAutomationState | null>(null);
export const TradeAutomationActionContext = createContext<TradeAutomationActions | null>(null);
