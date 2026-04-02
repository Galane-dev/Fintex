"use client";

import { useMemo } from "react";
import {
  BarChartOutlined,
  HeatMapOutlined,
  NodeIndexOutlined,
  PieChartOutlined,
  RiseOutlined,
} from "@ant-design/icons";
import { Card, Empty, Tag, Typography } from "antd";
import { formatTime } from "@/utils/market-data";
import type {
  ActivityItem,
  ChartPoint,
  InsightsOverview,
  ProviderBreakdownItem,
  StrategyScoreItem,
} from "./types";
import { useInsightsStyles } from "../style";

const DAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"] as const;
const BUCKETS = ["00-04", "04-08", "08-12", "12-16", "16-20", "20-24"] as const;

const polarToCartesian = (cx: number, cy: number, r: number, angleInDegrees: number) => {
  const angle = ((angleInDegrees - 90) * Math.PI) / 180;
  return { x: cx + r * Math.cos(angle), y: cy + r * Math.sin(angle) };
};

const describeArc = (
  cx: number,
  cy: number,
  r: number,
  startAngle: number,
  endAngle: number,
) => {
  const start = polarToCartesian(cx, cy, r, endAngle);
  const end = polarToCartesian(cx, cy, r, startAngle);
  const largeArcFlag = endAngle - startAngle <= 180 ? "0" : "1";
  return `M ${start.x} ${start.y} A ${r} ${r} 0 ${largeArcFlag} 0 ${end.x} ${end.y}`;
};

type SparkBar = {
  x: number;
  y: number;
  width: number;
  height: number;
};

const buildSparkBars = (points: ChartPoint[]) => {
  if (points.length === 0) {
    return [] as SparkBar[];
  }

  const values = points.map((point) => point.value);
  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;
  const leftPad = 6;
  const rightPad = 96;
  const topPad = 10;
  const bottomPad = 90;
  const drawableHeight = bottomPad - topPad;
  const slot = (rightPad - leftPad) / points.length;
  const width = Math.max(Math.min(4.2, slot - 0.6), 1);

  return points.map((point, index) => {
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
};

const heatColor = (intensity: number) => {
  if (intensity <= 0) {
    return "rgba(109, 170, 124, 0.08)";
  }

  const alpha = Math.min(0.16 + intensity * 0.72, 0.92);
  return `rgba(111, 212, 136, ${alpha})`;
};

const outcomeColor = (outcome: StrategyScoreItem["outcome"]) => {
  if (outcome === "Validated") {
    return "rgba(91, 208, 140, 0.9)";
  }

  if (outcome === "Fail") {
    return "rgba(255, 122, 116, 0.9)";
  }

  return "rgba(255, 203, 116, 0.9)";
};

const outcomeLabel = (outcome: StrategyScoreItem["outcome"] | number | string) => {
  if (outcome === "Validated" || outcome === 2 || outcome === "2") {
    return "Validated";
  }

  if (outcome === "Caution" || outcome === 1 || outcome === "1") {
    return "Caution";
  }

  return "Fail";
};

export function VisualAnalyticsCard({
  overview,
  providerBreakdown,
  strategyScores,
  recentActivity,
  strategyScoreSeries,
}: {
  overview: InsightsOverview;
  providerBreakdown: ProviderBreakdownItem[];
  strategyScores: StrategyScoreItem[];
  recentActivity: ActivityItem[];
  strategyScoreSeries: ChartPoint[];
}) {
  const { styles } = useInsightsStyles();

  const pieSegments = useMemo(() => {
    const base = [
      { label: "Closed trades", value: overview.closedTradeCount, color: "#63d482" },
      { label: "Open trades", value: overview.openTradeCount, color: "#9be6ae" },
      { label: "Alert hits", value: overview.priceAlertHitCount, color: "#ffd07f" },
    ];
    const total = base.reduce((sum, item) => sum + item.value, 0);

    if (total <= 0) {
      return { total: 0, arcs: [] as Array<{ label: string; color: string; value: number; path: string }> };
    }

    let start = 0;
    const arcs = base
      .filter((item) => item.value > 0)
      .map((item) => {
        const angle = (item.value / total) * 360;
        const arc = {
          label: item.label,
          color: item.color,
          value: item.value,
          path: describeArc(50, 50, 36, start, start + angle),
        };
        start += angle;
        return arc;
      });

    return { total, arcs };
  }, [overview.closedTradeCount, overview.openTradeCount, overview.priceAlertHitCount]);

  const providerBars = useMemo(() => {
    const max = Math.max(...providerBreakdown.map((item) => item.count), 1);
    return providerBreakdown.map((item) => ({
      ...item,
      ratio: item.count / max,
    }));
  }, [providerBreakdown]);

  const treemapTiles = useMemo(
    () =>
      strategyScores.slice(0, 8).map((item) => ({
        ...item,
        colSpan: item.score >= 82 ? 6 : item.score >= 65 ? 4 : 3,
        rowSpan: item.score >= 82 ? 4 : 3,
      })),
    [strategyScores],
  );

  const heatmap = useMemo(() => {
    const matrix = Array.from({ length: DAYS.length }, () => Array(BUCKETS.length).fill(0));

    for (const item of recentActivity) {
      const date = new Date(item.occurredAt);
      if (Number.isNaN(date.getTime())) {
        continue;
      }

      const day = date.getDay();
      const bucket = Math.min(Math.floor(date.getHours() / 4), BUCKETS.length - 1);
      matrix[day][bucket] += 1;
    }

    const max = Math.max(...matrix.flat(), 0);
    return { matrix, max };
  }, [recentActivity]);

  const sparkBars = useMemo(
    () => buildSparkBars(strategyScoreSeries),
    [strategyScoreSeries],
  );
  const latestSparkIndex = Math.max(sparkBars.length - 1, 0);

  return (
    <Card className={styles.panel}>
      <div className={styles.visualHeader}>
        <div>
          <Typography.Title level={4} className={styles.panelTitle}>
            Visual analytics studio
          </Typography.Title>
          <Typography.Paragraph className={styles.metaRow}>
            Premium at-a-glance visuals for distribution, trend quality, provider flow, and session timing.
          </Typography.Paragraph>
        </div>
        <Tag>{recentActivity.length} activity points</Tag>
      </div>

      <div className={styles.visualGrid}>
        <div className={styles.visualPanel}>
          <div className={styles.visualPanelHead}>
            <span className={styles.visualPanelTitle}><PieChartOutlined /> Position distribution</span>
          </div>
          {pieSegments.total <= 0 ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No trade distribution yet." />
          ) : (
            <div className={styles.donutWrap}>
              <div className={styles.donutShell}>
                <svg viewBox="0 0 100 100" className={styles.donutSvg}>
                  <circle cx="50" cy="50" r="36" fill="none" stroke="rgba(255,255,255,0.08)" strokeWidth="12" />
                  {pieSegments.arcs.map((segment) => (
                    <path
                      key={segment.label}
                      d={segment.path}
                      fill="none"
                      stroke={segment.color}
                      strokeWidth="12"
                      strokeLinecap="round"
                    />
                  ))}
                </svg>
                <div className={styles.donutCenter}>
                  <span className={styles.donutValue}>{pieSegments.total}</span>
                  <span className={styles.donutLabel}>total points</span>
                </div>
              </div>
              <div className={styles.visualLegend}>
                {pieSegments.arcs.map((segment) => (
                  <div key={segment.label} className={styles.visualLegendRow}>
                    <span className={styles.visualSwatch} style={{ background: segment.color }} />
                    <span>{segment.label}</span>
                    <span>{segment.value}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        <div className={styles.visualPanel}>
          <div className={styles.visualPanelHead}>
            <span className={styles.visualPanelTitle}><BarChartOutlined /> Provider flow</span>
          </div>
          {providerBars.length === 0 ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No provider data yet." />
          ) : (
            <div className={styles.verticalBars}>
              {providerBars.map((item) => (
                <div key={item.label} className={styles.verticalBarCol}>
                  <div className={styles.verticalBarTrack}>
                    <div className={styles.verticalBarFill} style={{ height: `${Math.max(item.ratio * 100, 8)}%` }} />
                  </div>
                  <span className={styles.verticalBarLabel}>{item.label}</span>
                  <span className={styles.verticalBarValue}>{item.count}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className={styles.visualPanel}>
          <div className={styles.visualPanelHead}>
            <span className={styles.visualPanelTitle}><NodeIndexOutlined /> Strategy treemap</span>
          </div>
          {treemapTiles.length === 0 ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No strategy tiles yet." />
          ) : (
            <div className={styles.treemapGrid}>
              {treemapTiles.map((tile) => (
                <div
                  key={tile.id}
                  className={styles.treemapTile}
                  style={{
                    gridColumn: `span ${tile.colSpan}`,
                    gridRow: `span ${tile.rowSpan}`,
                    borderColor: outcomeColor(outcomeLabel(tile.outcome)),
                  }}
                >
                  <div className={styles.treemapTop}>
                    <span className={styles.treemapName}>{tile.label}</span>
                  </div>
                  <div className={styles.treemapScoreWrap}>
                    <Tag
                      color={
                        outcomeLabel(tile.outcome) === "Validated"
                          ? "green"
                          : outcomeLabel(tile.outcome) === "Fail"
                            ? "red"
                            : "gold"
                      }
                    >
                      {tile.score.toFixed(1)}
                    </Tag>
                  </div>
                  <span className={styles.treemapMeta}>{tile.timeframe} | {outcomeLabel(tile.outcome)}</span>
                  <span className={styles.treemapMeta}>{formatTime(tile.createdAt)}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className={styles.visualPanel}>
          <div className={styles.visualPanelHead}>
            <span className={styles.visualPanelTitle}><HeatMapOutlined /> Session heatmap</span>
          </div>
          {heatmap.max <= 0 ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No activity heatmap yet." />
          ) : (
            <div className={styles.heatmapWrap}>
              <div className={styles.heatmapHeader}>
                <span />
                {BUCKETS.map((bucket) => (
                  <span key={bucket} className={styles.heatmapBucket}>{bucket}</span>
                ))}
              </div>
              {DAYS.map((day, dayIndex) => (
                <div key={day} className={styles.heatmapRow}>
                  <span className={styles.heatmapDay}>{day}</span>
                  {BUCKETS.map((bucket, bucketIndex) => {
                    const value = heatmap.matrix[dayIndex][bucketIndex];
                    const intensity = heatmap.max > 0 ? value / heatmap.max : 0;
                    return (
                      <span
                        key={`${day}-${bucket}`}
                        className={styles.heatCell}
                        style={{ background: heatColor(intensity) }}
                        title={`${day} ${bucket}: ${value}`}
                      />
                    );
                  })}
                </div>
              ))}
            </div>
          )}
        </div>

        <div className={styles.visualPanel}>
          <div className={styles.visualPanelHead}>
            <span className={styles.visualPanelTitle}><RiseOutlined /> Validation pulse</span>
          </div>
          {strategyScoreSeries.length === 0 ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No validation pulse yet." />
          ) : (
            <div className={styles.sparkWrap}>
              <svg viewBox="0 0 100 100" className={styles.sparkSvg} preserveAspectRatio="none">
                <defs>
                  <linearGradient id="insights-spark-bar" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="rgba(133, 231, 157, 0.95)" />
                    <stop offset="100%" stopColor="rgba(78, 207, 114, 0.85)" />
                  </linearGradient>
                  <linearGradient id="insights-spark-bar-active" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="rgba(194, 247, 207, 0.98)" />
                    <stop offset="100%" stopColor="rgba(103, 220, 137, 0.92)" />
                  </linearGradient>
                </defs>
                {sparkBars.map((bar, index) => (
                  <rect
                    key={`spark-${index}`}
                    x={bar.x}
                    y={bar.y}
                    width={bar.width}
                    height={bar.height}
                    rx="1"
                    fill={index === latestSparkIndex ? "url(#insights-spark-bar-active)" : "url(#insights-spark-bar)"}
                    opacity={index === latestSparkIndex ? 1 : 0.88}
                  />
                ))}
              </svg>
            </div>
          )}
        </div>
      </div>
    </Card>
  );
}
