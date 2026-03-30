"use client";

import { useContext } from "react";
import {
  ExternalBrokerActionContext,
  ExternalBrokerStateContext,
} from "@/providers/external-broker-provider/context";

export const useExternalBrokerAccounts = () => {
  const stateContext = useContext(ExternalBrokerStateContext);
  const actionContext = useContext(ExternalBrokerActionContext);

  if (!stateContext || !actionContext) {
    throw new Error(
      "useExternalBrokerAccounts must be used within an ExternalBrokerProvider.",
    );
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
