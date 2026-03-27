"use client";

import Link from "next/link";
import { useCallback, useMemo, useState } from "react";
import { BellOutlined, HomeOutlined, LogoutOutlined } from "@ant-design/icons";
import { Badge, Button, Card, Space, Tabs } from "antd";
import { DashboardChart, type ChartTradeOverlay } from "@/components/dashboard/DashboardChart";
import { PaperTradingPanel, type DashboardPaperTradingActions } from "@/components/dashboard/PaperTradingPanel";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import { useDashboardBehaviorAnalysis } from "@/hooks/use-dashboard-behavior-analysis";
import { useLiveTrading } from "@/hooks/useLiveTrading";
import { useMarketData } from "@/hooks/useMarketData";
import { useNotifications } from "@/hooks/useNotifications";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import {
  buildFallbackProjectionFromHistory,
  buildMarketInsights,
  formatPercent,
  formatPrice,
  formatSigned,
  formatSignedPoints,
} from "@/utils/market-data";
import { AnalysisTab } from "./analysis-tab";
import { BehaviorAnalysisModal } from "./behavior-analysis-modal";
import { NotificationsModal } from "./notifications-modal";
import { TradeTab } from "./trade-tab";
import { useStyles } from "../style";

const defaultDashboardActions: DashboardPaperTradingActions = {
  hasAccount: false,
  openAccounts: () => undefined,
  openRecommendation: () => undefined,
  openTrade: () => undefined,
};

type CalculationTone = "positive" | "negative" | "neutral";

export function DashboardContent() {
  const { styles } = useStyles();
  const { signOut } = useAuth();
  const { closePosition, isSubmitting: isPaperSubmitting, snapshot } = usePaperTrading();
  const { trades: liveTrades, isLoading: isLiveTradesLoading, refreshTrades } = useLiveTrading();
  const { connectionStatus, error, history, isLoading, latest, refreshSnapshot, timeframeRsi, verdict } = useMarketData();
  const behaviorAnalysis = useDashboardBehaviorAnalysis();
  const notifications = useNotifications();
  const [dashboardActions, setDashboardActions] = useState<DashboardPaperTradingActions>(defaultDashboardActions);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);

  const timeframeRsiMap = useMemo(
    () => timeframeRsi.reduce<Record<string, number | null>>((accumulator, item) => {
      accumulator[item.timeframe] = item.value;
      return accumulator;
    }, {}),
    [timeframeRsi],
  );

  const oneMinuteRsi = timeframeRsiMap["1m"] ?? latest?.rsi ?? null;
  const nextOneMinuteProjection = verdict?.nextOneMinuteProjection?.consensusPrice != null ? verdict.nextOneMinuteProjection : buildFallbackProjectionFromHistory(history, latest?.price, 1);
  const nextFiveMinuteProjection = verdict?.nextFiveMinuteProjection?.consensusPrice != null ? verdict.nextFiveMinuteProjection : buildFallbackProjectionFromHistory(history, latest?.price, 5);
  const openPositions = useMemo(() => snapshot?.positions ?? [], [snapshot?.positions]);
  const closedFills = useMemo(() => snapshot?.recentFills ?? [], [snapshot?.recentFills]);
  const openLiveTrades = useMemo(() => liveTrades.filter((trade) => trade.status === "Open"), [liveTrades]);
  const closedLiveTrades = useMemo(() => liveTrades.filter((trade) => trade.status !== "Open"), [liveTrades]);

  const tradeOverlays = useMemo<ChartTradeOverlay[]>(
    () => [
      ...openPositions.map((position) => ({
        id: `paper-${position.id}`,
        direction: position.direction,
        entryPrice: position.averageEntryPrice,
        stopLoss: position.stopLoss,
        takeProfit: position.takeProfit,
      })),
      ...openLiveTrades.map((trade) => ({
        id: `live-${trade.id}`,
        direction: trade.direction,
        entryPrice: trade.entryPrice,
        stopLoss: trade.stopLoss,
        takeProfit: trade.takeProfit,
      })),
    ],
    [openLiveTrades, openPositions],
  );

  const calculations = useMemo(
    () => [
      { name: "Simple moving average", note: "20-period trend anchor", value: formatPrice(verdict?.sma ?? latest?.sma ?? null), tone: ((verdict?.sma ?? latest?.sma ?? null) != null && latest?.price != null && latest.price >= (verdict?.sma ?? latest?.sma ?? 0) ? "positive" : "neutral") as CalculationTone },
      { name: "Exponential moving average", note: "9-period fast response", value: formatPrice(verdict?.ema ?? latest?.ema ?? null), tone: ((verdict?.ema ?? latest?.ema ?? null) != null && latest?.price != null && latest.price >= (verdict?.ema ?? latest?.ema ?? 0) ? "positive" : "neutral") as CalculationTone },
      { name: "Relative strength index", note: "14-period Wilder RSI", value: oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-", tone: (oneMinuteRsi == null ? "neutral" : oneMinuteRsi >= 65 ? "positive" : oneMinuteRsi <= 35 ? "negative" : "neutral") as CalculationTone },
      { name: "MACD signal", note: "9-period signal line", value: formatSigned(verdict?.macdSignal ?? latest?.macdSignal ?? null), tone: ((verdict?.macd ?? latest?.macd ?? null) != null && (verdict?.macdSignal ?? latest?.macdSignal ?? null) != null && (verdict?.macd ?? latest?.macd ?? 0) >= (verdict?.macdSignal ?? latest?.macdSignal ?? 0) ? "positive" : "negative") as CalculationTone },
      { name: "MACD histogram", note: "Bullish versus bearish expansion", value: formatSigned(verdict?.macdHistogram ?? latest?.macdHistogram ?? null), tone: ((verdict?.macdHistogram ?? latest?.macdHistogram ?? null) == null ? "neutral" : (verdict?.macdHistogram ?? latest?.macdHistogram ?? 0) >= 0 ? "positive" : "negative") as CalculationTone },
      { name: "ATR volatility", note: "14-period normalized ATR", value: verdict?.atrPercent != null ? formatPercent(verdict.atrPercent, 2) : "-", tone: (verdict?.atrPercent == null ? "neutral" : verdict.atrPercent >= 0.65 ? "negative" : "positive") as CalculationTone },
      { name: "Momentum", note: "14-period acceleration", value: formatSignedPoints(verdict?.momentum ?? latest?.momentum ?? null), tone: ((verdict?.momentum ?? latest?.momentum ?? null) == null ? "neutral" : (verdict?.momentum ?? latest?.momentum ?? 0) >= 0 ? "positive" : "negative") as CalculationTone },
      { name: "ADX trend strength", note: "14-period directional strength", value: verdict?.adx != null ? verdict.adx.toFixed(1) : "-", tone: (verdict?.adx == null ? "neutral" : verdict.adx >= 25 ? "positive" : verdict.adx < 15 ? "negative" : "neutral") as CalculationTone },
    ],
    [latest, oneMinuteRsi, verdict],
  );

  const marketSignals = useMemo(() => buildMarketInsights(latest, verdict), [latest, verdict]);
  const handleRegisterDashboardActions = useCallback((actions: DashboardPaperTradingActions) => setDashboardActions(actions), []);

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <div className={styles.header}>
          <Space wrap>
            <Button onClick={() => { void refreshSnapshot(); void refreshTrades(); }} loading={isLoading || isLiveTradesLoading}>Refresh snapshot</Button>
            <Badge count={notifications.unreadCount} size="small">
              <Button icon={<BellOutlined />} onClick={() => setIsNotificationsOpen(true)}>
                Notifications
              </Button>
            </Badge>
            <Link href={ROUTES.home}><Button icon={<HomeOutlined />}>Landing page</Button></Link>
            <Button type="primary" icon={<LogoutOutlined />} onClick={signOut}>Sign out</Button>
          </Space>
        </div>

        <div className={styles.workspace}>
          <div className={styles.chartColumn}>
            <DashboardChart
              symbol="BTCUSDT"
              venue="Binance"
              tradeOverlays={tradeOverlays}
              bid={latest?.bid ?? null}
              ask={latest?.ask ?? null}
              onOpenAccounts={dashboardActions.openAccounts}
              onOpenRecommendation={dashboardActions.openRecommendation}
              onOpenBehaviorAnalysis={() => { void behaviorAnalysis.open(); }}
              onOpenTrade={dashboardActions.openTrade}
            />
          </div>

          <div className={styles.sideColumn}>
            <PaperTradingPanel currentPrice={latest?.price ?? null} registerDashboardActions={handleRegisterDashboardActions} displayMode="support" />

            <Card className={styles.panelCard}>
              <Tabs
                className={styles.dashboardTabs}
                items={[
                  {
                    key: "analysis",
                    label: "Verdict & analysis",
                    children: (
                      <AnalysisTab
                        connectionStatus={connectionStatus}
                        error={error}
                        latestVerdict={verdict?.verdict ?? latest?.verdict ?? "Hold"}
                        confidenceScore={verdict?.confidenceScore ?? latest?.confidenceScore ?? null}
                        oneMinuteRsi={oneMinuteRsi}
                        macd={verdict?.macd ?? latest?.macd ?? null}
                        macdSignal={verdict?.macdSignal ?? latest?.macdSignal ?? null}
                        momentum={verdict?.momentum ?? latest?.momentum ?? null}
                        trendScore={verdict?.trendScore ?? latest?.trendScore ?? null}
                        adx={verdict?.adx ?? null}
                        timeframeRsiMap={timeframeRsiMap}
                        verdict={verdict}
                        calculations={calculations}
                        marketSignals={marketSignals}
                        nextOneMinuteProjection={nextOneMinuteProjection}
                        nextFiveMinuteProjection={nextFiveMinuteProjection}
                      />
                    ),
                  },
                  {
                    key: "open-trade",
                    label: `Open trade (${openPositions.length + openLiveTrades.length})`,
                    children: <TradeTab mode="open" openPositions={openPositions} liveTrades={openLiveTrades} isPaperSubmitting={isPaperSubmitting} onClosePaperPosition={(positionId) => { void closePosition({ positionId }); }} />,
                  },
                  {
                    key: "closed-trade",
                    label: `Closed trade (${closedFills.length + closedLiveTrades.length})`,
                    children: <TradeTab mode="closed" closedFills={closedFills} liveTrades={closedLiveTrades} isPaperSubmitting={isPaperSubmitting} />,
                  },
                ]}
              />
            </Card>
          </div>
        </div>
      </div>

      <BehaviorAnalysisModal
        isOpen={behaviorAnalysis.isOpen}
        isLoading={behaviorAnalysis.isLoading}
        error={behaviorAnalysis.error}
        profile={behaviorAnalysis.profile}
        onClose={behaviorAnalysis.close}
      />
      <NotificationsModal
        isOpen={isNotificationsOpen}
        isLoading={notifications.isLoading}
        isSaving={notifications.isSaving}
        error={notifications.error}
        unreadCount={notifications.unreadCount}
        notifications={notifications.notifications}
        alertRules={notifications.alertRules}
        onClose={() => setIsNotificationsOpen(false)}
        onClearError={notifications.clearError}
        onMarkAsRead={(notificationId) => { void notifications.markAsRead(notificationId); }}
        onMarkAllAsRead={() => { void notifications.markAllAsRead(); }}
        onDeleteAlertRule={(ruleId) => { void notifications.deleteAlertRule(ruleId); }}
        onCreatePriceAlert={(values) => {
          return notifications.createPriceAlert({
            name: values.name,
            symbol: values.symbol,
            provider: 1,
            targetPrice: values.targetPrice,
            notifyInApp: values.notifyInApp,
            notifyEmail: values.notifyEmail,
            notes: values.notes,
          });
        }}
        onSendTestAlert={() => notifications.sendTestAlert()}
      />
    </div>
  );
}
