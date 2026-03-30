"use client";

import { Button, Empty, Space, Tag, Typography } from "antd";
import type { ClosedTradeReview, LiveTrade } from "@/types/live-trading";
import type { PaperPosition, PaperTradeFill } from "@/types/paper-trading";
import { formatPrice, formatTime } from "@/utils/market-data";
import {
  buildLiveClosedTradeReview,
  buildPaperClosedTradeReview,
} from "./closed-trade-review";
import { useStyles } from "../style";

interface TradeTabProps {
  mode: "open" | "closed";
  openPositions?: PaperPosition[];
  closedFills?: PaperTradeFill[];
  liveTrades: LiveTrade[];
  isPaperSubmitting: boolean;
  onClosePaperPosition?: (positionId: number) => void;
}

export function TradeTab({
  mode,
  openPositions = [],
  closedFills = [],
  liveTrades,
  isPaperSubmitting,
  onClosePaperPosition,
}: TradeTabProps) {
  const { styles, cx } = useStyles();
  const isOpenMode = mode === "open";
  const emptyCopy = isOpenMode
    ? "No open trades are active yet."
    : "Closed trades will appear here once positions are exited or synced.";

  if (
    (isOpenMode && openPositions.length === 0 && liveTrades.length === 0) ||
    (!isOpenMode && closedFills.length === 0 && liveTrades.length === 0)
  ) {
    return (
      <div className={styles.tabStack}>
        <div className={styles.emptyState}>
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={emptyCopy} />
        </div>
      </div>
    );
  }

  return (
    <div className={styles.tabStack}>
      <div className={styles.positionsList}>
        {isOpenMode
          ? openPositions.map((position) => (
              <div key={position.id} className={styles.positionCard}>
                <div className={styles.positionHeader}>
                  <div>
                    <div className={styles.positionTitle}>{position.symbol} {position.direction}</div>
                    <div className={styles.positionSubtle}>Opened {formatTime(position.openedAt)}</div>
                  </div>

                  <Space wrap>
                    <Tag color="default">Paper academy</Tag>
                    <Tag color={position.direction === "Buy" ? "green" : "red"}>{position.quantity.toFixed(4)}</Tag>
                    <Button size="small" loading={isPaperSubmitting} onClick={() => onClosePaperPosition?.(position.id)}>Close</Button>
                  </Space>
                </div>

                <div className={styles.positionMetrics}>
                  <Metric label="Entry" value={formatPrice(position.averageEntryPrice)} />
                  <Metric label="Mark" value={formatPrice(position.currentMarketPrice)} />
                  <Metric label="Stop loss" value={formatPrice(position.stopLoss)} />
                  <Metric label="Take profit" value={formatPrice(position.takeProfit)} />
                  <Metric
                    label="Unrealized P/L"
                    value={formatPrice(position.unrealizedProfitLoss)}
                    className={position.unrealizedProfitLoss >= 0 ? styles.positive : styles.negative}
                  />
                </div>
              </div>
            ))
          : closedFills.map((fill) => (
              <div key={fill.id} className={styles.positionCard}>
                <div className={styles.positionHeader}>
                  <div>
                    <div className={styles.positionTitle}>{fill.symbol} {fill.direction}</div>
                    <div className={styles.positionSubtle}>Closed {formatTime(fill.executedAt)}</div>
                  </div>

                  <Space wrap>
                    <Tag color={fill.realizedProfitLoss >= 0 ? "green" : "red"}>{formatPrice(fill.realizedProfitLoss)}</Tag>
                    <Tag color="default">Paper academy</Tag>
                  </Space>
                </div>

                <div className={styles.positionMetrics}>
                  <Metric label="Quantity" value={fill.quantity.toFixed(4)} />
                  <Metric label="Exit price" value={formatPrice(fill.price)} />
                  <Metric
                    label="Realized P/L"
                    value={formatPrice(fill.realizedProfitLoss)}
                    className={fill.realizedProfitLoss >= 0 ? styles.positive : styles.negative}
                  />
                </div>

                <ClosedTradeReviewPanel
                  review={buildPaperClosedTradeReview(fill, closedFills, liveTrades)}
                />
              </div>
            ))}

        {liveTrades.map((trade) => (
          <div key={`${mode}-${trade.id}`} className={styles.positionCard}>
            <div className={styles.positionHeader}>
              <div>
                <div className={styles.positionTitle}>{trade.symbol} {trade.direction}</div>
                <div className={styles.positionSubtle}>
                  {isOpenMode ? "Opened" : "Closed"} {formatTime(isOpenMode ? trade.executedAt : (trade.closedAt ?? trade.executedAt))}
                </div>
              </div>

              <Space wrap>
                <Tag color="blue">Alpaca</Tag>
                <Tag color={trade.direction === "Buy" ? "green" : "red"}>{trade.quantity.toFixed(4)}</Tag>
              </Space>
            </div>

            <div className={styles.positionMetrics}>
              <Metric label="Entry" value={formatPrice(trade.entryPrice)} />
              <Metric label={isOpenMode ? "Mark" : "Exit"} value={formatPrice(isOpenMode ? trade.lastMarketPrice : trade.exitPrice)} />
              <Metric label="Stop loss" value={formatPrice(trade.stopLoss)} />
              <Metric label="Take profit" value={formatPrice(trade.takeProfit)} />
              <Metric
                label={isOpenMode ? "Unrealized P/L" : "Realized P/L"}
                value={formatPrice(isOpenMode ? trade.unrealizedProfitLoss : trade.realizedProfitLoss)}
                className={cx(styles.positionMetricValue, (isOpenMode ? trade.unrealizedProfitLoss : trade.realizedProfitLoss) >= 0 ? styles.positive : styles.negative)}
              />
            </div>

            {!isOpenMode ? (
              <ClosedTradeReviewPanel
                review={
                  trade.closedTradeReview ??
                  buildLiveClosedTradeReview(trade, closedFills, liveTrades)
                }
              />
            ) : null}
          </div>
        ))}
      </div>
    </div>
  );
}

function ClosedTradeReviewPanel({
  review,
}: {
  review: {
    good: string;
    bad: string;
    repeatedPattern: string;
  } | ClosedTradeReview;
}) {
  const { styles } = useStyles();

  return (
    <div className={styles.closedTradeReviewPanel}>
      <ReviewLine label="What was good" copy={review.good} />
      <ReviewLine label="What was bad" copy={review.bad} />
      <ReviewLine label="Repeated pattern" copy={review.repeatedPattern} />
    </div>
  );
}

function ReviewLine({ label, copy }: { label: string; copy: string }) {
  const { styles } = useStyles();

  return (
    <div className={styles.closedTradeReviewLine}>
      <div className={styles.closedTradeReviewLabel}>{label}</div>
      <Typography.Paragraph className={styles.closedTradeReviewCopy}>
        {copy}
      </Typography.Paragraph>
    </div>
  );
}

function Metric({ label, value, className }: { label: string; value: string; className?: string }) {
  const { styles, cx } = useStyles();

  return (
    <div className={styles.positionMetric}>
      <span className={styles.positionMetricLabel}>{label}</span>
      <span className={cx(styles.positionMetricValue, className)}>{value}</span>
    </div>
  );
}
