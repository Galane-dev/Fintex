import type { AuthSession, AuthState } from "@/types/auth";

export type AuthReducerAction =
  | { type: "HYDRATE_SESSION"; payload: AuthSession | null };

export const createStateFromSession = (session: AuthSession | null): AuthState => ({
  isReady: true,
  isAuthenticated: Boolean(session),
  user: session?.user ?? null,
});

export const authActions = {
  hydrateSession: (session: AuthSession | null): AuthReducerAction => ({
    type: "HYDRATE_SESSION",
    payload: session,
  }),
};
