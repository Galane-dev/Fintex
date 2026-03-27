"use client";

import { useCallback, useEffect, useMemo, useReducer, useRef } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { notification } from "antd";
import type { NotificationsProviderActions } from "@/types/notifications";
import { getApiBaseUrl } from "@/utils/api-config";
import { readStoredSession } from "@/utils/auth-storage";
import {
  createPriceAlert,
  deleteAlertRule,
  getMyNotificationInbox,
  markAllNotificationsAsRead,
  markNotificationAsRead,
  sendTestAlert,
} from "@/utils/notifications-api";
import { normalizeNotification } from "@/utils/notifications";
import { notificationsActions } from "./actions";
import { initialNotificationsState, notificationsReducer } from "./reducer";

const NOTIFICATIONS_REFRESH_MS = 60_000;

const buildHubUrl = (encryptedToken: string) =>
  `${getApiBaseUrl()}/signalr/market-data?enc_auth_token=${encodeURIComponent(encryptedToken)}`;

export const useNotificationsProvider = () => {
  const [state, dispatch] = useReducer(notificationsReducer, initialNotificationsState);
  const connectionRef = useRef<HubConnection | null>(null);

  const refreshInbox = useCallback(async () => {
    dispatch(notificationsActions.loadStart());

    try {
      const inbox = await getMyNotificationInbox();
      dispatch(notificationsActions.loadSuccess(inbox.unreadCount, inbox.notifications, inbox.alertRules));
    } catch (error) {
      dispatch(
        notificationsActions.loadFailure(
          error instanceof Error ? error.message : "We could not refresh your notifications.",
        ),
      );
    }
  }, []);

  const runSavingAction = useCallback(async (action: () => Promise<void>) => {
    dispatch(notificationsActions.saveStart());

    try {
      await action();
      await refreshInbox();
      dispatch(notificationsActions.saveDone());
    } catch (error) {
      dispatch(
        notificationsActions.loadFailure(
          error instanceof Error ? error.message : "We could not update your notification settings.",
        ),
      );
      throw error;
    }
  }, [refreshInbox]);

  useEffect(() => {
    void refreshInbox();
  }, [refreshInbox]);

  useEffect(() => {
    const session = readStoredSession();
    if (!session?.encryptedToken) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(buildHubUrl(session.encryptedToken))
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on("notificationCreated", (payload: Record<string, unknown>) => {
      const item = normalizeNotification(payload);
      dispatch(notificationsActions.liveNotification(item));
      notification.open({
        message: item.title,
        description: item.message,
        placement: "topRight",
      });
    });

    const startConnection = async () => {
      try {
        await connection.start();
      } catch {
        // Keep the notification center usable even when live push is unavailable.
      }
    };

    void startConnection();

    return () => {
      const activeConnection = connectionRef.current;
      connectionRef.current = null;

      if (activeConnection) {
        void activeConnection.stop();
      }
    };
  }, []);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refreshInbox();
    }, NOTIFICATIONS_REFRESH_MS);

    return () => {
      window.clearInterval(interval);
    };
  }, [refreshInbox]);

  const actionValues = useMemo<NotificationsProviderActions>(
    () => ({
      refreshInbox,
      createPriceAlert: async (input) => {
        try {
          await runSavingAction(async () => {
            await createPriceAlert(input);
          });
          return true;
        } catch {
          return false;
        }
      },
      sendTestAlert: async () => {
        try {
          await runSavingAction(async () => {
            await sendTestAlert();
          });
          return true;
        } catch {
          return false;
        }
      },
      deleteAlertRule: async (ruleId) => {
        await runSavingAction(async () => {
          await deleteAlertRule(ruleId);
        });
      },
      markAsRead: async (notificationId) => {
        await markNotificationAsRead(notificationId);
        dispatch(notificationsActions.markAsRead(notificationId));
      },
      markAllAsRead: async () => {
        await markAllNotificationsAsRead();
        dispatch(notificationsActions.markAllAsRead());
      },
      clearError: () => dispatch(notificationsActions.clearError()),
    }),
    [refreshInbox, runSavingAction],
  );

  return { state, actionValues };
};
