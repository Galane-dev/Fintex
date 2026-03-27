import type { NotificationAlertRule, NotificationItem, NotificationsState } from "@/types/notifications";

export type NotificationsReducerAction =
  | { type: "LOAD_START" }
  | { type: "LOAD_SUCCESS"; payload: { unreadCount: number; notifications: NotificationItem[]; alertRules: NotificationAlertRule[] } }
  | { type: "LOAD_FAILURE"; payload: string }
  | { type: "SAVE_START" }
  | { type: "SAVE_DONE" }
  | { type: "LIVE_NOTIFICATION"; payload: NotificationItem }
  | { type: "MARK_AS_READ"; payload: number }
  | { type: "MARK_ALL_AS_READ" }
  | { type: "CLEAR_ERROR" };

export const createInitialNotificationsState = (): NotificationsState => ({
  isLoading: true,
  isSaving: false,
  error: null,
  unreadCount: 0,
  notifications: [],
  alertRules: [],
});

export const notificationsActions = {
  loadStart: (): NotificationsReducerAction => ({ type: "LOAD_START" }),
  loadSuccess: (
    unreadCount: number,
    notifications: NotificationItem[],
    alertRules: NotificationAlertRule[],
  ): NotificationsReducerAction => ({
    type: "LOAD_SUCCESS",
    payload: { unreadCount, notifications, alertRules },
  }),
  loadFailure: (payload: string): NotificationsReducerAction => ({ type: "LOAD_FAILURE", payload }),
  saveStart: (): NotificationsReducerAction => ({ type: "SAVE_START" }),
  saveDone: (): NotificationsReducerAction => ({ type: "SAVE_DONE" }),
  liveNotification: (payload: NotificationItem): NotificationsReducerAction => ({ type: "LIVE_NOTIFICATION", payload }),
  markAsRead: (payload: number): NotificationsReducerAction => ({ type: "MARK_AS_READ", payload }),
  markAllAsRead: (): NotificationsReducerAction => ({ type: "MARK_ALL_AS_READ" }),
  clearError: (): NotificationsReducerAction => ({ type: "CLEAR_ERROR" }),
};
