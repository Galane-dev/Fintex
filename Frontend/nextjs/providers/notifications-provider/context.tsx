"use client";

import { createContext } from "react";
import type {
  NotificationsProviderActions,
  NotificationsState,
} from "@/types/notifications";

export const NotificationsStateContext = createContext<NotificationsState | null>(null);
export const NotificationsActionContext =
  createContext<NotificationsProviderActions | null>(null);
