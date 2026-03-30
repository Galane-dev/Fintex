"use client";

import { createContext } from "react";
import type {
  PaperTradingProviderActions,
  PaperTradingState,
} from "@/types/paper-trading";

export const PaperTradingStateContext = createContext<PaperTradingState | undefined>(
  undefined,
);
export const PaperTradingActionContext = createContext<
  PaperTradingProviderActions | undefined
>(undefined);
