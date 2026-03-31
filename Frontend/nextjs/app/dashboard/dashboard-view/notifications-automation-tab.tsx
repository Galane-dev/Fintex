"use client";

import { DeleteOutlined } from "@ant-design/icons";
import { Alert, Button, Collapse, Empty, Form, Input, InputNumber, List, Select, Space, Switch, Typography } from "antd";
import { useEffect, useMemo, useState } from "react";
import type { TradeAutomationRule, TradeAutomationTriggerType } from "@/types/trade-automation";
import { formatPrice } from "@/utils/market-data";

export type AutomationExecutionTargetOption = {
  label: string;
  value: string;
};

type CreateTradeAutomationValues = {
  name: string;
  symbol: string;
  triggerType: TradeAutomationTriggerType;
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
};

type NotificationsAutomationTabProps = {
  isOpen: boolean;
  isSaving: boolean;
  rules: TradeAutomationRule[];
  executionTargets: AutomationExecutionTargetOption[];
  onCreateRule: (values: CreateTradeAutomationValues) => Promise<boolean>;
  onDeleteRule: (ruleId: number) => void;
};

const triggerOptions = [
  { label: "Price cross", value: "PriceTarget" },
  { label: "RSI level", value: "RelativeStrengthIndex" },
  { label: "MACD histogram", value: "MacdHistogram" },
  { label: "Momentum", value: "Momentum" },
  { label: "Trend score", value: "TrendScore" },
  { label: "Confidence score", value: "ConfidenceScore" },
  { label: "Verdict", value: "Verdict" },
] as const;

const triggerLabels: Record<TradeAutomationTriggerType, string> = {
  PriceTarget: "Price cross",
  RelativeStrengthIndex: "RSI level",
  MacdHistogram: "MACD histogram",
  Momentum: "Momentum",
  TrendScore: "Trend score",
  ConfidenceScore: "Confidence score",
  Verdict: "Verdict",
};

export function NotificationsAutomationTab({
  isOpen,
  isSaving,
  rules,
  executionTargets,
  onCreateRule,
  onDeleteRule,
}: NotificationsAutomationTabProps) {
  const [form] = Form.useForm<CreateTradeAutomationValues>();
  const [isExpanded, setIsExpanded] = useState(false);
  const selectedTriggerType = Form.useWatch("triggerType", form);
  const isVerdictRule = selectedTriggerType === "Verdict";
  const defaultExecutionTarget = executionTargets[0]?.value;
  const hasExecutionTarget = executionTargets.length > 0;

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    form.setFieldsValue({
      name: "BTC auto execution",
      symbol: "BTCUSDT",
      triggerType: "PriceTarget",
      executionTarget: defaultExecutionTarget,
      tradeDirection: "Buy",
      quantity: 0.01,
      notifyInApp: true,
      notifyEmail: true,
    });
  }, [defaultExecutionTarget, form, isOpen]);

  const triggerValueLabel = useMemo(() => {
    switch (selectedTriggerType) {
      case "RelativeStrengthIndex":
        return "RSI trigger";
      case "MacdHistogram":
        return "MACD histogram trigger";
      case "Momentum":
        return "Momentum trigger";
      case "TrendScore":
        return "Trend score trigger";
      case "ConfidenceScore":
        return "Confidence trigger";
      default:
        return "Price trigger";
    }
  }, [selectedTriggerType]);

  const handleCreateRule = async (values: CreateTradeAutomationValues) => {
    const wasCreated = await onCreateRule(values);
    if (!wasCreated) {
      return;
    }

    form.resetFields();
    form.setFieldsValue({
      name: "BTC auto execution",
      symbol: "BTCUSDT",
      triggerType: "PriceTarget",
      executionTarget: defaultExecutionTarget,
      tradeDirection: "Buy",
      quantity: 0.01,
      notifyInApp: true,
      notifyEmail: true,
    });
    setIsExpanded(false);
  };

  return (
    <Space direction="vertical" size={18} style={{ width: "100%" }}>
      <Collapse
        activeKey={isExpanded ? ["create-automation"] : []}
        onChange={(keys) => setIsExpanded(Array.isArray(keys) ? keys.includes("create-automation") : keys === "create-automation")}
        items={[
          {
            key: "create-automation",
            label: "Create auto execution",
            children: (
              <Space direction="vertical" size={16} style={{ width: "100%" }}>
                <Alert
                  type="info"
                  showIcon
                  message="Auto executions are one-shot. When the trigger fires, Fintex attempts the trade automatically and then deactivates the rule."
                />

                {!hasExecutionTarget ? (
                  <Alert
                    type="warning"
                    showIcon
                    message="Create your paper academy account or connect Alpaca first so Fintex has somewhere to route the automated trade."
                  />
                ) : null}

                <Form form={form} layout="vertical" onFinish={handleCreateRule}>
                  <Space wrap align="start" style={{ width: "100%" }}>
                    <Form.Item name="name" label="Rule name" rules={[{ required: true }]}>
                      <Input placeholder="BTC oversold auto-buy" style={{ width: 220 }} />
                    </Form.Item>
                    <Form.Item name="symbol" label="Symbol" rules={[{ required: true }]}>
                      <Input style={{ width: 140 }} />
                    </Form.Item>
                    <Form.Item name="triggerType" label="Trigger" rules={[{ required: true }]}>
                      <Select options={triggerOptions.map((option) => ({ label: option.label, value: option.value }))} style={{ width: 180 }} />
                    </Form.Item>
                  </Space>

                  <Space wrap align="start" style={{ width: "100%" }}>
                    {isVerdictRule ? (
                      <>
                        <Form.Item name="targetVerdict" label="Verdict target" rules={[{ required: true }]}>
                          <Select options={[{ label: "Buy", value: "Buy" }, { label: "Sell", value: "Sell" }]} style={{ width: 180 }} />
                        </Form.Item>
                        <Form.Item name="minimumConfidenceScore" label="Minimum confidence">
                          <InputNumber min={0} max={100} style={{ width: 180 }} />
                        </Form.Item>
                      </>
                    ) : (
                      <Form.Item name="triggerValue" label={triggerValueLabel} rules={[{ required: true }]}>
                        <InputNumber style={{ width: 180 }} />
                      </Form.Item>
                    )}
                    <Form.Item name="executionTarget" label="Execution destination" rules={[{ required: true }]}>
                      <Select options={executionTargets} style={{ width: 220 }} />
                    </Form.Item>
                    <Form.Item name="tradeDirection" label="Trade side" rules={[{ required: true }]}>
                      <Select options={[{ label: "Buy", value: "Buy" }, { label: "Sell", value: "Sell" }]} style={{ width: 140 }} />
                    </Form.Item>
                    <Form.Item name="quantity" label="Quantity" rules={[{ required: true }]}>
                      <InputNumber min={0.00000001} style={{ width: 160 }} />
                    </Form.Item>
                  </Space>

                  <Space wrap align="start" style={{ width: "100%" }}>
                    <Form.Item name="stopLoss" label="Stop loss">
                      <InputNumber min={0.00000001} style={{ width: 160 }} />
                    </Form.Item>
                    <Form.Item name="takeProfit" label="Take profit">
                      <InputNumber min={0.00000001} style={{ width: 160 }} />
                    </Form.Item>
                    <Form.Item name="notifyInApp" label="In-app notice" valuePropName="checked">
                      <Switch />
                    </Form.Item>
                    <Form.Item name="notifyEmail" label="Email notice" valuePropName="checked">
                      <Switch />
                    </Form.Item>
                  </Space>

                  <Space wrap align="end" style={{ width: "100%" }}>
                    <Form.Item name="notes" label="Notes" style={{ flex: 1, minWidth: 240 }}>
                      <Input placeholder="Optional automation context" />
                    </Form.Item>
                    <Button type="primary" htmlType="submit" loading={isSaving} disabled={!hasExecutionTarget}>
                      Save auto execution
                    </Button>
                  </Space>
                </Form>
              </Space>
            ),
          },
        ]}
      />

      {rules.length === 0 ? (
        <Empty description="No auto-execution rules yet." />
      ) : (
        <List
          dataSource={rules}
          renderItem={(rule) => (
            <List.Item
              actions={[
                <Button key="delete" icon={<DeleteOutlined />} danger type="text" onClick={() => onDeleteRule(rule.id)} />,
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
                    {triggerLabels[rule.triggerType]} {rule.triggerType === "Verdict" ? `${rule.targetVerdict} @ confidence ${rule.minimumConfidenceScore ?? "any"}` : `cross ${rule.triggerValue != null ? formatPrice(rule.triggerValue) : "-"}`} | executes {rule.tradeDirection.toLowerCase()} {rule.quantity} via {rule.destination === "PaperTrading" ? "Paper academy" : "Alpaca"} | SL {rule.stopLoss != null ? formatPrice(rule.stopLoss) : "-"} | TP {rule.takeProfit != null ? formatPrice(rule.takeProfit) : "-"}
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
