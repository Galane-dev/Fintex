"use client";

import { createContext } from "react";
import type {
  ExternalBrokerProviderActions,
  ExternalBrokerState,
} from "@/types/external-broker";

export const ExternalBrokerStateContext = createContext<
  ExternalBrokerState | undefined
>(undefined);

export const ExternalBrokerActionContext = createContext<
  ExternalBrokerProviderActions | undefined
>(undefined);
