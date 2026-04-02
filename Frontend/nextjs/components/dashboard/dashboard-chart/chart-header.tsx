"use client";

import { BulbOutlined, FundProjectionScreenOutlined, RadarChartOutlined, WalletOutlined } from "@ant-design/icons";
import { Button, Segmented, Space, Tag, Tooltip, Typography } from "antd";
import { formatPrice, formatSigned, formatTime } from "@/utils/market-data";
import { intervals } from "./types";
import type { DashboardChartController, DashboardChartProps } from "./types";
import { useStyles } from "../style";

interface ChartHeaderProps extends Pick<DashboardChartProps, "onOpenAccounts" | "onOpenBehaviorAnalysis" | "onOpenRecommendation" | "onOpenStrategyValidation" | "onOpenTrade" | "symbol" | "venue"> {
  controller: DashboardChartController;
}

export const ChartHeader = ({
  controller,
  onOpenAccounts,
  onOpenBehaviorAnalysis,
  onOpenRecommendation,
  onOpenStrategyValidation,
  onOpenTrade,
  symbol,
  venue,
}: ChartHeaderProps) => {
  const { styles, cx } = useStyles();

  return (
    <div className={styles.header}>
      <div className={styles.symbolRow}>
        <div className={styles.symbolWrap}>
          <Typography.Text className={styles.symbol}>{symbol}</Typography.Text>
          <Typography.Text className={styles.price}>{formatPrice(controller.lastVisibleCandle?.close)}</Typography.Text>
          <Tag color={controller.isPositive ? "green" : "red"}>{formatSigned(controller.priceChange)}%</Tag>
          <Tag>{venue}</Tag>
          <Tag color={controller.status === "live" ? "green" : controller.status === "reconnecting" || controller.status === "loading" ? "gold" : "red"}>{controller.status}</Tag>
          {controller.visualSpreadBand ? <Tag color="purple">Spread {formatPrice(controller.visualSpreadBand.width)}{controller.visualSpreadBand.isSimulated ? " visual" : ""}</Tag> : null}
        </div>
        <Segmented options={intervals} value={controller.interval} onChange={(value) => controller.setInterval(value as typeof controller.interval)} />
      </div>

      <div className={styles.actionBar}>
        <Space wrap size={10}>
          <Button type="primary" className={cx(styles.actionButton, styles.buyButton)} onClick={() => onOpenTrade("Buy")}>Buy</Button>
          <Button danger className={cx(styles.actionButton, styles.sellButton)} onClick={() => onOpenTrade("Sell")}>Sell</Button>
          <Button icon={<WalletOutlined />} className={styles.actionButton} onClick={onOpenAccounts}>Accounts</Button>
          <Tooltip title="Get recommendation">
            <Button
              aria-label="Get recommendation"
              icon={<BulbOutlined />}
              className={cx(styles.actionButton, styles.iconActionButton)}
              onClick={onOpenRecommendation}
            />
          </Tooltip>
          <Tooltip title="Validate strategy">
            <Button
              aria-label="Validate strategy"
              icon={<FundProjectionScreenOutlined />}
              className={cx(styles.actionButton, styles.iconActionButton)}
              onClick={onOpenStrategyValidation}
            />
          </Tooltip>
          <Tooltip title="My behavior analysis">
            <Button
              aria-label="My behavior analysis"
              icon={<RadarChartOutlined />}
              className={cx(styles.actionButton, styles.iconActionButton)}
              onClick={onOpenBehaviorAnalysis}
            />
          </Tooltip>
        </Space>
      </div>

      {controller.lastVisibleCandle ? (
        <div className={styles.liveMetaRow}>
          <Typography.Text type="secondary">Last candle close: {formatTime(new Date(controller.lastVisibleCandle.closeTime).toISOString())}</Typography.Text>
          <Typography.Text type="secondary">Interval: {controller.interval}</Typography.Text>
        </div>
      ) : null}
    </div>
  );
};
