import type { NotificationsState } from "@/types/notifications";
import {
  createInitialNotificationsState,
  type NotificationsReducerAction,
} from "./actions";

export const initialNotificationsState: NotificationsState =
  createInitialNotificationsState();

export const notificationsReducer = (
  state: NotificationsState,
  action: NotificationsReducerAction,
): NotificationsState => {
  switch (action.type) {
    case "LOAD_START":
      return { ...state, isLoading: true, error: null };
    case "LOAD_SUCCESS":
      return {
        ...state,
        isLoading: false,
        isSaving: false,
        error: null,
        unreadCount: action.payload.unreadCount,
        notifications: action.payload.notifications,
        alertRules: action.payload.alertRules,
      };
    case "LOAD_FAILURE":
      return { ...state, isLoading: false, isSaving: false, error: action.payload };
    case "SAVE_START":
      return { ...state, isSaving: true, error: null };
    case "SAVE_DONE":
      return { ...state, isSaving: false };
    case "LIVE_NOTIFICATION":
      return {
        ...state,
        unreadCount: state.unreadCount + 1,
        notifications: [action.payload, ...state.notifications.filter((item) => item.id !== action.payload.id)],
      };
    case "MARK_AS_READ":
      return {
        ...state,
        unreadCount: Math.max(0, state.unreadCount - (state.notifications.some((item) => item.id === action.payload && !item.isRead) ? 1 : 0)),
        notifications: state.notifications.map((item) => item.id === action.payload ? { ...item, isRead: true } : item),
      };
    case "MARK_ALL_AS_READ":
      return {
        ...state,
        unreadCount: 0,
        notifications: state.notifications.map((item) => ({ ...item, isRead: true })),
      };
    case "CLEAR_ERROR":
      return { ...state, error: null };
    default:
      return state;
  }
};
