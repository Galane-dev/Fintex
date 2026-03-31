"use client";

import { PropsWithChildren } from "react";
import { NotificationsActionContext, NotificationsStateContext } from "./context";
import { useNotificationsProvider } from "./use-notifications-provider";

export function NotificationsProvider({ children }: PropsWithChildren) {
  const { state, actionValues } = useNotificationsProvider();

  return (
    <NotificationsStateContext.Provider value={state}>
      <NotificationsActionContext.Provider value={actionValues}>
        {children}
      </NotificationsActionContext.Provider>
    </NotificationsStateContext.Provider>
  );
}
