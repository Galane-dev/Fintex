"use client";

import { BellOutlined } from "@ant-design/icons";
import { Alert, Badge, Modal, Space, Tabs } from "antd";
import type { NotificationAlertRule, NotificationItem } from "@/types/notifications";
import type { TradeAutomationRule } from "@/types/trade-automation";
import { NotificationsAlertsTab } from "./notifications-alerts-tab";
import { NotificationsAutomationTab, type AutomationExecutionTargetOption } from "./notifications-automation-tab";
import { NotificationsInboxTab } from "./notifications-inbox-tab";

type NotificationsModalProps = {
  isOpen: boolean;
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
  unreadCount: number;
  notifications: NotificationItem[];
  alertRules: NotificationAlertRule[];
  automationRules: TradeAutomationRule[];
  automationExecutionTargets: AutomationExecutionTargetOption[];
  onClose: () => void;
  onClearError: () => void;
  onMarkAsRead: (notificationId: number) => void;
  onMarkAllAsRead: () => void;
  onDeleteAlertRule: (ruleId: number) => void;
  onCreatePriceAlert: (values: {
    name: string;
    symbol: string;
    targetPrice: number;
    notifyInApp: boolean;
    notifyEmail: boolean;
    notes?: string;
  }) => Promise<boolean>;
  onCreateAutomationRule: (values: {
    name: string;
    symbol: string;
    triggerType: "PriceTarget" | "RelativeStrengthIndex" | "MacdHistogram" | "Momentum" | "TrendScore" | "ConfidenceScore" | "Verdict";
    triggerValue?: number;
    targetVerdict?: "Buy" | "Sell";
    minimumConfidenceScore?: number;
    executionTarget: string;
    tradeDirection: "Buy" | "Sell";
    quantity: number;
    stopLoss?: number;
    takeProfit?: number;
    notifyInApp: boolean;
    notifyEmail: boolean;
    notes?: string;
  }) => Promise<boolean>;
  onSendTestAlert: () => Promise<boolean>;
  onDeleteAutomationRule: (ruleId: number) => void;
};

export function NotificationsModal({
  isOpen,
  isLoading,
  isSaving,
  error,
  unreadCount,
  notifications,
  alertRules,
  automationRules,
  automationExecutionTargets,
  onClose,
  onClearError,
  onMarkAsRead,
  onMarkAllAsRead,
  onDeleteAlertRule,
  onCreatePriceAlert,
  onCreateAutomationRule,
  onSendTestAlert,
  onDeleteAutomationRule,
}: NotificationsModalProps) {
  return (
    <Modal
      open={isOpen}
      onCancel={onClose}
      footer={null}
      width={760}
      title={<Space><BellOutlined /><span>Notifications</span><Badge count={unreadCount} /></Space>}
    >
      {error ? <Alert type="warning" showIcon closable onClose={onClearError} message={error} style={{ marginBottom: 16 }} /> : null}

      <Tabs
        items={[
          {
            key: "inbox",
            label: `Inbox (${notifications.length})`,
            children: <NotificationsInboxTab isLoading={isLoading} unreadCount={unreadCount} notifications={notifications} onMarkAsRead={onMarkAsRead} onMarkAllAsRead={onMarkAllAsRead} />,
          },
          {
            key: "alerts",
            label: `Alerts (${alertRules.length})`,
            children: <NotificationsAlertsTab isOpen={isOpen} isSaving={isSaving} alertRules={alertRules} onCreatePriceAlert={onCreatePriceAlert} onSendTestAlert={onSendTestAlert} onDeleteAlertRule={onDeleteAlertRule} />,
          },
          {
            key: "automation",
            label: `Auto execute (${automationRules.length})`,
            children: <NotificationsAutomationTab isOpen={isOpen} isSaving={isSaving} rules={automationRules} executionTargets={automationExecutionTargets} onCreateRule={onCreateAutomationRule} onDeleteRule={onDeleteAutomationRule} />,
          },
        ]}
      />
    </Modal>
  );
}
