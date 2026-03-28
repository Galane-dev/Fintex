"use client";

import { Button, Empty, List, Space, Tag, Typography } from "antd";
import type { NotificationItem } from "@/types/notifications";

type NotificationsInboxTabProps = {
  isLoading: boolean;
  unreadCount: number;
  notifications: NotificationItem[];
  onMarkAsRead: (notificationId: number) => void;
  onMarkAllAsRead: () => void;
};

const severityColorMap: Record<string, string> = {
  Danger: "red",
  Warning: "orange",
  Success: "green",
  Info: "blue",
};

const formatDateTime = (value: string) =>
  new Date(value).toLocaleString("en-ZA", {
    hour: "2-digit",
    minute: "2-digit",
    day: "2-digit",
    month: "short",
  });

export function NotificationsInboxTab({
  isLoading,
  unreadCount,
  notifications,
  onMarkAsRead,
  onMarkAllAsRead,
}: NotificationsInboxTabProps) {
  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Space style={{ justifyContent: "space-between", width: "100%" }}>
        <Typography.Text type="secondary">
          High-confidence opportunities, triggered price alerts, and executed automations arrive here.
        </Typography.Text>
        <Button onClick={() => onMarkAllAsRead()} disabled={unreadCount === 0}>
          Mark all as read
        </Button>
      </Space>

      {notifications.length === 0 ? (
        <Empty description="No notifications yet." />
      ) : (
        <List
          loading={isLoading}
          dataSource={notifications}
          renderItem={(item) => (
            <List.Item
              actions={[
                item.isRead ? null : (
                  <Button key="read" type="link" onClick={() => onMarkAsRead(item.id)}>
                    Mark as read
                  </Button>
                ),
              ].filter(Boolean)}
            >
              <List.Item.Meta
                title={(
                  <Space wrap>
                    <Typography.Text strong>{item.title}</Typography.Text>
                    <Tag color={severityColorMap[item.severity] ?? "blue"}>{item.severity}</Tag>
                    {!item.isRead ? <Tag color="gold">Unread</Tag> : null}
                  </Space>
                )}
                description={(
                  <Space direction="vertical" size={4}>
                    <Typography.Text>{item.message}</Typography.Text>
                    <Typography.Text type="secondary">
                      {item.symbol} | {formatDateTime(item.occurredAt)}
                      {item.confidenceScore != null ? ` | confidence ${item.confidenceScore.toFixed(1)}` : ""}
                    </Typography.Text>
                  </Space>
                )}
              />
            </List.Item>
          )}
        />
      )}
    </Space>
  );
}
