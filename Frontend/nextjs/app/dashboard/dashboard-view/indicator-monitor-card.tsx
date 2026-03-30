"use client";

import { useMemo } from "react";
import { Card, Tag, Typography } from "antd";
import type { MarketDataPoint, MarketVerdictSnapshot } from "@/types/market-data";
import { formatSigned, formatSignedPoints } from "@/utils/market-data";
import { useStyles } from "../style";

interface IndicatorMonitorCardProps {
  history: MarketDataPoint[];
  latest: MarketDataPoint | null;
  verdict: MarketVerdictSnapshot | null;
}

interface IndicatorSeriesConfig {
  key: string;
  label: string;
  note: string;
  value: string;
  accent: string;
  data: number[];
}

const SVG_WIDTH = 320;
const SVG_HEIGHT = 82;
const SERIES_WINDOW = 60;

const buildSparklinePath = (values: number[]) => {
  if (values.length === 0) {
    return "";
  }

  if (values.length === 1) {
    return `M 0 ${SVG_HEIGHT / 2} L ${SVG_WIDTH} ${SVG_HEIGHT / 2}`;
  }

  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;

  return values
    .map((value, index) => {
      const x = (index / (values.length - 1)) * SVG_WIDTH;
      const y = SVG_HEIGHT - ((value - min) / range) * (SVG_HEIGHT - 8) - 4;
      return `${index === 0 ? "M" : "L"} ${x.toFixed(2)} ${y.toFixed(2)}`;
    })
    .join(" ");
};

const getRecentSeries = (history: MarketDataPoint[], selector: (point: MarketDataPoint) => number | null) =>
  history
    .slice(-SERIES_WINDOW)
    .map(selector)
    .filter((value): value is number => value != null && Number.isFinite(value));

const getSeriesRange = (values: number[]) => {
  if (values.length === 0) {
    return null;
  }

  const min = Math.min(...values);
  const max = Math.max(...values);
  const latest = values[values.length - 1];

  return { min, max, latest };
};

const formatAxisValue = (value: number, decimals = 1) => value.toFixed(decimals);

export function IndicatorMonitorCard({ history, latest, verdict }: IndicatorMonitorCardProps) {
  const { styles } = useStyles();

  const series = useMemo<IndicatorSeriesConfig[]>(() => {
    const rsiSeries = getRecentSeries(history, (point) => point.rsi);
    const macdHistogramSeries = getRecentSeries(history, (point) => point.macdHistogram);
    const momentumSeries = getRecentSeries(history, (point) => point.momentum);
    const trendSeries = getRecentSeries(history, (point) => point.trendScore);

    const currentRsi = latest?.rsi ?? verdict?.rsi ?? null;
    const currentMacdHistogram = latest?.macdHistogram ?? verdict?.macdHistogram ?? null;
    const currentMomentum = latest?.momentum ?? verdict?.momentum ?? null;
    const currentTrend = latest?.trendScore ?? verdict?.trendScore ?? null;

    return [
      {
        key: "rsi",
        label: "RSI 1m",
        note: "Short-horizon pressure",
        value: currentRsi != null ? currentRsi.toFixed(1) : "-",
        accent: currentRsi != null && currentRsi <= 35 ? "#ff7875" : "#9bf2b1",
        data: rsiSeries,
      },
      {
        key: "macd-histogram",
        label: "MACD histogram",
        note: "Bullish vs bearish expansion",
        value: formatSigned(currentMacdHistogram),
        accent: currentMacdHistogram != null && currentMacdHistogram < 0 ? "#ff7875" : "#7cb4ff",
        data: macdHistogramSeries,
      },
      {
        key: "momentum",
        label: "Momentum",
        note: "Impulse and deceleration",
        value: formatSignedPoints(currentMomentum),
        accent: currentMomentum != null && currentMomentum < 0 ? "#ff9f43" : "#52c41a",
        data: momentumSeries,
      },
      {
        key: "trend",
        label: "Trend score",
        note: "Composite structure bias",
        value: currentTrend != null ? formatSigned(currentTrend, 0) : "-",
        accent: currentTrend != null && currentTrend < 0 ? "#ff7875" : "#b37feb",
        data: trendSeries,
      },
    ];
  }, [history, latest, verdict]);

  return (
    <Card className={styles.panelCard}>
      <div className={styles.indicatorMonitor}>
        <div className={styles.indicatorMonitorHeader}>
          <div className={styles.indicatorMonitorTitle}>
            <Typography.Text type="secondary">Live indicator monitor</Typography.Text>
            <div className={styles.indicatorMonitorValue}>
              {latest?.verdict ?? verdict?.verdict ?? "Hold"} bias
            </div>
          </div>
          <div className={styles.indicatorMonitorMeta}>
            <Tag color="gold">1m desk</Tag>
            <Tag color="blue">{series.some((item) => item.data.length > 0) ? "Streaming" : "Waiting"}</Tag>
          </div>
        </div>

        {series.every((item) => item.data.length === 0) ? (
          <Typography.Paragraph className={styles.indicatorFallback}>
            Fintex is waiting for enough live market points before it draws the micro charts for RSI, MACD histogram, momentum, and trend score.
          </Typography.Paragraph>
        ) : (
          <div className={styles.indicatorMonitorGrid}>
            {series.map((item) => {
              const range = getSeriesRange(item.data);

              return (
                <div key={item.key} className={styles.indicatorMiniCard}>
                  <div className={styles.indicatorMiniHeader}>
                    <div>
                      <div className={styles.indicatorMiniLabel}>{item.label}</div>
                      <div className={styles.indicatorMiniValue}>{item.value}</div>
                    </div>
                  </div>
                  <div className={styles.indicatorChartRow}>
                    <div className={styles.indicatorAxis}>
                      <span className={styles.indicatorAxisLabel}>
                        {range ? formatAxisValue(range.max, item.key === "trend" ? 0 : 1) : "-"}
                      </span>
                      <span className={styles.indicatorAxisLabel}>
                        {range ? formatAxisValue(range.latest, item.key === "trend" ? 0 : 1) : "-"}
                      </span>
                      <span className={styles.indicatorAxisLabel}>
                        {range ? formatAxisValue(range.min, item.key === "trend" ? 0 : 1) : "-"}
                      </span>
                    </div>
                    <div>
                      <svg
                        className={styles.indicatorSparkline}
                        viewBox={`0 0 ${SVG_WIDTH} ${SVG_HEIGHT}`}
                        preserveAspectRatio="none"
                        role="img"
                        aria-label={`${item.label} sparkline`}
                      >
                        <defs>
                          <linearGradient id={`spark-${item.key}`} x1="0%" y1="0%" x2="100%" y2="0%">
                            <stop offset="0%" stopColor={item.accent} stopOpacity="0.35" />
                            <stop offset="100%" stopColor={item.accent} stopOpacity="1" />
                          </linearGradient>
                        </defs>
                        <path
                          d={buildSparklinePath(item.data)}
                          fill="none"
                          stroke={`url(#spark-${item.key})`}
                          strokeWidth="2.5"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                      <div className={styles.indicatorXAxis}>
                        <span>{Math.max(item.data.length - 1, 0)} points ago</span>
                        <span>Now</span>
                      </div>
                    </div>
                  </div>
                  <div className={styles.indicatorMiniNote}>{item.note}</div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </Card>
  );
}
