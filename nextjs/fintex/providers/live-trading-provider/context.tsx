"use client";

import { createContext } from "react";
import type {
  LiveTradingProviderActions,
  LiveTradingState,
} from "@/types/live-trading";

export const LiveTradingStateContext = createContext<LiveTradingState | null>(null);
export const LiveTradingActionContext =
  createContext<LiveTradingProviderActions | null>(null);
