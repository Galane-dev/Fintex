"use client";

import { Card, Empty, Tag, Typography } from "antd";
import { formatTime } from "@/utils/market-data";
import type { ActivityItem } from "./types";
import { useInsightsStyles } from "../style";

const toneToColor = (tone: ActivityItem["tone"]) => {
  if (tone === "positive") {
    return "green";
  }

  return tone === "negative" ? "red" : "default";
};

export function ActivityFeedCard({ items }: { items: ActivityItem[] }) {
  const { styles } = useInsightsStyles();
  const visibleItems = items.slice(0, 10);

  return (
    <Card className={styles.panel}>
      <Typography.Title level={4} className={styles.panelTitle}>
        Recent actions and alerts
      </Typography.Title>
      <Typography.Paragraph className={styles.metaRow}>
        A compact stream of trade closures, strategy validations, and notification events stored in Fintex.
      </Typography.Paragraph>

      {visibleItems.length === 0 ? (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Activity will appear here as you use the platform." />
      ) : (
        <div className={styles.list}>
          {visibleItems.map((item) => (
            <div key={item.id} className={styles.timelineItem}>
              <div className={styles.timelineDot} />
              <div className={styles.itemHeader}>
                <div>
                  <div className={styles.itemTitle}>{item.title}</div>
                  <div className={styles.itemMeta}>
                    <span>{item.provider}</span>
                    <span>{formatTime(item.occurredAt)}</span>
                  </div>
                </div>
                <Tag color={toneToColor(item.tone)}>{item.valueLabel}</Tag>
              </div>
              <div className={styles.itemCopy}>{item.description}</div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}
