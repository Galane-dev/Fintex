"use client";

import { DeleteOutlined } from "@ant-design/icons";
import { Alert, Button, Collapse, Empty, Form, Input, InputNumber, List, Space, Switch, Typography } from "antd";
import { useEffect, useState } from "react";
import type { NotificationAlertRule } from "@/types/notifications";
import { formatPrice } from "@/utils/market-data";

type CreatePriceAlertValues = {
  name: string;
  symbol: string;
  targetPrice: number;
  notifyInApp: boolean;
  notifyEmail: boolean;
  notes?: string;
};

type NotificationsAlertsTabProps = {
  isOpen: boolean;
  isSaving: boolean;
  alertRules: NotificationAlertRule[];
  onCreatePriceAlert: (values: CreatePriceAlertValues) => Promise<boolean>;
  onSendTestAlert: () => Promise<boolean>;
  onDeleteAlertRule: (ruleId: number) => void;
};

export function NotificationsAlertsTab({
  isOpen,
  isSaving,
  alertRules,
  onCreatePriceAlert,
  onSendTestAlert,
  onDeleteAlertRule,
}: NotificationsAlertsTabProps) {
  const [form] = Form.useForm<CreatePriceAlertValues>();
  const [isCreateAlertExpanded, setIsCreateAlertExpanded] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    form.setFieldsValue({
      name: "BTC price alert",
      symbol: "BTCUSDT",
      notifyInApp: true,
      notifyEmail: true,
    });
  }, [form, isOpen]);

  const handleCreatePriceAlert = async (values: CreatePriceAlertValues) => {
    const wasCreated = await onCreatePriceAlert(values);
    if (!wasCreated) {
      return;
    }

    form.resetFields();
    form.setFieldsValue({
      name: "BTC price alert",
      symbol: "BTCUSDT",
      notifyInApp: true,
      notifyEmail: true,
    });
    setIsCreateAlertExpanded(false);
  };

  return (
    <Space direction="vertical" size={18} style={{ width: "100%" }}>
      <Collapse
        activeKey={isCreateAlertExpanded ? ["create-alert"] : []}
        onChange={(keys) => setIsCreateAlertExpanded(Array.isArray(keys) ? keys.includes("create-alert") : keys === "create-alert")}
        items={[
          {
            key: "create-alert",
            label: "Create price alert",
            children: (
              <Space direction="vertical" size={16} style={{ width: "100%" }}>
                <Alert
                  type="info"
                  showIcon
                  message="Price alerts trigger once the live market moves through your target between one price update and the next."
                />

                <Space style={{ justifyContent: "space-between", width: "100%" }} wrap>
                  <Typography.Text type="secondary">
                    Use a test alert to verify in-app notifications and email delivery before debugging market-crossing rules.
                  </Typography.Text>
                  <Button loading={isSaving} onClick={() => void onSendTestAlert()}>
                    Send alert test
                  </Button>
                </Space>

                <Form form={form} layout="vertical" onFinish={handleCreatePriceAlert}>
                  <Space align="start" wrap style={{ width: "100%" }}>
                    <Form.Item name="name" label="Alert name" rules={[{ required: true }]}>
                      <Input placeholder="BTC breakout alert" style={{ width: 220 }} />
                    </Form.Item>
                    <Form.Item name="symbol" label="Symbol" rules={[{ required: true }]}>
                      <Input style={{ width: 140 }} />
                    </Form.Item>
                    <Form.Item name="targetPrice" label="Target price" rules={[{ required: true }]}>
                      <InputNumber min={0.00000001} style={{ width: 180 }} />
                    </Form.Item>
                  </Space>

                  <Space align="center" wrap style={{ marginBottom: 12 }}>
                    <Form.Item name="notifyInApp" label="In-app alert" valuePropName="checked" style={{ marginBottom: 0 }}>
                      <Switch />
                    </Form.Item>
                    <Form.Item name="notifyEmail" label="Email alert" valuePropName="checked" style={{ marginBottom: 0 }}>
                      <Switch />
                    </Form.Item>
                    <Form.Item name="notes" label="Notes" style={{ marginBottom: 0 }}>
                      <Input placeholder="Optional context" style={{ width: 220 }} />
                    </Form.Item>
                    <Button type="primary" htmlType="submit" loading={isSaving} style={{ marginTop: 30 }}>
                      Create alert
                    </Button>
                  </Space>
                </Form>
              </Space>
            ),
          },
        ]}
      />

      {alertRules.length === 0 ? (
        <Empty description="No active alerts yet." />
      ) : (
        <List
          dataSource={alertRules}
          renderItem={(rule) => (
            <List.Item
              actions={[
                <Button key="delete" icon={<DeleteOutlined />} danger type="text" onClick={() => onDeleteAlertRule(rule.id)} />,
              ]}
            >
              <List.Item.Meta
                title={(
                  <Space wrap>
                    <Typography.Text strong>{rule.name}</Typography.Text>
                    <Typography.Text type="secondary">{rule.symbol}</Typography.Text>
                  </Space>
                )}
                description={(
                  <Typography.Text type="secondary">
                    {(rule.lastObservedPrice ?? rule.createdPrice) != null
                      ? `Tracking from ${formatPrice(rule.lastObservedPrice ?? rule.createdPrice ?? 0)}`
                      : "Legacy alert rule"} | target {formatPrice(rule.targetPrice)} | in-app {rule.notifyInApp ? "on" : "off"} | email {rule.notifyEmail ? "on" : "off"}
                  </Typography.Text>
                )}
              />
            </List.Item>
          )}
        />
      )}
    </Space>
  );
}
