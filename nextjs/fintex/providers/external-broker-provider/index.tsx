"use client";

import { PropsWithChildren } from "react";
import {
  ExternalBrokerActionContext,
  ExternalBrokerStateContext,
} from "./context";
import { useExternalBrokerProvider } from "./use-external-broker-provider";

export function ExternalBrokerProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = useExternalBrokerProvider();

  return (
    <ExternalBrokerStateContext.Provider value={state}>
      <ExternalBrokerActionContext.Provider value={actionValues}>
        {children}
      </ExternalBrokerActionContext.Provider>
    </ExternalBrokerStateContext.Provider>
  );
}
