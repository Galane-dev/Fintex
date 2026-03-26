"use client";

import { useContext } from "react";
import { AuthActionContext, AuthStateContext } from "@/providers/auth-provider/context";

export const useAuth = () => {
  const stateContext = useContext(AuthStateContext);
  const actionContext = useContext(AuthActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
