"use client";

import {
  BellOutlined,
  FundProjectionScreenOutlined,
  LineChartOutlined,
  RiseOutlined,
  TrophyOutlined,
} from "@ant-design/icons";
import { Progress, Typography } from "antd";
import { formatPercent, formatPrice } from "@/utils/market-data";
import type { InsightsOverview } from "./types";
import { useInsightsStyles } from "../style";

export function OverviewCards({
  overview,
  latestMarketPrice,
}: {
  overview: InsightsOverview;
  latestMarketPrice: number | null;
}) {
  const { styles } = useInsightsStyles();
  const items = [
    {
      label: "Market price",
      value: formatPrice(latestMarketPrice),
      note: "BTCUSDT",
      icon: <LineChartOutlined />,
      accent: 72,
    },
    {
      label: "Success rate",
      value: overview.winRate != null ? formatPercent(overview.winRate, 1) : "-",
      note: `${overview.closedTradeCount} closed`,
      icon: <TrophyOutlined />,
      accent: overview.winRate ?? 0,
    },
    {
      label: "Realized P/L",
      value: formatPrice(overview.realizedPnl),
      note: "Closed outcomes",
      icon: <RiseOutlined />,
      accent: overview.realizedPnl >= 0 ? 68 : 32,
    },
    {
      label: "Avg strategy score",
      value:
        overview.averageStrategyScore != null
          ? overview.averageStrategyScore.toFixed(1)
          : "-",
      note: "Validator",
      icon: <FundProjectionScreenOutlined />,
      accent: overview.averageStrategyScore ?? 0,
    },
    {
      label: "Alerts and unread",
      value: `${overview.priceAlertHitCount} / ${overview.unreadNotificationCount}`,
      note: "Hits / unread",
      icon: <BellOutlined />,
      accent: overview.unreadNotificationCount > 0 ? 80 : 45,
    },
  ];

  return (
    <div className={styles.cardsGrid}>
      {items.map((item) => (
        <div key={item.label} className={styles.overviewCard}>
          <div className={styles.overviewTopRow}>
            <div className={styles.overviewIcon}>{item.icon}</div>
            <div className={styles.overviewLabel}>{item.label}</div>
          </div>
          <div className={styles.overviewValue}>{item.value}</div>
          <div className={styles.overviewBottomRow}>
            <Typography.Text className={styles.overviewNote}>{item.note}</Typography.Text>
            <Progress
              percent={Math.max(0, Math.min(item.accent, 100))}
              showInfo={false}
              strokeColor="#9bf2b1"
              trailColor="rgba(255,255,255,0.08)"
              size="small"
              className={styles.overviewProgress}
            />
          </div>
        </div>
      ))}
    </div>
  );
}
