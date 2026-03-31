"use client";

import { Alert, Button, Empty, Form, Input, InputNumber, List, Progress, Select, Space, Switch, Tag, Typography } from "antd";
import { useEffect } from "react";
import type { GoalStatus, GoalTarget, GoalTradingSession } from "@/types/goal-automation";

export type GoalExecutionTargetOption = {
  label: string;
  value: string;
};

type CreateGoalValues = {
  name?: string;
  executionTarget: string;
  targetType: "PercentGrowth" | "TargetAmount";
  targetPercent?: number;
  targetAmount?: number;
  deadlineLocal: string;
  maxAcceptableRisk: number;
  maxDrawdownPercent: number;
  maxPositionSizePercent: number;
  tradingSession: GoalTradingSession;
  allowOvernightPositions: boolean;
};

type TargetsTabProps = {
  isSaving: boolean;
  error: string | null;
  goals: GoalTarget[];
  executionTargets: GoalExecutionTargetOption[];
  onCreateGoal: (values: CreateGoalValues) => Promise<boolean>;
  onPauseGoal: (goalId: number) => void;
  onResumeGoal: (goalId: number) => void;
  onCancelGoal: (goalId: number) => void;
  onClearError: () => void;
};

const statusColorMap: Record<GoalStatus, string> = {
  Draft: "default",
  Accepted: "blue",
  Rejected: "red",
  Active: "green",
  Paused: "gold",
  Completed: "cyan",
  Expired: "orange",
  Canceled: "default",
};

const tradingSessionOptions = [
  { label: "Any time", value: "AnyTime" },
  { label: "Europe", value: "Europe" },
  { label: "US", value: "Us" },
  { label: "Overlap", value: "EuropeUsOverlap" },
] as const;

export function TargetsTab({
  isSaving,
  error,
  goals,
  executionTargets,
  onCreateGoal,
  onPauseGoal,
  onResumeGoal,
  onCancelGoal,
  onClearError,
}: TargetsTabProps) {
  const [form] = Form.useForm<CreateGoalValues>();
  const targetType = Form.useWatch("targetType", form);

  useEffect(() => {
    form.setFieldsValue({
      executionTarget: executionTargets[0]?.value,
      targetType: "PercentGrowth",
      maxAcceptableRisk: 45,
      maxDrawdownPercent: 2.5,
      maxPositionSizePercent: 20,
      tradingSession: "AnyTime",
      allowOvernightPositions: true,
    });
  }, [executionTargets, form]);

  const handleCreateGoal = async (values: CreateGoalValues) => {
    const created = await onCreateGoal(values);
    if (!created) {
      return;
    }

    form.resetFields();
    form.setFieldsValue({
      executionTarget: executionTargets[0]?.value,
      targetType: "PercentGrowth",
      maxAcceptableRisk: 45,
      maxDrawdownPercent: 2.5,
      maxPositionSizePercent: 20,
      tradingSession: "AnyTime",
      allowOvernightPositions: true,
    });
  };

  return (
    <Space direction="vertical" size={18} style={{ width: "100%" }}>
      <Alert
        type="info"
        showIcon
        message="Target Autopilot is best-effort only. Fintex will not promise returns, and it will reject BTC goals that require excessive risk."
      />
      {error ? <Alert type="warning" showIcon closable onClose={onClearError} message={error} /> : null}

      <Form form={form} layout="vertical" onFinish={handleCreateGoal}>
        <Space wrap align="start" style={{ width: "100%" }}>
          <Form.Item name="name" label="Goal name" style={{ minWidth: 220 }}>
            <Input placeholder="Tomorrow BTC paper push" />
          </Form.Item>
          <Form.Item name="executionTarget" label="Account" rules={[{ required: true }]}>
            <Select style={{ width: 220 }} options={executionTargets} />
          </Form.Item>
          <Form.Item name="targetType" label="Target type" rules={[{ required: true }]}>
            <Select
              style={{ width: 180 }}
              options={[
                { label: "Percent growth", value: "PercentGrowth" },
                { label: "Target amount", value: "TargetAmount" },
              ]}
            />
          </Form.Item>
          {targetType === "TargetAmount" ? (
            <Form.Item name="targetAmount" label="Target amount" rules={[{ required: true }]}>
              <InputNumber min={1} style={{ width: 180 }} />
            </Form.Item>
          ) : (
            <Form.Item name="targetPercent" label="Target %" rules={[{ required: true }]}>
              <InputNumber min={0.01} max={100} step={0.01} style={{ width: 180 }} />
            </Form.Item>
          )}
          <Form.Item name="deadlineLocal" label="Deadline" rules={[{ required: true }]}>
            <Input type="datetime-local" style={{ width: 220 }} />
          </Form.Item>
        </Space>

        <Space wrap align="start" style={{ width: "100%" }}>
          <Form.Item name="maxAcceptableRisk" label="Max risk" rules={[{ required: true }]}>
            <InputNumber min={1} max={100} style={{ width: 160 }} />
          </Form.Item>
          <Form.Item name="maxDrawdownPercent" label="Max drawdown %" rules={[{ required: true }]}>
            <InputNumber min={0.01} max={100} step={0.01} style={{ width: 160 }} />
          </Form.Item>
          <Form.Item name="maxPositionSizePercent" label="Max position %" rules={[{ required: true }]}>
            <InputNumber min={0.01} max={100} step={0.01} style={{ width: 160 }} />
          </Form.Item>
          <Form.Item name="tradingSession" label="Session" rules={[{ required: true }]}>
            <Select style={{ width: 160 }} options={tradingSessionOptions.map((item) => ({ label: item.label, value: item.value }))} />
          </Form.Item>
          <Form.Item name="allowOvernightPositions" label="Overnight" valuePropName="checked">
            <Switch />
          </Form.Item>
        </Space>

        <Button type="primary" htmlType="submit" loading={isSaving} disabled={executionTargets.length === 0}>
          Create BTC goal
        </Button>
      </Form>

      {goals.length === 0 ? (
        <Empty description="No BTC goal targets yet." />
      ) : (
        <List
          dataSource={goals}
          renderItem={(goal) => (
            <List.Item
              actions={[
                goal.status === "Active" || goal.status === "Accepted" ? (
                  <Button key="pause" onClick={() => onPauseGoal(goal.id)}>Pause</Button>
                ) : null,
                goal.status === "Paused" ? (
                  <Button key="resume" onClick={() => onResumeGoal(goal.id)}>Resume</Button>
                ) : null,
                goal.status !== "Completed" && goal.status !== "Canceled" && goal.status !== "Expired" ? (
                  <Button key="cancel" danger onClick={() => onCancelGoal(goal.id)}>Cancel</Button>
                ) : null,
              ].filter(Boolean)}
            >
              <List.Item.Meta
                title={(
                  <Space wrap>
                    <Typography.Text strong>{goal.name}</Typography.Text>
                    <Tag color={statusColorMap[goal.status]}>{goal.status}</Tag>
                    <Tag>{goal.accountType === "ExternalBroker" ? goal.externalConnectionName ?? "External broker" : "Paper academy"}</Tag>
                  </Space>
                )}
                description={(
                  <Space direction="vertical" size={10} style={{ width: "100%" }}>
                    <Typography.Text type="secondary">
                      Target {goal.targetType === "TargetAmount" ? `${goal.targetEquity.toFixed(2)}` : `${goal.targetPercent.toFixed(2)}%`} by {new Date(goal.deadlineUtc).toLocaleString()}
                    </Typography.Text>
                    <Progress percent={Math.min(100, Math.max(0, Number(goal.progressPercent.toFixed(1))))} />
                    <Typography.Text>
                      Current equity {goal.currentEquity.toFixed(2)} | Required daily pace {goal.requiredDailyGrowthPercent.toFixed(3)}%
                    </Typography.Text>
                    <Typography.Text type="secondary">{goal.latestPlanSummary ?? goal.statusReason ?? "Monitoring BTC for the next action."}</Typography.Text>
                    {goal.latestNextAction ? <Typography.Text type="secondary">{goal.latestNextAction}</Typography.Text> : null}
                    {goal.events.length > 0 ? (
                      <Space direction="vertical" size={4} style={{ width: "100%" }}>
                        {goal.events.slice(0, 3).map((event) => (
                          <Typography.Text key={`${goal.id}-${event.id}`} type="secondary">
                            {new Date(event.occurredAtUtc).toLocaleString()}: {event.summary}
                          </Typography.Text>
                        ))}
                      </Space>
                    ) : null}
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
