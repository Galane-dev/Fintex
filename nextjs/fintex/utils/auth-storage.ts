import type { AuthSession } from "@/types/auth";

export const AUTH_STORAGE_KEY = "fintex.auth.session";
export const AUTH_CHANGE_EVENT = "fintex-auth-change";

const hasWindow = () => typeof window !== "undefined";

export const readStoredSession = (): AuthSession | null => {
  if (!hasWindow()) {
    return null;
  }

  const raw = window.localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthSession;
  } catch {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
};

export const storeSession = (session: AuthSession) => {
  if (!hasWindow()) {
    return;
  }

  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
  window.dispatchEvent(new Event(AUTH_CHANGE_EVENT));
};

export const clearStoredSession = () => {
  if (!hasWindow()) {
    return;
  }

  window.localStorage.removeItem(AUTH_STORAGE_KEY);
  window.dispatchEvent(new Event(AUTH_CHANGE_EVENT));
};
