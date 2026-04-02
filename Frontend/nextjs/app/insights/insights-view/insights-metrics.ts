import type { LiveTrade } from "@/types/live-trading";
import type { NotificationItem } from "@/types/notifications";
import type { PaperPosition, PaperTradeFill } from "@/types/paper-trading";
import type { StrategyValidationResult } from "@/types/strategy-validation";
import type { UserProfile } from "@/types/user-profile";
import type {
  ActivityItem,
  ChartPoint,
  InsightsFilters,
  InsightsDataset,
  InsightsOverview,
  PnlPoint,
  ProviderBreakdownItem,
  StrategyScoreItem,
} from "./types";

const sortByDateDesc = <T>(items: T[], getValue: (item: T) => string) =>
  [...items].sort(
    (left, right) =>
      new Date(getValue(right)).getTime() - new Date(getValue(left)).getTime(),
  );

const normalizeOutcome = (
  outcome: StrategyValidationResult["outcome"] | number | string,
): StrategyValidationResult["outcome"] => {
  if (outcome === "Validated" || outcome === 2 || outcome === "2") {
    return "Validated";
  }

  if (outcome === "Caution" || outcome === 1 || outcome === "1") {
    return "Caution";
  }

  return "Fail";
};

const buildOverview = (
  closedPaperFills: PaperTradeFill[],
  closedLiveTrades: LiveTrade[],
  openPaperPositions: PaperPosition[],
  openLiveTrades: LiveTrade[],
  strategyHistory: StrategyValidationResult[],
  notifications: NotificationItem[],
): InsightsOverview => {
  const closedTradeCount = closedPaperFills.length + closedLiveTrades.length;
  const realizedPnl =
    closedPaperFills.reduce((sum, fill) => sum + fill.realizedProfitLoss, 0) +
    closedLiveTrades.reduce((sum, trade) => sum + trade.realizedProfitLoss, 0);
  const winningTrades =
    closedPaperFills.filter((fill) => fill.realizedProfitLoss > 0).length +
    closedLiveTrades.filter((trade) => trade.realizedProfitLoss > 0).length;
  const averageStrategyScore =
    strategyHistory.length > 0
      ? strategyHistory.reduce((sum, item) => sum + item.validationScore, 0) /
        strategyHistory.length
      : null;
  const validatedStrategyRate =
    strategyHistory.length > 0
      ? (strategyHistory.filter((item) => item.outcome === "Validated").length /
          strategyHistory.length) *
        100
      : null;

  return {
    closedTradeCount,
    openTradeCount: openPaperPositions.length + openLiveTrades.length,
    winRate: closedTradeCount > 0 ? (winningTrades / closedTradeCount) * 100 : null,
    realizedPnl,
    averageStrategyScore,
    validatedStrategyRate,
    priceAlertHitCount: notifications.filter((item) => item.type === "PriceTarget").length,
    unreadNotificationCount: notifications.filter((item) => !item.isRead).length,
  };
};

const buildProviderBreakdown = (
  openPaperPositions: PaperPosition[],
  closedPaperFills: PaperTradeFill[],
  openLiveTrades: LiveTrade[],
  closedLiveTrades: LiveTrade[],
): ProviderBreakdownItem[] => {
  const paperCount = openPaperPositions.length + closedPaperFills.length;
  const alpacaCount = openLiveTrades.length + closedLiveTrades.length;

  return [
    { label: "Paper academy", count: paperCount },
    { label: "Alpaca", count: alpacaCount },
  ].filter((item) => item.count > 0);
};

const buildPnlSeries = (
  closedPaperFills: PaperTradeFill[],
  closedLiveTrades: LiveTrade[],
): PnlPoint[] => {
  const events = sortByDateDesc(
    [
      ...closedPaperFills.map((fill) => ({
        date: fill.executedAt,
        pnl: fill.realizedProfitLoss,
      })),
      ...closedLiveTrades.map((trade) => ({
        date: trade.closedAt ?? trade.executedAt,
        pnl: trade.realizedProfitLoss,
      })),
    ],
    (item) => item.date,
  ).reverse();

  let runningTotal = 0;
  return events.slice(-12).map((item) => {
    runningTotal += item.pnl;
    return {
      label: item.date,
      value: runningTotal,
    };
  });
};

const buildStrategyScores = (
  strategyHistory: StrategyValidationResult[],
): StrategyScoreItem[] =>
  sortByDateDesc(strategyHistory, (item) => item.creationTime)
    .slice(0, 6)
    .map((item) => ({
      id: item.id,
      label: item.strategyName || "Unnamed strategy",
      timeframe: item.timeframe || "1m",
      outcome: normalizeOutcome(item.outcome as StrategyValidationResult["outcome"] | number | string),
      score: item.validationScore,
      createdAt: item.creationTime,
    }));

const buildStrategyScoreSeries = (
  strategyHistory: StrategyValidationResult[],
): ChartPoint[] =>
  sortByDateDesc(strategyHistory, (item) => item.creationTime)
    .slice(0, 12)
    .reverse()
    .map((item) => ({
      label: item.creationTime,
      value: item.validationScore,
    }));

const buildAlertTimeline = (
  notifications: NotificationItem[],
): ChartPoint[] => {
  const alerts = sortByDateDesc(
    notifications.filter((item) => item.type === "PriceTarget"),
    (item) => item.occurredAt,
  )
    .slice(0, 12)
    .reverse();

  let runningHits = 0;
  return alerts.map((item) => {
    runningHits += 1;
    return {
      label: item.occurredAt,
      value: runningHits,
    };
  });
};

const buildRecentActivity = (
  closedPaperFills: PaperTradeFill[],
  closedLiveTrades: LiveTrade[],
  strategyHistory: StrategyValidationResult[],
  notifications: NotificationItem[],
): ActivityItem[] => {
  const pnlTone = (value: number): ActivityItem["tone"] =>
    value > 0 ? "positive" : value < 0 ? "negative" : "neutral";
  const severityTone = (value: NotificationItem["severity"]): ActivityItem["tone"] =>
    value === "Success" ? "positive" : value === "Danger" ? "negative" : "neutral";
  const outcomeTone = (
    value: StrategyValidationResult["outcome"] | number | string,
  ): ActivityItem["tone"] =>
    value === "Validated" ? "positive" : value === "Fail" ? "negative" : "neutral";

  const activities: ActivityItem[] = [
    ...closedPaperFills.map((fill) => ({
      id: `paper-${fill.id}`,
      kind: "paper-trade" as const,
      title: `${fill.symbol} ${fill.direction} closed`,
      description: "Paper academy trade realized.",
      provider: "Paper academy",
      occurredAt: fill.executedAt,
      tone: pnlTone(fill.realizedProfitLoss),
      valueLabel: `${fill.realizedProfitLoss >= 0 ? "+" : ""}${fill.realizedProfitLoss.toFixed(2)}`,
    })),
    ...closedLiveTrades.map((trade) => ({
      id: `live-${trade.id}`,
      kind: "live-trade" as const,
      title: `${trade.symbol} ${trade.direction} closed`,
      description: "Live broker trade synced from Alpaca.",
      provider: "Alpaca",
      occurredAt: trade.closedAt ?? trade.executedAt,
      tone: pnlTone(trade.realizedProfitLoss),
      valueLabel: `${trade.realizedProfitLoss >= 0 ? "+" : ""}${trade.realizedProfitLoss.toFixed(2)}`,
    })),
    ...strategyHistory.map((item) => ({
      id: `strategy-${item.id}`,
      kind: "validation" as const,
      title: item.strategyName || "Strategy validation",
      description: item.summary,
      provider: "Validator",
      occurredAt: item.creationTime,
      tone: outcomeTone(normalizeOutcome(item.outcome as StrategyValidationResult["outcome"] | number | string)),
      valueLabel: `${item.validationScore.toFixed(1)} / 100`,
    })),
    ...notifications.map((item) => ({
      id: `notification-${item.id}`,
      kind: "alert" as const,
      title: item.title,
      description: item.message,
      provider: item.provider || "Fintex",
      occurredAt: item.occurredAt,
      tone: severityTone(item.severity),
      valueLabel: item.targetPrice != null ? item.targetPrice.toFixed(2) : item.verdict || "Alert",
    })),
  ];

  return sortByDateDesc(activities, (item) => item.occurredAt).slice(0, 40);
};

export const buildInsightsDataset = ({
  profile,
  openPaperPositions,
  closedPaperFills,
  openLiveTrades,
  closedLiveTrades,
  strategyHistory,
  notifications,
}: {
  profile: UserProfile | null;
  openPaperPositions: PaperPosition[];
  closedPaperFills: PaperTradeFill[];
  openLiveTrades: LiveTrade[];
  closedLiveTrades: LiveTrade[];
  strategyHistory: StrategyValidationResult[];
  notifications: NotificationItem[];
}): InsightsDataset => ({
  profile,
  openPaperPositions,
  closedPaperFills,
  openLiveTrades,
  closedLiveTrades,
  strategyHistory,
  notifications,
  providerBreakdown: buildProviderBreakdown(
    openPaperPositions,
    closedPaperFills,
    openLiveTrades,
    closedLiveTrades,
  ),
  pnlSeries: buildPnlSeries(closedPaperFills, closedLiveTrades),
  strategyScoreSeries: buildStrategyScoreSeries(strategyHistory),
  alertTimeline: buildAlertTimeline(notifications),
  recentActivity: buildRecentActivity(
    closedPaperFills,
    closedLiveTrades,
    strategyHistory,
    notifications,
  ),
  strategyScores: buildStrategyScores(strategyHistory),
  overview: buildOverview(
    closedPaperFills,
    closedLiveTrades,
    openPaperPositions,
    openLiveTrades,
    strategyHistory,
    notifications,
  ),
});

const getRangeStart = (dateRange: InsightsFilters["dateRange"]) => {
  if (dateRange === "All") {
    return null;
  }

  const now = Date.now();
  const days =
    dateRange === "7D" ? 7 : dateRange === "30D" ? 30 : 90;
  return now - days * 24 * 60 * 60 * 1000;
};

const isWithinRange = (value: string, rangeStart: number | null) =>
  rangeStart == null || new Date(value).getTime() >= rangeStart;

export const filterInsightsDataset = (
  dataset: InsightsDataset,
  filters: InsightsFilters,
): InsightsDataset => {
  const rangeStart = getRangeStart(filters.dateRange);
  const includePaper = filters.provider === "All" || filters.provider === "Paper academy";
  const includeAlpaca = filters.provider === "All" || filters.provider === "Alpaca";

  return buildInsightsDataset({
    profile: dataset.profile,
    openPaperPositions: includePaper
      ? dataset.openPaperPositions.filter((item) => isWithinRange(item.openedAt, rangeStart))
      : [],
    closedPaperFills: includePaper
      ? dataset.closedPaperFills.filter((item) => isWithinRange(item.executedAt, rangeStart))
      : [],
    openLiveTrades: includeAlpaca
      ? dataset.openLiveTrades.filter((item) => isWithinRange(item.executedAt, rangeStart))
      : [],
    closedLiveTrades: includeAlpaca
      ? dataset.closedLiveTrades.filter((item) =>
          isWithinRange(item.closedAt ?? item.executedAt, rangeStart),
        )
      : [],
    strategyHistory: dataset.strategyHistory.filter((item) =>
      isWithinRange(item.creationTime, rangeStart),
    ),
    notifications: dataset.notifications.filter((item) =>
      isWithinRange(item.occurredAt, rangeStart),
    ),
  });
};
