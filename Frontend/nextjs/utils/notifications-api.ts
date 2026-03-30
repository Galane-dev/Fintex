import type { CreatePriceAlertInput, NotificationInbox } from "@/types/notifications";
import { getAxiosInstance } from "./axios-instance";
import { normalizeNotificationInbox } from "./notifications";

export const getMyNotificationInbox = async (): Promise<NotificationInbox> => {
  const response = await getAxiosInstance().get("/api/services/app/Notification/GetMyInbox", {
    params: { maxResultCount: 25, unreadOnly: false },
  });

  return normalizeNotificationInbox(response.data.result ?? response.data);
};

export const createPriceAlert = async (input: CreatePriceAlertInput) => {
  await getAxiosInstance().post("/api/services/app/Notification/CreatePriceAlert", input);
};

export const sendTestAlert = async () => {
  await getAxiosInstance().post("/api/services/app/Notification/SendTestAlert");
};

export const deleteAlertRule = async (ruleId: number) => {
  await getAxiosInstance().post("/api/services/app/Notification/DeleteAlertRule", { id: ruleId });
};

export const markNotificationAsRead = async (notificationId: number) => {
  await getAxiosInstance().post("/api/services/app/Notification/MarkAsRead", { id: notificationId });
};

export const markAllNotificationsAsRead = async () => {
  await getAxiosInstance().post("/api/services/app/Notification/MarkAllAsRead");
};
