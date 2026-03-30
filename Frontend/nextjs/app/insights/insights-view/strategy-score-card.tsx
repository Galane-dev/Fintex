"use client";

import { Card, Empty, Progress, Tag, Typography } from "antd";
import { formatTime } from "@/utils/market-data";
import type { StrategyScoreItem } from "./types";
import { useInsightsStyles } from "../style";

const getOutcomeColor = (outcome: StrategyScoreItem["outcome"]) => {
  if (outcome === "Validated") {
    return "green";
  }

  return outcome === "Fail" ? "red" : "gold";
};

export function StrategyScoreCard({
  items,
  validatedRate,
}: {
  items: StrategyScoreItem[];
  validatedRate: number | null;
}) {
  const { styles } = useInsightsStyles();

  return (
    <Card className={styles.panel}>
      <Typography.Title level={4} className={styles.panelTitle}>
        Strategy scorecards
      </Typography.Title>
      {validatedRate != null ? (
        <div className={styles.statRow}>
          <div className={styles.statPill}>
            <span className={styles.statLabel}>Validated</span>
            <span className={styles.statValue}>{validatedRate.toFixed(1)}%</span>
          </div>
        </div>
      ) : null}

      {items.length === 0 ? (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No validation runs yet." />
      ) : (
        <div className={styles.list}>
          {items.map((item) => (
            <div key={item.id} className={styles.listItem}>
              <div className={styles.itemHeader}>
                <div className={styles.itemTitle}>{item.label}</div>
                <Tag color={getOutcomeColor(item.outcome)}>{item.score.toFixed(1)}</Tag>
              </div>
              <Progress
                percent={item.score}
                showInfo={false}
                strokeColor={item.outcome === "Validated" ? "#9bf2b1" : item.outcome === "Fail" ? "#ff7875" : "#ffd666"}
                trailColor="rgba(255,255,255,0.08)"
                size="small"
              />
              <div className={styles.itemMeta}>
                <span>{item.timeframe}</span>
                <span>{item.outcome}</span>
                <span>{formatTime(item.createdAt)}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}
