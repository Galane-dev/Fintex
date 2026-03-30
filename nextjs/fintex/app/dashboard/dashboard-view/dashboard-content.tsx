"use client";

import Link from "next/link";
import { useCallback, useMemo, useState } from "react";
import {
  FlagOutlined,
  BellOutlined,
  BarChartOutlined,
  CalendarOutlined,
  HomeOutlined,
  LogoutOutlined,
  MessageOutlined,
  ReloadOutlined,
} from "@ant-design/icons";
import { Badge, Button, Card, Space, Tabs } from "antd";
import { AssistantDrawer } from "@/components/dashboard/assistant-drawer";
import { DashboardChart, type ChartTradeOverlay } from "@/components/dashboard/DashboardChart";
import { PaperTradingPanel, type DashboardPaperTradingActions } from "@/components/dashboard/PaperTradingPanel";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import { useDashboardAssistant } from "@/hooks/use-dashboard-assistant";
import { useDashboardBehaviorAnalysis } from "@/hooks/use-dashboard-behavior-analysis";
import { useDashboardEconomicCalendar } from "@/hooks/use-dashboard-economic-calendar";
import { useDashboardStrategyValidation } from "@/hooks/use-dashboard-strategy-validation";
import { useExternalBrokerAccounts } from "@/hooks/useExternalBrokerAccounts";
import { useGoalAutomation } from "@/hooks/useGoalAutomation";
import { useLiveTrading } from "@/hooks/useLiveTrading";
import { useMarketData } from "@/hooks/useMarketData";
import { useNotifications } from "@/hooks/useNotifications";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import { useTradeAutomation } from "@/hooks/useTradeAutomation";
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
import { EconomicCalendarModal } from "./economic-calendar-modal";
import { NotificationsModal } from "./notifications-modal";
import { StrategyValidationModal } from "./strategy-validation-modal";
import { GoalAutomationDrawer } from "./goal-automation-drawer";
import type { GoalExecutionTargetOption } from "./targets-tab";
import { TradeTab } from "./trade-tab";
import { useStyles } from "../style";

const defaultDashboardActions: DashboardPaperTradingActions = {
  hasAccount: false,
  openAccounts: () => undefined,
  openRecommendation: () => undefined,
  openTrade: () => undefined,
};

type CalculationTone = "positive" | "negative" | "neutral";

const PAPER_EXECUTION_TARGET = "paper";

const buildAutomationExecutionTargets = (
  hasPaperAccount: boolean,
  connections: Array<{ id: number; displayName: string; status: string }>,
) => {
  const targets = hasPaperAccount
    ? [{ label: "Paper academy", value: PAPER_EXECUTION_TARGET }]
    : [];

  return targets.concat(
    connections
      .filter((connection) => connection.status === "Connected")
      .map((connection) => ({
        label: connection.displayName,
        value: `external-${connection.id}`,
      })),
  );
};

const parseAutomationExecutionTarget = (value: string) => {
  if (value === PAPER_EXECUTION_TARGET) {
    return { destination: 1, externalConnectionId: null as number | null };
  }

  if (value.startsWith("external-")) {
    const externalConnectionId = Number(value.replace("external-", ""));
    return { destination: 2, externalConnectionId };
  }

  return { destination: 1, externalConnectionId: null as number | null };
};

const mapGoalTradingSessionToCode = (value: string) => {
  switch (value) {
    case "Europe":
      return 2;
    case "Us":
      return 3;
    case "EuropeUsOverlap":
      return 4;
    default:
      return 1;
  }
};

const mapTriggerTypeToCode = (value: string) => {
  switch (value) {
    case "RelativeStrengthIndex":
      return 2;
    case "MacdHistogram":
      return 3;
    case "Momentum":
      return 4;
    case "TrendScore":
      return 5;
    case "ConfidenceScore":
      return 6;
    case "Verdict":
      return 7;
    default:
      return 1;
  }
};

const mapVerdictToCode = (value?: "Buy" | "Sell") => (value === "Sell" ? 2 : value === "Buy" ? 1 : null);

export function DashboardContent() {
  const { styles } = useStyles();
  const { signOut } = useAuth();
  const { closePosition, isSubmitting: isPaperSubmitting, snapshot } = usePaperTrading();
  const externalBrokers = useExternalBrokerAccounts();
  const { trades: liveTrades, isLoading: isLiveTradesLoading, refreshTrades } = useLiveTrading();
  const { connectionStatus, error, history, isLoading, latest, refreshSnapshot, timeframeRsi, verdict } = useMarketData();
  const behaviorAnalysis = useDashboardBehaviorAnalysis();
  const economicCalendar = useDashboardEconomicCalendar();
  const strategyValidation = useDashboardStrategyValidation();
  const notifications = useNotifications();
  const tradeAutomation = useTradeAutomation();
  const goalAutomation = useGoalAutomation();
  const assistant = useDashboardAssistant({
    onActionRefresh: async () => {
      await Promise.all([
        refreshSnapshot(),
        refreshTrades(),
        notifications.refreshInbox(),
        goalAutomation.refreshGoals(),
      ]);
    },
  });
  const [dashboardActions, setDashboardActions] = useState<DashboardPaperTradingActions>(defaultDashboardActions);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [isGoalAutomationOpen, setIsGoalAutomationOpen] = useState(false);

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
  const automationExecutionTargets = useMemo(
    () => buildAutomationExecutionTargets(snapshot?.account != null, externalBrokers.connections),
    [externalBrokers.connections, snapshot?.account],
  );
  const goalExecutionTargets = automationExecutionTargets as GoalExecutionTargetOption[];

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
            <Button
              aria-label="Refresh snapshot"
              title="Refresh snapshot"
              icon={<ReloadOutlined />}
              onClick={() => { void refreshSnapshot(); void refreshTrades(); }}
              loading={isLoading || isLiveTradesLoading}
            />
            <Badge count={notifications.unreadCount} size="small">
              <Button
                aria-label="Notifications"
                title="Notifications"
                icon={<BellOutlined />}
                onClick={() => setIsNotificationsOpen(true)}
              />
            </Badge>
            <Button
              aria-label="Economic calendar"
              title="Economic calendar"
              icon={<CalendarOutlined />}
              onClick={() => {
                void economicCalendar.open();
              }}
            />
            <Button
              aria-label="Goal automation"
              title="Goal automation"
              icon={<FlagOutlined />}
              onClick={() => setIsGoalAutomationOpen(true)}
            />
            <Button
              aria-label="Assistant"
              title="Assistant"
              icon={<MessageOutlined />}
              onClick={assistant.open}
            />
            <Link href={ROUTES.insights}>
              <Button
                aria-label="Insights"
                title="Insights"
                icon={<BarChartOutlined />}
              />
            </Link>
            <Link href={ROUTES.home}>
              <Button
                aria-label="Landing page"
                title="Landing page"
                icon={<HomeOutlined />}
              />
            </Link>
            <Button
              type="primary"
              aria-label="Sign out"
              title="Sign out"
              icon={<LogoutOutlined />}
              onClick={signOut}
            />
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
              onOpenStrategyValidation={() => { void strategyValidation.open(); }}
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
      <EconomicCalendarModal
        isOpen={economicCalendar.isOpen}
        isLoading={economicCalendar.isLoading}
        error={economicCalendar.error}
        insight={economicCalendar.insight}
        onClose={economicCalendar.close}
      />
      <NotificationsModal
        isOpen={isNotificationsOpen}
        isLoading={notifications.isLoading}
        isSaving={notifications.isSaving}
        error={notifications.error}
        unreadCount={notifications.unreadCount}
        notifications={notifications.notifications}
        alertRules={notifications.alertRules}
        automationRules={tradeAutomation.rules}
        automationExecutionTargets={automationExecutionTargets}
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
        onCreateAutomationRule={(values) => {
          const executionTarget = parseAutomationExecutionTarget(values.executionTarget);

          return tradeAutomation.createRule({
            name: values.name,
            symbol: values.symbol,
            provider: 1,
            triggerType: mapTriggerTypeToCode(values.triggerType),
            triggerValue: values.triggerValue ?? null,
            targetVerdict: mapVerdictToCode(values.targetVerdict),
            minimumConfidenceScore: values.minimumConfidenceScore ?? null,
            destination: executionTarget.destination,
            externalConnectionId: executionTarget.externalConnectionId,
            tradeDirection: values.tradeDirection === "Sell" ? 2 : 1,
            quantity: values.quantity,
            stopLoss: values.stopLoss ?? null,
            takeProfit: values.takeProfit ?? null,
            notifyInApp: values.notifyInApp,
            notifyEmail: values.notifyEmail,
            notes: values.notes,
          });
        }}
        onSendTestAlert={() => notifications.sendTestAlert()}
        onDeleteAutomationRule={(ruleId) => { void tradeAutomation.deleteRule(ruleId); }}
      />
      <StrategyValidationModal
        isOpen={strategyValidation.isOpen}
        isLoadingHistory={strategyValidation.isLoadingHistory}
        isSubmitting={strategyValidation.isSubmitting}
        error={strategyValidation.error}
        latestResult={strategyValidation.latestResult}
        history={strategyValidation.history}
        onClose={strategyValidation.close}
        onSubmit={strategyValidation.submit}
      />
      <GoalAutomationDrawer
        isOpen={isGoalAutomationOpen}
        isSaving={goalAutomation.isSaving}
        error={goalAutomation.error}
        goals={goalAutomation.goals}
        executionTargets={goalExecutionTargets}
        onClose={() => setIsGoalAutomationOpen(false)}
        onClearError={goalAutomation.clearError}
        onPauseGoal={(goalId) => {
          void goalAutomation.pauseGoal(goalId);
        }}
        onResumeGoal={(goalId) => {
          void goalAutomation.resumeGoal(goalId);
        }}
        onCancelGoal={(goalId) => {
          void goalAutomation.cancelGoal(goalId);
        }}
        onCreateGoal={(values) => {
          const executionTarget = parseAutomationExecutionTarget(values.executionTarget);
          const deadlineLocal = values.deadlineLocal ? new Date(values.deadlineLocal) : null;

          return goalAutomation.createGoal({
            name: values.name,
            accountType: executionTarget.destination,
            externalConnectionId: executionTarget.externalConnectionId,
            targetType: values.targetType === "TargetAmount" ? 2 : 1,
            targetPercent: values.targetType === "TargetAmount" ? null : values.targetPercent ?? null,
            targetAmount: values.targetType === "TargetAmount" ? values.targetAmount ?? null : null,
            deadlineUtc: deadlineLocal?.toISOString() ?? "",
            maxAcceptableRisk: values.maxAcceptableRisk,
            maxDrawdownPercent: values.maxDrawdownPercent,
            maxPositionSizePercent: values.maxPositionSizePercent,
            tradingSession: mapGoalTradingSessionToCode(values.tradingSession),
            allowOvernightPositions: values.allowOvernightPositions,
          });
        }}
      />
      <AssistantDrawer
        isOpen={assistant.isOpen}
        isSending={assistant.isSending}
        isListening={assistant.isListening}
        error={assistant.error}
        draft={assistant.draft}
        transcript={assistant.transcript}
        speakReplies={assistant.speakReplies}
        messages={assistant.messages}
        suggestedPrompts={assistant.suggestedPrompts}
        onClose={assistant.close}
        onDraftChange={assistant.setDraft}
        onSubmit={assistant.submitMessage}
        onStartListening={assistant.startListening}
        onStopListening={assistant.stopListening}
        onToggleSpeakReplies={assistant.setSpeakReplies}
      />
    </div>
  );
}
