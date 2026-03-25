"use client";

import Link from "next/link";
import { useMemo } from "react";
import { HomeOutlined, LogoutOutlined, ThunderboltFilled } from "@ant-design/icons";
import { Alert, Button, Card, Progress, Space, Tag, Typography } from "antd";
import { DashboardChart } from "@/components/dashboard/DashboardChart";
import { ROUTES } from "@/constants/routes";
import { withAuth } from "@/hoc/withAuth";
import { useAuth } from "@/hooks/useAuth";
import { useMarketData } from "@/hooks/useMarketData";
import { MarketDataProvider } from "@/providers/market-data-provider";
import {
  buildMarketInsights,
  formatCompact,
  formatPercent,
  formatPrice,
  formatSigned,
  formatSignedPoints,
  formatTime,
  getConnectionTone,
} from "@/utils/market-data";
import { useStyles } from "./style";

function DashboardContent() {
  const { styles, cx } = useStyles();
  const { signOut, user } = useAuth();
  const {
    connectionStatus,
    error,
    history,
    isLoading,
    latest,
    refreshSnapshot,
    timeframeRsi,
    verdict,
  } = useMarketData();

  const timeframeRsiMap = useMemo(
    () =>
      timeframeRsi.reduce<Record<string, number | null>>((accumulator, item) => {
        accumulator[item.timeframe] = item.value;
        return accumulator;
      }, {}),
    [timeframeRsi],
  );

  const oneMinuteRsi = timeframeRsiMap["1m"] ?? latest?.rsi ?? null;
  const effectiveSma = verdict?.sma ?? latest?.sma ?? null;
  const effectiveEma = verdict?.ema ?? latest?.ema ?? null;
  const effectiveMacd = verdict?.macd ?? latest?.macd ?? null;
  const effectiveMacdSignal = verdict?.macdSignal ?? latest?.macdSignal ?? null;
  const effectiveMacdHistogram = verdict?.macdHistogram ?? latest?.macdHistogram ?? null;
  const effectiveMomentum = verdict?.momentum ?? latest?.momentum ?? null;
  const effectiveAtrPercent = verdict?.atrPercent ?? null;
  const effectiveAdx = verdict?.adx ?? null;

  const calculations = useMemo(
    () => [
      {
        name: "Simple moving average",
        note: "20-period trend anchor",
        value: formatPrice(effectiveSma),
        tone:
          effectiveSma != null && latest?.price != null && latest.price >= effectiveSma
            ? "positive"
            : "neutral",
      },
      {
        name: "Exponential moving average",
        note: "9-period fast response",
        value: formatPrice(effectiveEma),
        tone:
          effectiveEma != null && latest?.price != null && latest.price >= effectiveEma
            ? "positive"
            : "neutral",
      },
      {
        name: "Relative strength index",
        note: "14-period Wilder RSI",
        value: oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-",
        tone:
          oneMinuteRsi == null
            ? "neutral"
            : oneMinuteRsi >= 65
              ? "positive"
              : oneMinuteRsi <= 35
                ? "negative"
                : "neutral",
      },
      {
        name: "MACD signal",
        note: "9-period signal line",
        value: formatSigned(effectiveMacdSignal),
        tone:
          effectiveMacd != null && effectiveMacdSignal != null
            ? effectiveMacd >= effectiveMacdSignal
              ? "positive"
              : "negative"
            : "neutral",
      },
      {
        name: "MACD histogram",
        note: "Bullish versus bearish expansion",
        value: formatSigned(effectiveMacdHistogram),
        tone:
          effectiveMacdHistogram == null
            ? "neutral"
            : effectiveMacdHistogram >= 0
              ? "positive"
              : "negative",
      },
      {
        name: "ATR volatility",
        note: "14-period normalized ATR",
        value: effectiveAtrPercent != null ? formatPercent(effectiveAtrPercent, 2) : "-",
        tone:
          effectiveAtrPercent == null
            ? "neutral"
            : effectiveAtrPercent >= 0.65
              ? "negative"
              : "positive",
      },
      {
        name: "Momentum",
        note: "14-period acceleration",
        value: formatSignedPoints(effectiveMomentum),
        tone:
          effectiveMomentum == null ? "neutral" : effectiveMomentum >= 0 ? "positive" : "negative",
      },
      {
        name: "ADX trend strength",
        note: "14-period directional strength",
        value: effectiveAdx != null ? effectiveAdx.toFixed(1) : "-",
        tone:
          effectiveAdx == null
            ? "neutral"
            : effectiveAdx >= 25
              ? "positive"
              : effectiveAdx < 15
                ? "negative"
                : "neutral",
      },
    ],
    [
      effectiveAdx,
      effectiveAtrPercent,
      effectiveEma,
      effectiveMacd,
      effectiveMacdHistogram,
      effectiveMacdSignal,
      effectiveMomentum,
      effectiveSma,
      latest,
      oneMinuteRsi,
    ],
  );

  const marketSignals = useMemo(() => buildMarketInsights(latest, verdict), [latest, verdict]);

  const miniStats = useMemo(
    () => [
      {
        label: "Trend score",
        value: verdict?.trendScore != null ? `${Math.round(verdict.trendScore)} / 100` : "-",
      },
      {
        label: "Confidence",
        value: verdict?.confidenceScore != null ? verdict.confidenceScore.toFixed(1) : "-",
      },
      {
        label: "Volume window",
        value: formatCompact(history.reduce((sum, item) => sum + (item.volume ?? 0), 0)),
      },
      {
        label: "Last tick",
        value: formatTime(latest?.timestamp),
      },
    ],
    [history, latest, verdict],
  );

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <div className={styles.header}>
          <div className={styles.headingWrap}>
            <Typography.Text className={styles.eyebrow}>
              <ThunderboltFilled /> Live trading workspace
            </Typography.Text>
            <Typography.Title level={2} className={styles.title}>
              Welcome back, {user?.firstName ?? "Trader"}
            </Typography.Title>
            <Typography.Paragraph className={styles.helper}>
              The platform now focuses on Binance BTCUSDT only, which keeps storage leaner while the dashboard stays chart-first and realtime.
            </Typography.Paragraph>
          </div>

          <Space wrap>
            <Button onClick={() => void refreshSnapshot()} loading={isLoading}>
              Refresh snapshot
            </Button>
            <Link href={ROUTES.home}>
              <Button icon={<HomeOutlined />}>Landing page</Button>
            </Link>
            <Button type="primary" icon={<LogoutOutlined />} onClick={signOut}>
              Sign out
            </Button>
          </Space>
        </div>

        <div className={styles.workspace}>
          <div className={styles.chartColumn}>
            <DashboardChart symbol="BTCUSDT" venue="Binance" />
          </div>

          <div className={styles.sideColumn}>
            <Card className={styles.panelCard} title="Verdict and confidence">
              <div className={styles.verdictHero}>
                <div className={styles.verdictRow}>
                  <div className={styles.verdictLabel}>
                    <Typography.Text type="secondary">Realtime stance</Typography.Text>
                    <div className={styles.verdictValue}>{verdict?.verdict ?? latest?.verdict ?? "Hold"} bias</div>
                  </div>
                  <Tag color={getConnectionTone(connectionStatus)}>{connectionStatus}</Tag>
                </div>

                <div className={styles.scoreBlock}>
                  <div className={styles.scoreLabel}>Confidence score</div>
                  <div className={styles.scoreValue}>
                    {verdict?.confidenceScore != null ? verdict.confidenceScore.toFixed(1) : "-"}
                  </div>
                  <Progress
                    percent={Math.max(0, Math.min(Math.round(verdict?.confidenceScore ?? 0), 100))}
                    showInfo={false}
                    strokeColor="#9bf2b1"
                    trailColor="rgba(255,255,255,0.08)"
                  />
                </div>

                <Typography.Paragraph className={styles.verdictCopy}>
                  Multi-timeframe EMA, RSI, MACD, ATR, ADX, structure, and alignment checks surface actionable direction beside the chart without forcing the trader to hunt for context.
                </Typography.Paragraph>

                <Space wrap>
                  <Tag color="green">MACD {formatSigned(effectiveMacd)}</Tag>
                  <Tag color="blue">Signal {formatSigned(effectiveMacdSignal)}</Tag>
                  <Tag color="lime">Momentum {formatSignedPoints(effectiveMomentum)}</Tag>
                  <Tag color="gold">
                    RSI 1m {oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-"}
                  </Tag>
                  <Tag color="blue">
                    Trend {verdict?.trendScore != null ? formatSigned(verdict.trendScore, 0) : "-"}
                  </Tag>
                  <Tag color="purple">
                    ADX {effectiveAdx != null ? effectiveAdx.toFixed(1) : "-"}
                  </Tag>
                </Space>
              </div>
            </Card>

            <Card className={styles.panelCard} title="RSI by timeframe">
              <div className={styles.metricList}>
                {["1m", "5m", "15m", "1h", "4h"].map((timeframeKey) => {
                  const itemValue =
                    timeframeKey === "1m"
                      ? oneMinuteRsi
                      : timeframeRsiMap[timeframeKey] ?? null;

                  return (
                    <div key={timeframeKey} className={styles.metricRow}>
                      <div className={styles.metricMeta}>
                        <span className={styles.metricName}>{timeframeKey}</span>
                        <span className={styles.metricNote}>Wilder RSI based on {timeframeKey} candles </span>
                      </div>
                      <span
                        className={cx(
                          styles.metricValue,
                          itemValue == null
                            ? styles.neutral
                            : itemValue >= 65
                              ? styles.positive
                              : itemValue <= 35
                                ? styles.negative
                                : styles.neutral,
                        )}
                      >
                        {itemValue != null ? itemValue.toFixed(1) : "-"}
                      </span>
                    </div>
                  );
                })}
              </div>
            </Card>

            <Card className={styles.panelCard} title="Timeframe confirmation">
              <div className={styles.metricList}>
                {(verdict?.timeframeSignals ?? []).map((item) => (
                  <div key={item.timeframe} className={styles.metricRow}>
                    <div className={styles.metricMeta}>
                      <span className={styles.metricName}>{item.timeframe}</span>
                      <span className={styles.metricNote}>Cross-timeframe directional bias</span>
                    </div>
                    <span
                      className={cx(
                        styles.metricValue,
                        item.signal === "Bullish" ? styles.positive : undefined,
                        item.signal === "Bearish" ? styles.negative : undefined,
                        item.signal === "Neutral" ? styles.neutral : undefined,
                      )}
                    >
                      {item.biasScore != null ? formatSigned(item.biasScore, 0) : "-"}
                    </span>
                  </div>
                ))}
                <div className={styles.metricRow}>
                  <div className={styles.metricMeta}>
                    <span className={styles.metricName}>Alignment score</span>
                    <span className={styles.metricNote}>5m, 15m, and 1h confirmation mix</span>
                  </div>
                  <span className={cx(styles.metricValue, styles.neutral)}>
                    {verdict?.timeframeAlignmentScore != null
                      ? formatSigned(verdict.timeframeAlignmentScore, 0)
                      : "-"}
                  </span>
                </div>
              </div>
            </Card>

            <Card className={styles.panelCard} title="Live calculations">
              <div className={styles.metricList}>
                {calculations.map((item) => (
                  <div key={item.name} className={styles.metricRow}>
                    <div className={styles.metricMeta}>
                      <span className={styles.metricName}>{item.name}</span>
                      <span className={styles.metricNote}>{item.note}</span>
                    </div>
                    <span
                      className={cx(
                        styles.metricValue,
                        item.tone === "positive" ? styles.positive : undefined,
                        item.tone === "negative" ? styles.negative : undefined,
                        item.tone === "neutral" ? styles.neutral : undefined,
                      )}
                    >
                      {item.value}
                    </span>
                  </div>
                ))}
              </div>
            </Card>

            <Card className={styles.panelCard} title="Signal desk">
              <div className={styles.signalList}>
                {marketSignals.map((item) => (
                  <div key={item.title} className={styles.signalItem}>
                    <div className={styles.signalHeading}>
                      <span className={styles.signalTitle}>{item.title}</span>
                      <Tag color={item.tone}>{item.tag}</Tag>
                    </div>
                    <Typography.Paragraph className={styles.signalCopy}>
                      {item.copy}
                    </Typography.Paragraph>
                  </div>
                ))}
              </div>
            </Card>

            <Card className={styles.panelCard} title="Decision overlays">
              <div className={styles.signalList}>
                <div className={styles.signalItem}>
                  <div className={styles.signalHeading}>
                    <span className={styles.signalTitle}>Market structure</span>
                    <Tag color="blue">{verdict?.structureLabel || "Loading"}</Tag>
                  </div>
                  <Typography.Paragraph className={styles.signalCopy}>
                    Structure score {verdict?.structureScore != null ? formatSigned(verdict.structureScore, 0) : "-"} adds breakout and swing-quality context on top of pure indicators.
                  </Typography.Paragraph>
                </div>
                <div className={styles.signalItem}>
                  <div className={styles.signalHeading}>
                    <span className={styles.signalTitle}>Behavior and academy gates</span>
                    <Tag color="default">Future layer</Tag>
                  </div>
                  <Typography.Paragraph className={styles.signalCopy}>
                    The current verdict is market-only. Later we can overlay behavioral discipline, simulator performance, and academy unlock rules before allowing real-market action.
                  </Typography.Paragraph>
                </div>
              </div>
            </Card>

            {error ? <Alert title={error} type="warning" showIcon /> : null}
          </div>
        </div>

        <div className={styles.miniStrip}>
          {miniStats.map((item) => (
            <div key={item.label} className={styles.miniCard}>
              <div className={styles.miniLabel}>{item.label}</div>
              <div className={styles.miniValue}>{item.value}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function DashboardView() {
  return (
    <MarketDataProvider>
      <DashboardContent />
    </MarketDataProvider>
  );
}

const ProtectedDashboardPage = withAuth(DashboardView);

export default ProtectedDashboardPage;
