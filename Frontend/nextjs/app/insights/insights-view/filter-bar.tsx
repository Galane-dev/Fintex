"use client";

import { CalendarOutlined, FilterOutlined } from "@ant-design/icons";
import { Segmented, Space, Typography } from "antd";
import type {
  InsightsDateRangeFilter,
  InsightsProviderFilter,
} from "./types";
import { useInsightsStyles } from "../style";

type FilterBarProps = {
  provider: InsightsProviderFilter;
  dateRange: InsightsDateRangeFilter;
  onProviderChange: (value: InsightsProviderFilter) => void;
  onDateRangeChange: (value: InsightsDateRangeFilter) => void;
};

export function FilterBar({
  provider,
  dateRange,
  onProviderChange,
  onDateRangeChange,
}: FilterBarProps) {
  const { styles } = useInsightsStyles();

  return (
    <div className={styles.filterBar}>
      <div className={styles.filterGroup}>
        <Space size={8}>
          <FilterOutlined />
          <Typography.Text className={styles.filterLabel}>Provider</Typography.Text>
        </Space>
        <Segmented
          value={provider}
          options={["All", "Paper academy", "Alpaca"]}
          onChange={(value) => onProviderChange(value as InsightsProviderFilter)}
        />
      </div>

      <div className={styles.filterGroup}>
        <Space size={8}>
          <CalendarOutlined />
          <Typography.Text className={styles.filterLabel}>Date range</Typography.Text>
        </Space>
        <Segmented
          value={dateRange}
          options={["7D", "30D", "90D", "All"]}
          onChange={(value) => onDateRangeChange(value as InsightsDateRangeFilter)}
        />
      </div>
    </div>
  );
}
