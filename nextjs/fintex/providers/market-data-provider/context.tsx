"use client";

import { createContext } from "react";
import type { MarketDataProviderActions, MarketDataState } from "@/types/market-data";

export const MarketDataStateContext = createContext<MarketDataState | undefined>(undefined);
export const MarketDataActionContext = createContext<MarketDataProviderActions | undefined>(undefined);
