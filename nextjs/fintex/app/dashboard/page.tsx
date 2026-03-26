"use client";

import Link from "next/link";
import { useCallback, useMemo, useState } from "react";
import {
  HomeOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import {
  Alert,
  Button,
  Card,
  Collapse,
  Empty,
  Modal,
  Progress,
  Skeleton,
  Space,
  Tabs,
  Tag,
  Typography,
} from "antd";
import { DashboardChart } from "@/components/dashboard/DashboardChart";
import {
  type DashboardPaperTradingActions,
  PaperTradingPanel,
} from "@/components/dashboard/PaperTradingPanel";
import { ROUTES } from "@/constants/routes";
import { withAuth } from "@/hoc/withAuth";
import { useAuth } from "@/hooks/useAuth";
import { useMarketData } from "@/hooks/useMarketData";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import { ExternalBrokerProvider } from "@/providers/external-broker-provider";
import { MarketDataProvider } from "@/providers/market-data-provider";
import { PaperTradingProvider } from "@/providers/paper-trading-provider";
import type { UserProfile } from "@/types/user-profile";
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
import { getMyUserProfile } from "@/utils/user-profile-api";
import { useStyles } from "./style";

const defaultDashboardActions: DashboardPaperTradingActions = {
  hasAccount: false,
  openAccounts: () => undefined,
  openRecommendation: () => undefined,
  openTrade: () => undefined,
};

function DashboardContent() {
  const { styles, cx } = useStyles();
  const { signOut } = useAuth();
  const {
    closePosition,
    isSubmitting: isPaperSubmitting,
    snapshot,
  } = usePaperTrading();
  const [dashboardActions, setDashboardActions] =
    useState<DashboardPaperTradingActions>(defaultDashboardActions);
  const [isBehaviorOpen, setIsBehaviorOpen] = useState(false);
  const [isBehaviorLoading, setIsBehaviorLoading] = useState(false);
  const [behaviorError, setBehaviorError] = useState<string | null>(null);
  const [behaviorProfile, setBehaviorProfile] = useState<UserProfile | null>(null);
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
  const openPositions = snapshot?.positions ?? [];
  const closedFills = snapshot?.recentFills ?? [];

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

  const handleRegisterDashboardActions = useCallback(
    (actions: DashboardPaperTradingActions) => {
      setDashboardActions(actions);
    },
    [],
  );

  const handleOpenBehaviorAnalysis = useCallback(async () => {
    setIsBehaviorOpen(true);
    setIsBehaviorLoading(true);
    setBehaviorError(null);

    try {
      const profile = await getMyUserProfile();
      setBehaviorProfile(profile);
    } catch (profileError) {
      setBehaviorError(
        profileError instanceof Error
          ? profileError.message
          : "We could not load your behavior analysis.",
      );
    } finally {
      setIsBehaviorLoading(false);
    }
  }, []);

  const miniStats = useMemo(
    () => [
      {
        label: "Market price",
        value: formatPrice(latest?.price),
        note: connectionStatus,
      },
      {
        label: "Bias and confidence",
        value:
          verdict?.verdict != null
            ? `${verdict.verdict}${verdict.confidenceScore != null ? ` · ${verdict.confidenceScore.toFixed(1)}` : ""}`
            : "-",
        note: verdict?.trendScore != null ? `Trend ${Math.round(verdict.trendScore)}` : "Trend loading",
      },
      {
        label: "Open trade",
        value: String(openPositions.length),
        note:
          snapshot?.account != null
            ? `Realized ${formatSignedPoints(snapshot.account.realizedProfitLoss)}`
            : "Create paper account",
      },
      {
        label: "Volume window",
        value: formatCompact(history.reduce((sum, item) => sum + (item.volume ?? 0), 0)),
        note: `Last tick ${latest?.timestamp ? formatTime(latest.timestamp) : "-"}`,
      },
    ],
    [connectionStatus, history, latest, openPositions.length, snapshot?.account, verdict],
  );
  void miniStats;

  const renderAccordionLabel = (title: string, summary: string, tone?: string) => (
    <div className={styles.accordionHeader}>
      <span>{title}</span>
      <span
        className={cx(
          styles.accordionSummary,
          tone === "positive" ? styles.positive : undefined,
          tone === "negative" ? styles.negative : undefined,
        )}
      >
        {summary}
      </span>
    </div>
  );

  const analysisTabContent = (
    <div className={styles.tabStack}>
      <div className={styles.subPanel}>
        <div className={styles.verdictHero}>
          <div className={styles.verdictRow}>
            <div className={styles.verdictLabel}>
              <Typography.Text type="secondary">Realtime stance</Typography.Text>
              <div className={styles.verdictValue}>
                {verdict?.verdict ?? latest?.verdict ?? "Hold"} bias
              </div>
            </div>
            <Tag color={getConnectionTone(connectionStatus)}>{connectionStatus}</Tag>
          </div>

          <div className={styles.scoreBlock}>
            <div className={styles.scoreLabel}>Confidence score</div>
            <div className={styles.scoreValue}>
              {verdict?.confidenceScore != null ? verdict.confidenceScore.toFixed(1) : "-"}
            </div>
            <Progress
              percent={Math.max(
                0,
                Math.min(Math.round(verdict?.confidenceScore ?? 0), 100),
              )}
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
            <Tag color="purple">ADX {effectiveAdx != null ? effectiveAdx.toFixed(1) : "-"}</Tag>
          </Space>
        </div>
      </div>

      <Collapse
        className={styles.analysisCollapse}
        ghost
        defaultActiveKey={["rsi"]}
        items={[
          {
            key: "rsi",
            label: renderAccordionLabel(
              "RSI by timeframe",
              `1m ${oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-"}`,
              oneMinuteRsi == null
                ? undefined
                : oneMinuteRsi >= 65
                  ? "positive"
                  : oneMinuteRsi <= 35
                    ? "negative"
                    : undefined,
            ),
            children: (
              <div className={styles.metricList}>
                {["1m", "5m", "15m", "1h", "4h"].map((timeframeKey) => {
                  const itemValue =
                    timeframeKey === "1m" ? oneMinuteRsi : timeframeRsiMap[timeframeKey] ?? null;

                  return (
                    <div key={timeframeKey} className={styles.metricRow}>
                      <div className={styles.metricMeta}>
                        <span className={styles.metricName}>{timeframeKey}</span>
                        <span className={styles.metricNote}>
                          Wilder RSI based on {timeframeKey} candles
                        </span>
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
            ),
          },
          {
            key: "timeframes",
            label: renderAccordionLabel(
              "Timeframe confirmation",
              verdict?.timeframeAlignmentScore != null
                ? formatSigned(verdict.timeframeAlignmentScore, 0)
                : "-",
            ),
            children: (
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
            ),
          },
          {
            key: "calculations",
            label: renderAccordionLabel(
              "Live calculations",
              `ADX ${effectiveAdx != null ? effectiveAdx.toFixed(1) : "-"}`,
              effectiveAdx != null && effectiveAdx >= 25 ? "positive" : undefined,
            ),
            children: (
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
            ),
          },
          {
            key: "signals",
            label: renderAccordionLabel(
              "Signal desk",
              marketSignals.length > 0 ? marketSignals[0]?.tag ?? "-" : "-",
            ),
            children: (
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
            ),
          },
          {
            key: "overlays",
            label: renderAccordionLabel(
              "Decision overlays",
              verdict?.structureLabel || "Loading",
            ),
            children: (
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
            ),
          },
        ]}
      />

      {error ? <Alert title={error} type="warning" showIcon /> : null}
    </div>
  );

  const openPositionsTabContent = (
    <div className={styles.tabStack}>
      {openPositions.length === 0 ? (
        <div className={styles.emptyState}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="No paper positions are open yet."
          />
        </div>
      ) : (
        <div className={styles.positionsList}>
          {openPositions.map((position) => (
            <div key={position.id} className={styles.positionCard}>
              <div className={styles.positionHeader}>
                <div>
                  <div className={styles.positionTitle}>
                    {position.symbol} {position.direction}
                  </div>
                  <div className={styles.positionSubtle}>
                    Opened {formatTime(position.openedAt)}
                  </div>
                </div>

                <Space wrap>
                  <Tag color={position.direction === "Buy" ? "green" : "red"}>
                    {position.quantity.toFixed(4)}
                  </Tag>
                  <Button
                    size="small"
                    loading={isPaperSubmitting}
                    onClick={() =>
                      void closePosition({
                        positionId: position.id,
                      })
                    }
                  >
                    Close
                  </Button>
                </Space>
              </div>

              <div className={styles.positionMetrics}>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Entry</span>
                  <span className={styles.positionMetricValue}>
                    {formatPrice(position.averageEntryPrice)}
                  </span>
                </div>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Mark</span>
                  <span className={styles.positionMetricValue}>
                    {formatPrice(position.currentMarketPrice)}
                  </span>
                </div>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Stop loss</span>
                  <span className={styles.positionMetricValue}>
                    {formatPrice(position.stopLoss)}
                  </span>
                </div>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Take profit</span>
                  <span className={styles.positionMetricValue}>
                    {formatPrice(position.takeProfit)}
                  </span>
                </div>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Unrealized P/L</span>
                  <span
                    className={cx(
                      styles.positionMetricValue,
                      position.unrealizedProfitLoss >= 0 ? styles.positive : styles.negative,
                    )}
                  >
                    {formatPrice(position.unrealizedProfitLoss)}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );

  const closedPositionsTabContent = (
    <div className={styles.tabStack}>
      {closedFills.length === 0 ? (
        <div className={styles.emptyState}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="Closed paper trades will appear here once positions are exited."
          />
        </div>
      ) : (
        <div className={styles.positionsList}>
          {closedFills.map((fill) => (
            <div key={fill.id} className={styles.positionCard}>
              <div className={styles.positionHeader}>
                <div>
                  <div className={styles.positionTitle}>
                    {fill.symbol} {fill.direction}
                  </div>
                  <div className={styles.positionSubtle}>
                    Closed {formatTime(fill.executedAt)}
                  </div>
                </div>

                <Tag color={fill.realizedProfitLoss >= 0 ? "green" : "red"}>
                  {formatPrice(fill.realizedProfitLoss)}
                </Tag>
              </div>

              <div className={styles.positionMetrics}>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Quantity</span>
                  <span className={styles.positionMetricValue}>{fill.quantity.toFixed(4)}</span>
                </div>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Exit price</span>
                  <span className={styles.positionMetricValue}>{formatPrice(fill.price)}</span>
                </div>
                <div className={styles.positionMetric}>
                  <span className={styles.positionMetricLabel}>Realized P/L</span>
                  <span
                    className={cx(
                      styles.positionMetricValue,
                      fill.realizedProfitLoss >= 0 ? styles.positive : styles.negative,
                    )}
                  >
                    {formatPrice(fill.realizedProfitLoss)}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );

  const behaviorContent = isBehaviorLoading ? (
    <Skeleton active paragraph={{ rows: 7 }} />
  ) : behaviorError ? (
    <Alert type="warning" showIcon title={behaviorError} />
  ) : behaviorProfile ? (
    <div className={styles.behaviorPanel}>
      <div className={styles.behaviorHero}>
        <div>
          <div className={styles.behaviorLabel}>Behavioral risk score</div>
          <div className={styles.behaviorValue}>
            {behaviorProfile.behavioralRiskScore.toFixed(1)}
          </div>
        </div>
        <Space wrap>
          <Tag color={behaviorProfile.isAiInsightsEnabled ? "green" : "default"}>
            {behaviorProfile.isAiInsightsEnabled ? "AI insights on" : "AI insights off"}
          </Tag>
          <Tag color="blue">{behaviorProfile.preferredBaseCurrency || "USD"}</Tag>
          <Tag color="purple">
            Last analysis{" "}
            {behaviorProfile.lastBehavioralAnalysisTime
              ? formatTime(behaviorProfile.lastBehavioralAnalysisTime)
              : "Pending"}
          </Tag>
        </Space>
      </div>

      <div className={styles.behaviorBlock}>
        <div className={styles.behaviorSectionTitle}>Behavior summary</div>
        <Typography.Paragraph className={styles.signalCopy}>
          {behaviorProfile.behavioralSummary ||
            "Your behavioral summary has not been generated yet. As you place more simulator trades, this panel will become more useful."}
        </Typography.Paragraph>
      </div>

      <div className={styles.behaviorGrid}>
        <div className={styles.summaryCard}>
          <span className={styles.summaryLabel}>Favorite symbols</span>
          <span className={styles.summaryValue}>
            {behaviorProfile.favoriteSymbols || "BTCUSDT"}
          </span>
        </div>
        <div className={styles.summaryCard}>
          <span className={styles.summaryLabel}>Risk tolerance</span>
          <span className={styles.summaryValue}>
            {behaviorProfile.riskTolerance.toFixed(2)}
          </span>
        </div>
        <div className={styles.summaryCard}>
          <span className={styles.summaryLabel}>AI provider</span>
          <span className={styles.summaryValue}>
            {behaviorProfile.lastAiProvider || "Not set"}
          </span>
        </div>
        <div className={styles.summaryCard}>
          <span className={styles.summaryLabel}>AI model</span>
          <span className={styles.summaryValue}>
            {behaviorProfile.lastAiModel || "Not set"}
          </span>
        </div>
      </div>

      <div className={styles.behaviorBlock}>
        <div className={styles.behaviorSectionTitle}>Strategy notes</div>
        <Typography.Paragraph className={styles.signalCopy}>
          {behaviorProfile.strategyNotes || "No strategy notes saved yet."}
        </Typography.Paragraph>
      </div>
    </div>
  ) : (
    <Alert type="info" showIcon title="Behavior analysis is not available yet." />
  );

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <div className={styles.header}>
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
            <DashboardChart
              symbol="BTCUSDT"
              venue="Binance"
              activePositions={openPositions}
              bid={latest?.bid ?? null}
              ask={latest?.ask ?? null}
              onOpenAccounts={dashboardActions.openAccounts}
              onOpenRecommendation={dashboardActions.openRecommendation}
              onOpenBehaviorAnalysis={() => {
                void handleOpenBehaviorAnalysis();
              }}
              onOpenTrade={dashboardActions.openTrade}
            />
          </div>

          <div className={styles.sideColumn}>
            <PaperTradingPanel
              currentPrice={latest?.price ?? null}
              registerDashboardActions={handleRegisterDashboardActions}
              displayMode="support"
            />

            <Card className={styles.panelCard}>
              <Tabs
                className={styles.dashboardTabs}
                items={[
                  {
                    key: "analysis",
                    label: "Verdict & analysis",
                    children: analysisTabContent,
                  },
                  {
                    key: "open-positions",
                    label: `Open trade (${openPositions.length})`,
                    children: openPositionsTabContent,
                  },
                  {
                    key: "closed-positions",
                    label: `Closed trade (${closedFills.length})`,
                    children: closedPositionsTabContent,
                  },
                ]}
              />
            </Card>
          </div>
        </div>
      </div>

      <Modal
        open={isBehaviorOpen}
        onCancel={() => setIsBehaviorOpen(false)}
        footer={null}
        title="My behavior analysis"
        width={720}
      >
        {behaviorContent}
      </Modal>
    </div>
  );
}

function DashboardView() {
  return (
    <MarketDataProvider>
      <ExternalBrokerProvider>
        <PaperTradingProvider>
          <DashboardContent />
        </PaperTradingProvider>
      </ExternalBrokerProvider>
    </MarketDataProvider>
  );
}

const ProtectedDashboardPage = withAuth(DashboardView);

export default ProtectedDashboardPage;
