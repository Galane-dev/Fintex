"use client";

import Link from "next/link";
import { useCallback, useMemo, useState } from "react";
import {
  FlagOutlined,
  BellOutlined,
  BarChartOutlined,
  CalendarOutlined,
  LogoutOutlined,
  MessageOutlined,
  ReloadOutlined,
} from "@ant-design/icons";
import { Badge, Button, Card, Space, Tabs } from "antd";
import { AssistantDrawer } from "@/components/dashboard/assistant-drawer";
import { DashboardChart, type ChartTradeOverlay } from "@/components/dashboard/DashboardChart";
import { PaperTradingPanel, type DashboardPaperTradingActions } from "@/components/dashboard/PaperTradingPanel";
import { getFintexButtonLoading } from "@/components/fintex-loader";
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
  buildMarketInsights,
  formatPercent,
  formatPrice,
  formatSigned,
  formatSignedPoints,
} from "@/utils/market-data";
import { AnalysisTab } from "./analysis-tab";
import { AutomationDeskCard } from "./automation-desk-card";
import { BehaviorAnalysisModal } from "./behavior-analysis-modal";
import { DashboardSideSection } from "./dashboard-side-section";
import { EconomicCalendarModal } from "./economic-calendar-modal";
import { IndicatorMonitorCard } from "./indicator-monitor-card";
import { NotificationsModal } from "./notifications-modal";
import { StrategyValidationModal } from "./strategy-validation-modal";
import { GoalAutomationDrawer } from "./goal-automation-drawer";
import type { GoalExecutionTargetOption } from "./targets-tab";
import { TradeTab } from "./trade-tab";
import { useDecisionSupport } from "./use-decision-support";
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
  const { trades: liveTrades, refreshTrades } = useLiveTrading();
  const {
    connectionStatus,
    history,
    latest,
    refreshSnapshot,
    selection,
    verdict: streamedVerdict,
    timeframeRsi: streamedTimeframeRsi,
  } = useMarketData();
  const behaviorAnalysis = useDashboardBehaviorAnalysis();
  const economicCalendar = useDashboardEconomicCalendar();
  const strategyValidation = useDashboardStrategyValidation();
  const notifications = useNotifications();
  const tradeAutomation = useTradeAutomation();
  const goalAutomation = useGoalAutomation();
  const decisionSupportSnapshot = useDecisionSupport({
    selection,
    streamedVerdict,
    streamedTimeframeRsi,
    connectionStatus,
  });
  const assistant = useDashboardAssistant({
    onActionRefresh: async () => {
      await Promise.all([
        refreshSnapshot(),
        refreshTrades(),
        notifications.refreshInbox(),
        goalAutomation.refreshGoals(),
        tradeAutomation.refreshRules(),
        externalBrokers.refreshConnections(),
      ]);
    },
  });
  const [dashboardActions, setDashboardActions] = useState<DashboardPaperTradingActions>(defaultDashboardActions);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [isGoalAutomationOpen, setIsGoalAutomationOpen] = useState(false);
  const [isManualSnapshotRefreshing, setIsManualSnapshotRefreshing] = useState(false);

  const resolvedVerdict = decisionSupportSnapshot.verdict;
  const resolvedTimeframeRsi = decisionSupportSnapshot.timeframeRsi;

  const timeframeRsiMap = useMemo(
    () => resolvedTimeframeRsi.reduce<Record<string, number | null>>((accumulator, item) => {
      accumulator[item.timeframe] = item.value;
      return accumulator;
    }, {}),
    [resolvedTimeframeRsi],
  );

  const oneMinuteRsi = timeframeRsiMap["1m"] ?? resolvedVerdict?.rsi ?? null;
  const nextOneMinuteProjection = resolvedVerdict?.nextOneMinuteProjection ?? null;
  const nextFiveMinuteProjection = resolvedVerdict?.nextFiveMinuteProjection ?? null;
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
      { name: "Simple moving average", note: "20-period trend anchor", value: formatPrice(resolvedVerdict?.sma ?? null), tone: (resolvedVerdict?.sma != null && resolvedVerdict?.price != null && resolvedVerdict.price >= resolvedVerdict.sma ? "positive" : "neutral") as CalculationTone },
      { name: "Exponential moving average", note: "9-period fast response", value: formatPrice(resolvedVerdict?.ema ?? null), tone: (resolvedVerdict?.ema != null && resolvedVerdict?.price != null && resolvedVerdict.price >= resolvedVerdict.ema ? "positive" : "neutral") as CalculationTone },
      { name: "Relative strength index", note: "14-period Wilder RSI", value: oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-", tone: (oneMinuteRsi == null ? "neutral" : oneMinuteRsi >= 65 ? "positive" : oneMinuteRsi <= 35 ? "negative" : "neutral") as CalculationTone },
      { name: "MACD signal", note: "9-period signal line", value: formatSigned(resolvedVerdict?.macdSignal ?? null), tone: (resolvedVerdict?.macd != null && resolvedVerdict?.macdSignal != null && resolvedVerdict.macd >= resolvedVerdict.macdSignal ? "positive" : resolvedVerdict?.macd != null && resolvedVerdict?.macdSignal != null ? "negative" : "neutral") as CalculationTone },
      { name: "MACD histogram", note: "Bullish versus bearish expansion", value: formatSigned(resolvedVerdict?.macdHistogram ?? null), tone: (resolvedVerdict?.macdHistogram == null ? "neutral" : resolvedVerdict.macdHistogram >= 0 ? "positive" : "negative") as CalculationTone },
      { name: "ATR volatility", note: "14-period normalized ATR", value: resolvedVerdict?.atrPercent != null ? formatPercent(resolvedVerdict.atrPercent, 2) : "-", tone: (resolvedVerdict?.atrPercent == null ? "neutral" : resolvedVerdict.atrPercent >= 0.65 ? "negative" : "positive") as CalculationTone },
      { name: "Momentum", note: "14-period acceleration", value: formatSignedPoints(resolvedVerdict?.momentum ?? null), tone: (resolvedVerdict?.momentum == null ? "neutral" : resolvedVerdict.momentum >= 0 ? "positive" : "negative") as CalculationTone },
      { name: "ADX trend strength", note: "14-period directional strength", value: resolvedVerdict?.adx != null ? resolvedVerdict.adx.toFixed(1) : "-", tone: (resolvedVerdict?.adx == null ? "neutral" : resolvedVerdict.adx >= 25 ? "positive" : resolvedVerdict.adx < 15 ? "negative" : "neutral") as CalculationTone },
    ],
    [oneMinuteRsi, resolvedVerdict],
  );

  const marketSignals = useMemo(() => buildMarketInsights(resolvedVerdict), [resolvedVerdict]);
  const handleRegisterDashboardActions = useCallback((actions: DashboardPaperTradingActions) => setDashboardActions(actions), []);
  const handleManualSnapshotRefresh = useCallback(async () => {
    setIsManualSnapshotRefreshing(true);

    try {
      await Promise.all([refreshSnapshot(), refreshTrades()]);
    } finally {
      setIsManualSnapshotRefreshing(false);
    }
  }, [refreshSnapshot, refreshTrades]);

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <div className={styles.header}>
          <Space wrap>
            <Button
              aria-label="Refresh snapshot"
              title="Refresh snapshot"
              icon={<ReloadOutlined />}
              onClick={() => {
                void handleManualSnapshotRefresh();
              }}
              loading={getFintexButtonLoading(isManualSnapshotRefreshing)}
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
                className={styles.brandHomeButton}
                aria-label="Landing page"
                title="Fintex home"
              >
                F
              </Button>
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
            <DashboardSideSection title="Account operations" defaultOpen>
              <PaperTradingPanel currentPrice={latest?.price ?? null} registerDashboardActions={handleRegisterDashboardActions} displayMode="support" />
            </DashboardSideSection>

            <DashboardSideSection title="Decision support">
              <Card className={styles.panelCard}>
                <AnalysisTab
                  connectionStatus={decisionSupportSnapshot.connectionStatus}
                  error={decisionSupportSnapshot.error}
                  latestVerdict={resolvedVerdict?.verdict ?? "Waiting"}
                  confidenceScore={resolvedVerdict?.confidenceScore ?? null}
                  oneMinuteRsi={oneMinuteRsi}
                  macd={resolvedVerdict?.macd ?? null}
                  macdSignal={resolvedVerdict?.macdSignal ?? null}
                  momentum={resolvedVerdict?.momentum ?? null}
                  trendScore={resolvedVerdict?.trendScore ?? null}
                  adx={resolvedVerdict?.adx ?? null}
                  timeframeRsiMap={timeframeRsiMap}
                  verdict={resolvedVerdict}
                  calculations={calculations}
                  marketSignals={marketSignals}
                  nextOneMinuteProjection={nextOneMinuteProjection}
                  nextFiveMinuteProjection={nextFiveMinuteProjection}
                />
              </Card>
            </DashboardSideSection>

            <DashboardSideSection title="Automation">
              <AutomationDeskCard
                alertCount={notifications.alertRules.length}
                automationCount={tradeAutomation.rules.length}
                goalCount={goalAutomation.goals.length}
                onOpenAlerts={() => setIsNotificationsOpen(true)}
                onOpenGoals={() => setIsGoalAutomationOpen(true)}
              />
            </DashboardSideSection>

            <DashboardSideSection title="Trade activity">
              <Card className={styles.panelCard}>
              <Tabs
                className={styles.dashboardTabs}
                items={[
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
                defaultActiveKey="open-trade"
              />
              </Card>
            </DashboardSideSection>

            <IndicatorMonitorCard history={history} latest={latest} verdict={resolvedVerdict} />
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
        isVoiceConnecting={assistant.isVoiceConnecting}
        voiceStatus={assistant.voiceStatus}
        error={assistant.error}
        draft={assistant.draft}
        transcript={assistant.transcript}
        messages={assistant.messages}
        suggestedPrompts={assistant.suggestedPrompts}
        onClose={assistant.close}
        onDraftChange={assistant.setDraft}
        onSubmit={assistant.submitMessage}
        onStartListening={assistant.startListening}
        onStopListening={assistant.stopListening}
      />
    </div>
  );
}
