"use client";

import { createContext } from "react";
import type { AuthProviderActions, AuthState } from "@/types/auth";

export const AuthStateContext = createContext<AuthState | undefined>(undefined);
export const AuthActionContext = createContext<AuthProviderActions | undefined>(undefined);
