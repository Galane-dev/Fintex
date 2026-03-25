"use client";

import { PropsWithChildren, useCallback, useEffect, useMemo, useReducer } from "react";
import type { AuthProviderActions, SignInValues, SignUpValues } from "@/types/auth";
import { AuthActionContext, AuthStateContext } from "./context";
import { authActions, authReducer, initialAuthState } from "./reducer";
import {
  clearStoredSession,
  readStoredSession,
  storeSession,
  AUTH_CHANGE_EVENT,
} from "@/utils/auth-storage";
import { signInRequest, signUpRequest } from "@/utils/auth-api";
import { apiClient } from "@/utils/api-client";

export function AuthProvider({ children }: PropsWithChildren) {
  const [state, dispatch] = useReducer(authReducer, initialAuthState);

  useEffect(() => {
    const session = readStoredSession();

    if (session?.token) {
      apiClient.defaults.headers.common.Authorization = `Bearer ${session.token}`;
    } else {
      delete apiClient.defaults.headers.common.Authorization;
    }

    dispatch(authActions.hydrateSession(session));

    const syncSession = () => {
      const nextSession = readStoredSession();

      if (nextSession?.token) {
        apiClient.defaults.headers.common.Authorization = `Bearer ${nextSession.token}`;
      } else {
        delete apiClient.defaults.headers.common.Authorization;
      }

      dispatch(authActions.hydrateSession(nextSession));
    };

    window.addEventListener("storage", syncSession);
    window.addEventListener(AUTH_CHANGE_EVENT, syncSession);

    return () => {
      window.removeEventListener("storage", syncSession);
      window.removeEventListener(AUTH_CHANGE_EVENT, syncSession);
    };
  }, []);

  const signIn = useCallback(async (values: SignInValues) => {
    const session = await signInRequest(values);
    storeSession(session);
    dispatch(authActions.hydrateSession(session));
  }, []);

  const signUp = useCallback(async (values: SignUpValues) => {
    const session = await signUpRequest(values);
    storeSession(session);
    dispatch(authActions.hydrateSession(session));
  }, []);

  const signOut = useCallback(() => {
    delete apiClient.defaults.headers.common.Authorization;
    clearStoredSession();
    dispatch(authActions.hydrateSession(null));
  }, []);

  const actionValues = useMemo<AuthProviderActions>(
    () => ({
      signIn,
      signUp,
      signOut,
    }),
    [signIn, signOut, signUp],
  );

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider value={actionValues}>{children}</AuthActionContext.Provider>
    </AuthStateContext.Provider>
  );
}
