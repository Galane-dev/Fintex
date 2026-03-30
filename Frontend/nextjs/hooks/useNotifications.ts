"use client";

import { useContext } from "react";
import {
  NotificationsActionContext,
  NotificationsStateContext,
} from "@/providers/notifications-provider/context";

export const useNotifications = () => {
  const stateContext = useContext(NotificationsStateContext);
  const actionContext = useContext(NotificationsActionContext);

  if (!stateContext || !actionContext) {
    throw new Error("useNotifications must be used within a NotificationsProvider.");
  }

  return {
    ...stateContext,
    ...actionContext,
  };
};
