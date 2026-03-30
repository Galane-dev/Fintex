"use client";

import { useMemo, useState } from "react";
import {
  AlertOutlined,
  AreaChartOutlined,
  FundProjectionScreenOutlined,
} from "@ant-design/icons";
import { Card, Empty, Segmented, Space, Tag, Typography } from "antd";
import { formatPrice, formatTime } from "@/utils/market-data";
import type { ChartPoint, InsightsChartMode } from "./types";
import { useInsightsStyles } from "../style";

const buildPath = (points: ChartPoint[]) => {
  if (points.length === 0) {
    return "";
  }

  const values = points.map((point) => point.value);
  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;

  return points
    .map((point, index) => {
      const x = (index / Math.max(points.length - 1, 1)) * 100;
      const y = 100 - ((point.value - min) / range) * 100;
      return `${index === 0 ? "M" : "L"} ${x} ${y}`;
    })
    .join(" ");
};

const chartMeta = {
  equity: {
    title: "Equity curve",
    eyebrow: "Performance curve",
    icon: <AreaChartOutlined />,
    empty: "Closed trades will draw the equity curve here.",
  },
  "strategy-score": {
    title: "Strategy score trend",
    eyebrow: "Validation trend",
    icon: <FundProjectionScreenOutlined />,
    empty: "Validated strategies will start drawing a score trend here.",
  },
  "alert-hits": {
    title: "Alert hit timeline",
    eyebrow: "Alert flow",
    icon: <AlertOutlined />,
    empty: "Triggered price alerts will appear as a running hit timeline here.",
  },
} as const;

export function PnlChartCard({
  equityPoints,
  strategyScorePoints,
  alertPoints,
}: {
  equityPoints: ChartPoint[];
  strategyScorePoints: ChartPoint[];
  alertPoints: ChartPoint[];
}) {
  const { styles } = useInsightsStyles();
  const [mode, setMode] = useState<InsightsChartMode>("equity");

  const points = useMemo(() => {
    if (mode === "strategy-score") {
      return strategyScorePoints;
    }

    return mode === "alert-hits" ? alertPoints : equityPoints;
  }, [alertPoints, equityPoints, mode, strategyScorePoints]);

  const latestValue = points[points.length - 1]?.value ?? null;
  const firstValue = points[0]?.value ?? null;
  const delta = latestValue != null && firstValue != null ? latestValue - firstValue : null;
  const meta = chartMeta[mode];

  return (
    <Card className={styles.panel}>
      <div className={styles.heroPanel}>
        <div>
          <div className={styles.heroEyebrow}>{meta.eyebrow}</div>
          <Typography.Title level={4} className={styles.heroTitle}>
            {meta.title}
          </Typography.Title>
        </div>
        <Space wrap>
          <Segmented
            value={mode}
            options={[
              { label: "Equity", value: "equity" },
              { label: "Strategy", value: "strategy-score" },
              { label: "Alerts", value: "alert-hits" },
            ]}
            onChange={(value) => setMode(value as InsightsChartMode)}
          />
          <Tag icon={meta.icon}>{points.length} points</Tag>
          {delta != null ? (
            <Tag color={delta >= 0 ? "green" : "red"}>
              {mode === "equity" ? formatPrice(delta) : delta.toFixed(1)}
            </Tag>
          ) : null}
        </Space>
      </div>

      {points.length === 0 ? (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={meta.empty} />
      ) : (
        <div className={styles.chartWrap}>
          <svg viewBox="0 0 100 100" width="100%" height="100%" preserveAspectRatio="none">
            <defs>
              <linearGradient id="insights-line" x1="0" y1="0" x2="1" y2="0">
                <stop offset="0%" stopColor="#4be16b" />
                <stop offset="100%" stopColor="#9bf2b1" />
              </linearGradient>
            </defs>
            <path d={buildPath(points)} fill="none" stroke="url(#insights-line)" strokeWidth="2.6" />
          </svg>
        </div>
      )}

      {points.length > 0 ? (
        <div className={styles.statRow}>
          <div className={styles.statPill}>
            <span className={styles.statLabel}>Start</span>
            <span className={styles.statValue}>{formatTime(points[0].label)}</span>
          </div>
          <div className={styles.statPill}>
            <span className={styles.statLabel}>Latest</span>
            <span className={styles.statValue}>
              {mode === "equity" ? formatPrice(latestValue) : latestValue?.toFixed(1) ?? "-"}
            </span>
          </div>
        </div>
      ) : null}
    </Card>
  );
}
