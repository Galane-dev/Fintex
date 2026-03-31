"use client";

import { useEffect } from "react";
import { Alert, Button, Form, Input, Segmented, Skeleton, Space, Tag, Typography } from "antd";
import { DashboardDrawerShell } from "@/components/dashboard/dashboard-drawer-shell";
import type { StrategyValidationResult, ValidateStrategyInput } from "@/types/strategy-validation";
import { formatPrice, formatTime } from "@/utils/market-data";

type StrategyValidationModalProps = {
  isOpen: boolean;
  isLoadingHistory: boolean;
  isSubmitting: boolean;
  error: string | null;
  latestResult: StrategyValidationResult | null;
  history: StrategyValidationResult[];
  onClose: () => void;
  onSubmit: (input: ValidateStrategyInput) => Promise<StrategyValidationResult | null>;
};

const defaultValues: ValidateStrategyInput = {
  strategyName: "",
  symbol: "BTCUSDT",
  provider: 1,
  timeframe: "1m",
  directionPreference: "Buy",
  strategyText: "",
};

export function StrategyValidationModal({
  isOpen,
  isLoadingHistory,
  isSubmitting,
  error,
  latestResult,
  history,
  onClose,
  onSubmit,
}: StrategyValidationModalProps) {
  const [form] = Form.useForm<ValidateStrategyInput>();

  useEffect(() => {
    if (isOpen) {
      form.setFieldsValue(defaultValues);
    }
  }, [form, isOpen]);

  return (
    <DashboardDrawerShell open={isOpen} onClose={onClose} title="Validate strategy" width={860}>
      <Space direction="vertical" size={16} style={{ width: "100%" }}>
        {error ? <Alert type="warning" showIcon message={error} /> : null}

        <Form
          form={form}
          layout="vertical"
          initialValues={defaultValues}
          onFinish={() => {
            const values = form.getFieldsValue(true) as ValidateStrategyInput;
            void onSubmit({ ...defaultValues, ...values });
          }}
        >
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            <Form.Item label="Strategy name" name="strategyName">
              <Input placeholder="Breakout pullback, London open fade, RSI reversal..." />
            </Form.Item>
            <Space wrap style={{ width: "100%" }}>
              <Form.Item label="Timeframe" name="timeframe" style={{ marginBottom: 0 }}>
                <Segmented options={["1m", "5m", "15m", "1h", "4h"]} />
              </Form.Item>
              <Form.Item label="Direction bias" name="directionPreference" style={{ marginBottom: 0 }}>
                <Segmented options={["Buy", "Sell", "Both"]} />
              </Form.Item>
            </Space>
            <Form.Item
              label="Strategy details"
              name="strategyText"
              rules={[{ required: true, message: "Enter the strategy you want validated." }]}
            >
              <Input.TextArea
                placeholder="Describe the entry trigger, invalidation, stop loss, take profit, market conditions, and when you avoid the setup."
                autoSize={{ minRows: 6, maxRows: 12 }}
              />
            </Form.Item>
            <Button type="primary" htmlType="submit" loading={isSubmitting}>
              Validate strategy
            </Button>
          </Space>
        </Form>

        {isLoadingHistory && !latestResult ? <Skeleton active paragraph={{ rows: 8 }} /> : null}
        {latestResult ? <ValidationResultCard result={latestResult} /> : null}
        {history.length > 0 ? (
          <div>
            <Typography.Title level={5}>Recent validations</Typography.Title>
            <Space direction="vertical" size={10} style={{ width: "100%" }}>
              {history.map((item) => (
                <div key={item.id} style={{ border: "1px solid rgba(255,255,255,0.08)", borderRadius: 12, padding: 12 }}>
                  <Space wrap style={{ justifyContent: "space-between", width: "100%" }}>
                    <Typography.Text strong>{item.strategyName || "Unnamed strategy"}</Typography.Text>
                    <Tag color={getOutcomeColor(item.outcome)}>{item.outcome}</Tag>
                  </Space>
                  <Typography.Paragraph style={{ marginBottom: 8 }}>{item.summary}</Typography.Paragraph>
                  <Typography.Text type="secondary">
                    {item.symbol} · {item.timeframe || "1m"} · Score {item.validationScore.toFixed(1)} · {formatTime(item.creationTime)}
                  </Typography.Text>
                </div>
              ))}
            </Space>
          </div>
        ) : null}
      </Space>
    </DashboardDrawerShell>
  );
}

function ValidationResultCard({ result }: { result: StrategyValidationResult }) {
  return (
    <div style={{ border: "1px solid rgba(255,255,255,0.08)", borderRadius: 14, padding: 16 }}>
      <Space wrap style={{ justifyContent: "space-between", width: "100%" }}>
        <div>
          <Typography.Title level={4} style={{ marginBottom: 4 }}>
            {result.strategyName || "Latest validation"}
          </Typography.Title>
          <Typography.Text type="secondary">
            {result.symbol} · {result.timeframe || "1m"} · Market {result.marketVerdict || "Hold"}
          </Typography.Text>
        </div>
        <Space wrap>
          <Tag color={getOutcomeColor(result.outcome)}>{result.outcome}</Tag>
          <Tag color="blue">Score {result.validationScore.toFixed(1)}</Tag>
        </Space>
      </Space>

      <Typography.Paragraph style={{ marginTop: 12 }}>{result.summary}</Typography.Paragraph>
      <ListBlock title="Strengths" items={result.strengths} emptyText="No clear strengths were highlighted." />
      <ListBlock title="Risks" items={result.risks} emptyText="No major risks were highlighted." />
      <ListBlock title="Improvements" items={result.improvements} emptyText="No additional improvements were suggested." />

      <Space wrap style={{ marginTop: 12 }}>
        <Tag>Suggested action {result.suggestedAction || "Hold"}</Tag>
        <Tag>Entry {formatPrice(result.suggestedEntryPrice)}</Tag>
        <Tag>SL {formatPrice(result.suggestedStopLoss)}</Tag>
        <Tag>TP {formatPrice(result.suggestedTakeProfit)}</Tag>
      </Space>
    </div>
  );
}

function ListBlock({ title, items, emptyText }: { title: string; items: string[]; emptyText: string }) {
  return (
    <div style={{ marginTop: 12 }}>
      <Typography.Text strong>{title}</Typography.Text>
      {items.length > 0 ? (
        <ul style={{ marginTop: 8, paddingLeft: 18 }}>
          {items.map((item) => (
            <li key={`${title}-${item}`}>
              <Typography.Text>{item}</Typography.Text>
            </li>
          ))}
        </ul>
      ) : (
        <Typography.Paragraph type="secondary" style={{ marginTop: 6, marginBottom: 0 }}>
          {emptyText}
        </Typography.Paragraph>
      )}
    </div>
  );
}

function getOutcomeColor(outcome: StrategyValidationResult["outcome"]) {
  if (outcome === "Validated") {
    return "green";
  }

  return outcome === "Fail" ? "red" : "gold";
}
