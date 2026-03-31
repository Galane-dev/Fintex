import type { AuthState } from "@/types/auth";
import { authActions, createStateFromSession, type AuthReducerAction } from "./actions";

export const initialAuthState: AuthState = {
  isReady: false,
  isAuthenticated: false,
  user: null,
};

export const authReducer = (state: AuthState, action: AuthReducerAction): AuthState => {
  switch (action.type) {
    case "HYDRATE_SESSION":
      return createStateFromSession(action.payload);
    default:
      return state;
  }
};

export { authActions };
