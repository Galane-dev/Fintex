"use client";

import { Button, Empty, Space, Tag, Typography } from "antd";
import { formatPrice, formatTime } from "@/utils/market-data";
import type { PaperTradingPanelController } from "./types";
import { usePaperTradingStyles } from "../paper-trading-style";

interface PanelSectionsProps {
  controller: PaperTradingPanelController;
  currentPrice: number | null;
}

export const PanelSections = ({ controller, currentPrice }: PanelSectionsProps) => {
  const { styles, cx } = usePaperTradingStyles();

  if (!controller.account) {
    return (
      <div className={styles.section}>
        <Typography.Paragraph className={styles.helper}>
          Create your internal paper account or connect Alpaca to unlock trading. Buy, sell, recommendation, and account actions now live in the chart header so the workspace stays focused.
        </Typography.Paragraph>
        <Button type="primary" className={styles.actionButton} onClick={controller.openAccountsModal}>
          Open accounts
        </Button>
      </div>
    );
  }

  return (
    <>
      <div className={styles.section}>
        <div className={styles.sectionHeader}><span className={styles.sectionTitle}>{controller.account.name}</span><Tag color="green">{controller.account.baseCurrency}</Tag></div>
        <Typography.Paragraph className={styles.helper}>Latest Binance reference price: {currentPrice != null ? formatPrice(currentPrice) : "-"}. Account marked to market at {formatTime(controller.account.lastMarkedToMarketAt)}.</Typography.Paragraph>
        <div className={styles.inlineActions}><Button className={styles.actionButton} onClick={controller.openAccountsModal}>Manage account</Button><Button className={styles.actionButton} loading={controller.isBusy} onClick={() => void controller.openRecommendationModal()}>Get recommendation</Button></div>
        <div className={styles.metrics}>{controller.accountMetrics.map((metric) => <div key={metric.label} className={styles.metricCard}><div className={styles.metricLabel}>{metric.label}</div><div className={cx(styles.metricValue, metric.tone === "positive" ? styles.green : undefined, metric.tone === "negative" ? styles.red : undefined)}>{metric.value}</div></div>)}<div className={styles.metricCard}><div className={styles.metricLabel}>Live trades</div><div className={styles.metricValue}>{controller.liveTrades.filter((trade) => trade.status === "Open").length}</div></div></div>
      </div>

      <div className={styles.section}>
        <div className={styles.sectionHeader}><span className={styles.sectionTitle}>Open positions</span><Tag color="blue">{controller.positions.length}</Tag></div>
        {controller.positions.length === 0 ? <div className={styles.empty}><Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No paper positions are open yet." /></div> : <div className={styles.list}>{controller.positions.map((position) => <div key={position.id} className={styles.item}><div className={styles.itemTop}><span className={styles.itemTitle}>{position.symbol} {position.direction}</span><Space><Tag color={position.direction === "Buy" ? "green" : "red"}>{position.quantity.toFixed(4)}</Tag><Button size="small" loading={controller.isSubmitting} onClick={() => void controller.handleClosePaperPosition(position.id)}>Close</Button></Space></div><div className={styles.itemMeta}><span>Entry {formatPrice(position.averageEntryPrice)}</span><span>Mark {formatPrice(position.currentMarketPrice)}</span><span className={cx(position.unrealizedProfitLoss >= 0 ? styles.green : styles.red)}>U/P&amp;L {formatPrice(position.unrealizedProfitLoss)}</span></div></div>)}</div>}
      </div>

      <div className={styles.section}>
        <div className={styles.sectionHeader}><span className={styles.sectionTitle}>Recent orders</span><Tag color="default">{controller.orders.length}</Tag></div>
        {controller.orders.length === 0 ? <div className={styles.empty}><Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No paper orders yet." /></div> : <div className={styles.list}>{controller.orders.slice(0, 5).map((order) => <div key={order.id} className={styles.item}><div className={styles.itemTop}><span className={styles.itemTitle}>{order.symbol} {order.direction}</span><Tag color={order.status === "Filled" ? "green" : "blue"}>{order.status}</Tag></div><div className={styles.itemMeta}><span>Qty {order.quantity.toFixed(4)}</span><span>Fill {formatPrice(order.executedPrice)}</span><span>{formatTime(order.executedAt ?? order.submittedAt)}</span></div></div>)}</div>}
      </div>

      <div className={styles.section}>
        <div className={styles.sectionHeader}><span className={styles.sectionTitle}>Recent fills</span><Tag color="purple">{controller.fills.length}</Tag></div>
        {controller.fills.length === 0 ? <div className={styles.empty}><Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No paper fills yet." /></div> : <div className={styles.list}>{controller.fills.slice(0, 5).map((fill) => <div key={fill.id} className={styles.item}><div className={styles.itemTop}><span className={styles.itemTitle}>{fill.symbol} {fill.direction}</span><Tag color={fill.realizedProfitLoss >= 0 ? "green" : "red"}>{formatPrice(fill.realizedProfitLoss)}</Tag></div><div className={styles.itemMeta}><span>Qty {fill.quantity.toFixed(4)}</span><span>Price {formatPrice(fill.price)}</span><span>{formatTime(fill.executedAt)}</span></div></div>)}</div>}
      </div>
    </>
  );
};
