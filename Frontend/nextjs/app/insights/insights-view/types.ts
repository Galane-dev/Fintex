import type { LiveTrade } from "@/types/live-trading";
import type { NotificationItem } from "@/types/notifications";
import type { PaperPosition, PaperTradeFill } from "@/types/paper-trading";
import type { StrategyValidationResult } from "@/types/strategy-validation";
import type { UserProfile } from "@/types/user-profile";

export type ActivityItem = {
  id: string;
  kind: "paper-trade" | "live-trade" | "validation" | "alert";
  title: string;
  description: string;
  provider: string;
  occurredAt: string;
  tone: "positive" | "negative" | "neutral";
  valueLabel: string;
};

export type ProviderBreakdownItem = {
  label: string;
  count: number;
};

export type PnlPoint = {
  label: string;
  value: number;
};

export type ChartPoint = {
  label: string;
  value: number;
};

export type StrategyScoreItem = {
  id: number;
  label: string;
  timeframe: string;
  outcome: StrategyValidationResult["outcome"];
  score: number;
  createdAt: string;
};

export type InsightsProviderFilter = "All" | "Paper academy" | "Alpaca";
export type InsightsDateRangeFilter = "7D" | "30D" | "90D" | "All";

export type InsightsFilters = {
  provider: InsightsProviderFilter;
  dateRange: InsightsDateRangeFilter;
};

export type InsightsOverview = {
  closedTradeCount: number;
  openTradeCount: number;
  winRate: number | null;
  realizedPnl: number;
  averageStrategyScore: number | null;
  validatedStrategyRate: number | null;
  priceAlertHitCount: number;
  unreadNotificationCount: number;
};

export type InsightsChartMode = "equity" | "strategy-score" | "alert-hits";

export type InsightsDataset = {
  profile: UserProfile | null;
  openPaperPositions: PaperPosition[];
  closedPaperFills: PaperTradeFill[];
  openLiveTrades: LiveTrade[];
  closedLiveTrades: LiveTrade[];
  strategyHistory: StrategyValidationResult[];
  notifications: NotificationItem[];
  providerBreakdown: ProviderBreakdownItem[];
  pnlSeries: PnlPoint[];
  strategyScoreSeries: ChartPoint[];
  alertTimeline: ChartPoint[];
  recentActivity: ActivityItem[];
  strategyScores: StrategyScoreItem[];
  overview: InsightsOverview;
};
