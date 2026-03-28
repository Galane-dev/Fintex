import type {
  NotificationAlertRule,
  NotificationInbox,
  NotificationItem,
  NotificationSeverity,
  NotificationType,
} from "@/types/notifications";

const parseNumber = (value: unknown) =>
  typeof value === "number"
    ? value
    : typeof value === "string" && value.trim() !== ""
      ? Number(value)
      : null;

const mapSeverity = (value: unknown): NotificationSeverity => {
  if (value === 2 || value === "Success") {
    return "Success";
  }

  if (value === 3 || value === "Warning") {
    return "Warning";
  }

  if (value === 4 || value === "Danger") {
    return "Danger";
  }

  return "Info";
};

const mapType = (value: unknown): NotificationType =>
  value === 2 || value === "PriceTarget"
    ? "PriceTarget"
    : value === 3 || value === "TradeAutomation"
      ? "TradeAutomation"
      : "TradeOpportunity";

export const normalizeNotification = (payload: Record<string, unknown>): NotificationItem => ({
  id: Number(payload.id ?? payload.Id ?? 0),
  type: mapType(payload.type ?? payload.Type),
  severity: mapSeverity(payload.severity ?? payload.Severity),
  title: String(payload.title ?? payload.Title ?? "Notification"),
  message: String(payload.message ?? payload.Message ?? ""),
  symbol: String(payload.symbol ?? payload.Symbol ?? "BTCUSDT"),
  provider: String(payload.provider ?? payload.Provider ?? "Binance"),
  referencePrice: parseNumber(payload.referencePrice ?? payload.ReferencePrice),
  targetPrice: parseNumber(payload.targetPrice ?? payload.TargetPrice),
  confidenceScore: parseNumber(payload.confidenceScore ?? payload.ConfidenceScore),
  verdict: typeof (payload.verdict ?? payload.Verdict) === "string" ? String(payload.verdict ?? payload.Verdict) : null,
  isRead: Boolean(payload.isRead ?? payload.IsRead),
  emailSent: Boolean(payload.emailSent ?? payload.EmailSent),
  emailError: typeof (payload.emailError ?? payload.EmailError) === "string" ? String(payload.emailError ?? payload.EmailError) : null,
  occurredAt: String(payload.occurredAt ?? payload.OccurredAt ?? new Date().toISOString()),
});

export const normalizeAlertRule = (payload: Record<string, unknown>): NotificationAlertRule => ({
  id: Number(payload.id ?? payload.Id ?? 0),
  name: String(payload.name ?? payload.Name ?? "Price alert"),
  symbol: String(payload.symbol ?? payload.Symbol ?? "BTCUSDT"),
  provider: String(payload.provider ?? payload.Provider ?? "Binance"),
  alertType: String(payload.alertType ?? payload.AlertType ?? "PriceTarget"),
  createdPrice: parseNumber(payload.createdPrice ?? payload.CreatedPrice),
  lastObservedPrice: parseNumber(payload.lastObservedPrice ?? payload.LastObservedPrice),
  targetPrice: Number(payload.targetPrice ?? payload.TargetPrice ?? 0),
  notifyInApp: Boolean(payload.notifyInApp ?? payload.NotifyInApp),
  notifyEmail: Boolean(payload.notifyEmail ?? payload.NotifyEmail),
  isActive: Boolean(payload.isActive ?? payload.IsActive),
  notes: typeof (payload.notes ?? payload.Notes) === "string" ? String(payload.notes ?? payload.Notes) : null,
  creationTime: String(payload.creationTime ?? payload.CreationTime ?? new Date().toISOString()),
  lastTriggeredAt:
    typeof (payload.lastTriggeredAt ?? payload.LastTriggeredAt) === "string"
      ? String(payload.lastTriggeredAt ?? payload.LastTriggeredAt)
      : null,
});

export const normalizeNotificationInbox = (payload: Record<string, unknown>): NotificationInbox => {
  const notificationsPayload = payload.notifications ?? payload.Notifications;
  const alertRulesPayload = payload.alertRules ?? payload.AlertRules;
  const notificationItems = Array.isArray((notificationsPayload as { items?: unknown[] } | undefined)?.items)
    ? ((notificationsPayload as { items: unknown[] }).items)
    : Array.isArray(notificationsPayload)
      ? notificationsPayload
      : [];
  const alertRuleItems = Array.isArray((alertRulesPayload as { items?: unknown[] } | undefined)?.items)
    ? ((alertRulesPayload as { items: unknown[] }).items)
    : Array.isArray(alertRulesPayload)
      ? alertRulesPayload
      : [];

  return {
    unreadCount: Number(payload.unreadCount ?? payload.UnreadCount ?? 0),
    notifications: notificationItems.map((item) => normalizeNotification(item as Record<string, unknown>)),
    alertRules: alertRuleItems.map((item) => normalizeAlertRule(item as Record<string, unknown>)),
  };
};
