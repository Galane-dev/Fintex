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

type ChartBar = {
  x: number;
  y: number;
  width: number;
  height: number;
};

const buildBarGeometry = (points: ChartPoint[]) => {
  if (points.length === 0) {
    return { bars: [] as ChartBar[], baselineY: 92 };
  }

  const values = points.map((point) => point.value);
  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;
  const leftPad = 6;
  const rightPad = 96;
  const topPad = 8;
  const bottomPad = 92;
  const drawableHeight = bottomPad - topPad;
  const slot = (rightPad - leftPad) / points.length;
  const width = Math.max(Math.min(6.2, slot - 0.7), 1.2);

  const bars = points.map((point, index) => {
    const x = leftPad + index * slot + (slot - width) / 2;
    const height = Math.max(((point.value - min) / range) * drawableHeight, 2);
    const y = bottomPad - height;

    return {
      x,
      y,
      width,
      height,
    };
  });

  return { bars, baselineY: bottomPad };
};

const chartMeta = {
  equity: {
    title: "Equity bars",
    eyebrow: "Performance distribution",
    icon: <AreaChartOutlined />,
    empty: "Closed trades will draw equity bars here.",
  },
  "strategy-score": {
    title: "Strategy score bars",
    eyebrow: "Validation distribution",
    icon: <FundProjectionScreenOutlined />,
    empty: "Validated strategies will start drawing score bars here.",
  },
  "alert-hits": {
    title: "Alert hit bars",
    eyebrow: "Alert distribution",
    icon: <AlertOutlined />,
    empty: "Triggered price alerts will appear as running bars here.",
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
  const chartGeometry = buildBarGeometry(points);
  const latestIndex = Math.max(points.length - 1, 0);

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
              <linearGradient id="insights-bar" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="#93ebaa" />
                <stop offset="100%" stopColor="#4ecf72" />
              </linearGradient>
              <linearGradient id="insights-bar-active" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="#c2f7cf" />
                <stop offset="100%" stopColor="#67dc89" />
              </linearGradient>
            </defs>
            <path d="M 0 25 L 100 25" stroke="rgba(255,255,255,0.07)" strokeWidth="0.6" fill="none" />
            <path d="M 0 50 L 100 50" stroke="rgba(255,255,255,0.06)" strokeWidth="0.6" fill="none" />
            <path d="M 0 75 L 100 75" stroke="rgba(255,255,255,0.05)" strokeWidth="0.6" fill="none" />
            <path
              d={`M 6 ${chartGeometry.baselineY} L 96 ${chartGeometry.baselineY}`}
              stroke="rgba(255,255,255,0.08)"
              strokeWidth="0.8"
              fill="none"
            />
            {chartGeometry.bars.map((bar, index) => (
              <rect
                key={`${points[index]?.label ?? "bar"}-${index}`}
                x={bar.x}
                y={bar.y}
                width={bar.width}
                height={bar.height}
                rx="1.2"
                fill={index === latestIndex ? "url(#insights-bar-active)" : "url(#insights-bar)"}
                opacity={index === latestIndex ? 1 : 0.86}
              />
            ))}
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

