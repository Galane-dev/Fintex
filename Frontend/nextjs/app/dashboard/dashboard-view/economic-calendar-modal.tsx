"use client";

import { Alert, List, Skeleton, Space, Tag, Typography } from "antd";
import { DashboardDrawerShell } from "@/components/dashboard/dashboard-drawer-shell";
import type { EconomicCalendarInsight } from "@/types/economic-calendar";
import { formatTime } from "@/utils/market-data";

interface EconomicCalendarModalProps {
  isOpen: boolean;
  isLoading: boolean;
  error: string | null;
  insight: EconomicCalendarInsight | null;
  onClose: () => void;
}

const getRiskColor = (riskScore: number) => {
  if (riskScore >= 75) {
    return "red";
  }

  if (riskScore >= 45) {
    return "orange";
  }

  return "green";
};

export function EconomicCalendarModal({
  isOpen,
  isLoading,
  error,
  insight,
  onClose,
}: EconomicCalendarModalProps) {
  const content = isLoading ? (
    <Skeleton active paragraph={{ rows: 7 }} />
  ) : error ? (
    <Alert type="warning" showIcon message={error} />
  ) : insight ? (
    <Space direction="vertical" size={18} style={{ width: "100%" }}>
      <Alert
        type={insight.riskScore >= 45 ? "warning" : "info"}
        showIcon
        message="BTC / USD macro-event risk"
        description={insight.summary}
      />

      <Space wrap>
        <Tag color={getRiskColor(insight.riskScore)}>
          Risk {insight.riskScore.toFixed(1)}
        </Tag>
        {insight.nextEventAtUtc ? (
          <Tag color="gold">Next event {formatTime(insight.nextEventAtUtc)}</Tag>
        ) : null}
      </Space>

      <List
        bordered
        dataSource={insight.upcomingEvents}
        locale={{ emptyText: "No nearby CPI, NFP, or FOMC events are currently queued." }}
        renderItem={(item) => (
          <List.Item>
            <List.Item.Meta
              title={
                <Space wrap>
                  <Typography.Text strong>{item.title}</Typography.Text>
                  <Tag color="blue">{item.source}</Tag>
                </Space>
              }
              description={`${formatTime(item.occursAtUtc)} · impact ${item.impactScore.toFixed(0)}`}
            />
          </List.Item>
        )}
      />
    </Space>
  ) : (
    <Alert type="info" showIcon message="No economic calendar insight is available yet." />
  );

  return (
    <DashboardDrawerShell
      open={isOpen}
      onClose={onClose}
      title="Economic calendar"
      width={720}
    >
      {content}
    </DashboardDrawerShell>
  );
}
