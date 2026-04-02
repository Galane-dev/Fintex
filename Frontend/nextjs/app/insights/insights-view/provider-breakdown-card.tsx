"use client";

import { Card, Empty, Progress, Typography } from "antd";
import type { ProviderBreakdownItem } from "./types";
import { useInsightsStyles } from "../style";

export function ProviderBreakdownCard({
  items,
}: {
  items: ProviderBreakdownItem[];
}) {
  const { styles } = useInsightsStyles();
  const maxCount = Math.max(...items.map((item) => item.count), 1);

  return (
    <Card className={styles.panel}>
      <Typography.Title level={4} className={styles.panelTitle}>
        Provider mix
      </Typography.Title>

      {items.length === 0 ? (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No provider activity yet." />
      ) : (
        <div className={styles.barList}>
          {items.map((item) => (
            <div key={item.label} className={styles.row}>
              <div className={styles.rowLabel}>
                <span>{item.label}</span>
                <span>{item.count}</span>
              </div>
              <Progress
                percent={(item.count / maxCount) * 100}
                showInfo={false}
                strokeColor={item.label === "Alpaca" ? "#66c786" : "#9bf2b1"}
                trailColor="rgba(255,255,255,0.08)"
                size="small"
              />
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}
