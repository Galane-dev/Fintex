"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import {
  ApartmentOutlined,
  AppstoreOutlined,
  BarChartOutlined,
  CheckCircleOutlined,
  DashboardOutlined,
  DotChartOutlined,
  FundOutlined,
  LogoutOutlined,
  ProfileOutlined,
  ReloadOutlined,
  ThunderboltOutlined,
} from "@ant-design/icons";
import { Alert, Button, Space, Tag, Typography } from "antd";
import { getFintexButtonLoading } from "@/components/fintex-loader";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import { ActivityFeedCard } from "./activity-feed-card";
import { BehaviorSummaryCard } from "./behavior-summary-card";
import { FilterBar } from "./filter-bar";
import { OverviewCards } from "./overview-cards";
import { PnlChartCard } from "./pnl-chart-card";
import { ProviderBreakdownCard } from "./provider-breakdown-card";
import { StrategyScoreCard } from "./strategy-score-card";
import { VisualAnalyticsCard } from "./visual-analytics-card";
import { filterInsightsDataset } from "./insights-metrics";
import type { InsightsDateRangeFilter, InsightsProviderFilter } from "./types";
import { useInsightsPageData } from "./use-insights-page-data";
import { useInsightsStyles } from "../style";

export function InsightsContent() {
  const { styles } = useInsightsStyles();
  const { signOut } = useAuth();
  const {
    connectionStatus,
    dataset,
    error,
    isLoading,
    latestMarketPrice,
    latestVerdict,
    marketConfidence,
    refresh,
  } = useInsightsPageData();
  const [providerFilter, setProviderFilter] = useState<InsightsProviderFilter>("All");
  const [dateRangeFilter, setDateRangeFilter] = useState<InsightsDateRangeFilter>("30D");

  const filteredDataset = useMemo(
    () =>
      filterInsightsDataset(dataset, {
        provider: providerFilter,
        dateRange: dateRangeFilter,
      }),
    [dataset, dateRangeFilter, providerFilter],
  );

  const marketMeta = useMemo(
    () => [
      `Connection ${connectionStatus.toLowerCase()}`,
      `Verdict ${latestVerdict}`,
      `Confidence ${marketConfidence != null ? marketConfidence.toFixed(1) : "-"}`,
    ],
    [connectionStatus, latestVerdict, marketConfidence],
  );

  const navItems = [
    { id: "insights-overview", label: "Overview", icon: <AppstoreOutlined /> },
    { id: "insights-visuals", label: "Visuals", icon: <BarChartOutlined /> },
    { id: "insights-performance", label: "Performance", icon: <DotChartOutlined /> },
    { id: "insights-profile", label: "Behavior", icon: <ProfileOutlined /> },
    { id: "insights-strategy", label: "Strategy", icon: <FundOutlined /> },
  ];

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <div className={styles.pageLayout}>
          <aside className={styles.leftNav}>
            <div className={styles.leftNavTitle}>Insights</div>
            <div className={styles.leftNavList}>
              {navItems.map((item) => (
                <a key={item.id} href={`#${item.id}`} className={styles.leftNavLink}>
                  {item.icon}
                  <span>{item.label}</span>
                </a>
              ))}
            </div>
          </aside>

          <div className={styles.mainPane}>
            <div className={styles.header}>
              <div className={styles.titleWrap}>
                <div className={styles.eyebrow}>Review center</div>
                <Typography.Title level={2} className={styles.title}>
                  Your trading story at a glance
                </Typography.Title>
                <Typography.Paragraph className={styles.subtitle}>
                  A premium review workspace for outcomes, habits, strategy quality, and alert history.
                </Typography.Paragraph>
              </div>

              <div className={styles.actions}>
                <Button
                  icon={<ReloadOutlined />}
                  loading={getFintexButtonLoading(isLoading)}
                  onClick={() => void refresh()}
                />
                <Link href={ROUTES.dashboard}>
                  <Button icon={<DashboardOutlined />} />
                </Link>
                <Link href={ROUTES.home}>
                  <Button className={styles.brandHomeButton} aria-label="Fintex home" title="Fintex home">
                    F
                  </Button>
                </Link>
                <Button type="primary" icon={<LogoutOutlined />} onClick={signOut} />
              </div>
            </div>

            {error ? <Alert type="warning" showIcon message={error} style={{ marginBottom: 16 }} /> : null}

            <div className={styles.heroGrid}>
              <div className={styles.heroPanel}>
                <div>
                  <div className={styles.heroEyebrow}>Live context</div>
                  <Typography.Title level={4} className={styles.heroTitle}>
                    Review mode synced with the market
                  </Typography.Title>
                </div>
                <Space wrap>
                  <Tag icon={<ThunderboltOutlined />}>{marketMeta[0]}</Tag>
                  <Tag icon={<CheckCircleOutlined />}>{marketMeta[1]}</Tag>
                  <Tag>{marketMeta[2]}</Tag>
                </Space>
              </div>

              <div className={styles.spotlightCard}>
                <div className={styles.spotlightIcon}>
                  <ApartmentOutlined />
                </div>
                <div className={styles.spotlightBody}>
                  <div className={styles.spotlightLabel}>Active lens</div>
                  <div className={styles.spotlightValue}>
                    {providerFilter} | {dateRangeFilter}
                  </div>
                  <div className={styles.spotlightNote}>
                    Trade-focused visuals respond to your provider toggle while strategy and behavior remain app-wide
                    and date-aware.
                  </div>
                </div>
              </div>
            </div>

            <FilterBar
              provider={providerFilter}
              dateRange={dateRangeFilter}
              onProviderChange={setProviderFilter}
              onDateRangeChange={setDateRangeFilter}
            />

            <section id="insights-overview">
              <OverviewCards overview={filteredDataset.overview} latestMarketPrice={latestMarketPrice} />
            </section>

            <section id="insights-visuals">
              <VisualAnalyticsCard
                overview={filteredDataset.overview}
                providerBreakdown={filteredDataset.providerBreakdown}
                strategyScores={filteredDataset.strategyScores}
                recentActivity={filteredDataset.recentActivity}
                strategyScoreSeries={filteredDataset.strategyScoreSeries}
              />
            </section>

            <div className={styles.layout}>
              <div className={styles.column}>
                <section id="insights-performance">
                  <PnlChartCard
                    equityPoints={filteredDataset.pnlSeries}
                    strategyScorePoints={filteredDataset.strategyScoreSeries}
                    alertPoints={filteredDataset.alertTimeline}
                  />
                </section>
                <ActivityFeedCard items={filteredDataset.recentActivity} />
              </div>

              <div className={`${styles.column} ${styles.stickySidebar}`}>
                <section id="insights-profile">
                  <BehaviorSummaryCard profile={filteredDataset.profile} />
                </section>
                <ProviderBreakdownCard items={filteredDataset.providerBreakdown} />
                <section id="insights-strategy">
                  <StrategyScoreCard
                    items={filteredDataset.strategyScores}
                    validatedRate={filteredDataset.overview.validatedStrategyRate}
                  />
                </section>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
