export type NotificationSeverity = "Info" | "Success" | "Warning" | "Danger";

export type NotificationType = "TradeOpportunity" | "PriceTarget" | "TradeAutomation";

export type NotificationItem = {
  id: number;
  type: NotificationType;
  severity: NotificationSeverity;
  title: string;
  message: string;
  symbol: string;
  provider: string;
  referencePrice: number | null;
  targetPrice: number | null;
  confidenceScore: number | null;
  verdict: string | null;
  isRead: boolean;
  emailSent: boolean;
  emailError: string | null;
  occurredAt: string;
};

export type NotificationAlertRule = {
  id: number;
  name: string;
  symbol: string;
  provider: string;
  alertType: string;
  createdPrice: number | null;
  lastObservedPrice: number | null;
  targetPrice: number;
  notifyInApp: boolean;
  notifyEmail: boolean;
  isActive: boolean;
  notes: string | null;
  creationTime: string;
  lastTriggeredAt: string | null;
};

export type NotificationInbox = {
  unreadCount: number;
  notifications: NotificationItem[];
  alertRules: NotificationAlertRule[];
};

export type CreatePriceAlertInput = {
  name: string;
  symbol: string;
  provider: number;
  targetPrice: number;
  notifyInApp: boolean;
  notifyEmail: boolean;
  notes?: string;
};

export type NotificationsState = {
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
  unreadCount: number;
  notifications: NotificationItem[];
  alertRules: NotificationAlertRule[];
};

export type NotificationsProviderActions = {
  refreshInbox: () => Promise<void>;
  createPriceAlert: (input: CreatePriceAlertInput) => Promise<boolean>;
  sendTestAlert: () => Promise<boolean>;
  deleteAlertRule: (ruleId: number) => Promise<void>;
  markAsRead: (notificationId: number) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  clearError: () => void;
};
