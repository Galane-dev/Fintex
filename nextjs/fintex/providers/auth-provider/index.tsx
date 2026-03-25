"use client";

import { PropsWithChildren, useMemo, useSyncExternalStore } from "react";
import type { AuthState, SignInValues, SignUpValues } from "@/types/auth";
import { AuthContext } from "./context";
import { clearStoredSession, readStoredSession, storeSession, AUTH_CHANGE_EVENT } from "@/utils/auth-storage";

const initialState: AuthState = {
  isReady: false,
  isAuthenticated: false,
  user: null,
};

let cachedState: AuthState = initialState;

const getAuthSnapshot = (): AuthState => {
  const session = readStoredSession();

  const nextState: AuthState = session
    ? {
        isReady: true,
        isAuthenticated: true,
        user: session.user,
      }
    : {
        isReady: true,
        isAuthenticated: false,
        user: null,
      };

  if (
    cachedState.isReady === nextState.isReady &&
    cachedState.isAuthenticated === nextState.isAuthenticated &&
    cachedState.user?.email === nextState.user?.email &&
    cachedState.user?.firstName === nextState.user?.firstName &&
    cachedState.user?.lastName === nextState.user?.lastName
  ) {
    return cachedState;
  }

  cachedState = nextState;
  return cachedState;
};

export function AuthProvider({ children }: PropsWithChildren) {
  const state = useSyncExternalStore(
    (callback) => {
      if (typeof window === "undefined") {
        return () => undefined;
      }

      const onChange = () => callback();

      window.addEventListener("storage", onChange);
      window.addEventListener(AUTH_CHANGE_EVENT, onChange);

      return () => {
        window.removeEventListener("storage", onChange);
        window.removeEventListener(AUTH_CHANGE_EVENT, onChange);
      };
    },
    getAuthSnapshot,
    () => initialState,
  );

  const signIn = async (values: SignInValues) => {
    const emailName = values.email.split("@")[0] ?? "trader";
    const firstName = emailName.charAt(0).toUpperCase() + emailName.slice(1);
    const user = { email: values.email, firstName };

    storeSession(user);
  };

  const signUp = async (values: SignUpValues) => {
    const user = {
      email: values.email,
      firstName: values.firstName,
      lastName: values.lastName,
    };

    storeSession(user);
  };

  const signOut = () => {
    clearStoredSession();
  };

  const value = useMemo(
    () => ({
      ...state,
      signIn,
      signUp,
      signOut,
    }),
    [state],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
