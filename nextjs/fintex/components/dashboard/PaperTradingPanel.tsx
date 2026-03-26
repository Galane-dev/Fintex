"use client";

import { useMemo, useState } from "react";
import {
  Alert,
  Button,
  Empty,
  Form,
  Input,
  InputNumber,
  Skeleton,
  Space,
  Tag,
  Typography,
} from "antd";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import { formatPrice, formatTime } from "@/utils/market-data";
import { usePaperTradingStyles } from "./paper-trading-style";

interface PaperTradingPanelProps {
  currentPrice: number | null;
}

export function PaperTradingPanel({ currentPrice }: PaperTradingPanelProps) {
  const { styles, cx } = usePaperTradingStyles();
  const {
    snapshot,
    isLoading,
    isSubmitting,
    error,
    createAccount,
    placeOrder,
    closePosition,
    clearError,
  } = usePaperTrading();
  const [accountForm] = Form.useForm();
  const [tradeForm] = Form.useForm();
  const [tradeDirection, setTradeDirection] = useState<"Buy" | "Sell">("Buy");

  const account = snapshot?.account ?? null;
  const positions = snapshot?.positions ?? [];
  const orders = snapshot?.recentOrders ?? [];
  const fills = snapshot?.recentFills ?? [];

  const accountMetrics = useMemo(
    () =>
      account
        ? [
            {
              label: "Cash balance",
              value: formatPrice(account.cashBalance),
              tone: "neutral",
            },
            {
              label: "Equity",
              value: formatPrice(account.equity),
              tone: "positive",
            },
            {
              label: "Realized P/L",
              value: formatPrice(account.realizedProfitLoss),
              tone: account.realizedProfitLoss >= 0 ? "positive" : "negative",
            },
            {
              label: "Unrealized P/L",
              value: formatPrice(account.unrealizedProfitLoss),
              tone: account.unrealizedProfitLoss >= 0 ? "positive" : "negative",
            },
          ]
        : [],
    [account],
  );

  const handleAccountCreate = async (values: {
    name: string;
    baseCurrency: string;
    startingBalance: number;
  }) => {
    await createAccount(values);
    accountForm.resetFields();
  };

  const submitTrade = async (direction: "Buy" | "Sell") => {
    setTradeDirection(direction);
    const values = await tradeForm.validateFields();

    await placeOrder({
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      direction,
      quantity: values.quantity,
      stopLoss: values.stopLoss ?? null,
      takeProfit: values.takeProfit ?? null,
      notes: values.notes ?? "",
    });

    tradeForm.resetFields(["quantity", "stopLoss", "takeProfit", "notes"]);
  };

  if (isLoading && !snapshot) {
    return <Skeleton active paragraph={{ rows: 8 }} />;
  }

  return (
    <div className={styles.wrapper}>
      {error ? (
        <Alert
          type="warning"
          showIcon
          title={error}
          closable
          onClose={clearError}
        />
      ) : null}

      {!account ? (
        <div className={styles.section}>
          <Typography.Paragraph className={styles.helper}>
            Create your internal paper account first. We will use live BTCUSDT
            prices, but all fills, positions, and P/L stay simulated.
          </Typography.Paragraph>

          <Form
            form={accountForm}
            layout="vertical"
            onFinish={(values) =>
              void handleAccountCreate(
                values as {
                  name: string;
                  baseCurrency: string;
                  startingBalance: number;
                },
              )
            }
          >
            <div className={styles.formGrid}>
              <Form.Item
                name="name"
                label="Account name"
                initialValue="Fintex Paper"
                rules={[{ required: true, message: "Give your paper account a name." }]}
              >
                <Input placeholder="Fintex Paper" />
              </Form.Item>

              <Form.Item
                name="baseCurrency"
                label="Base currency"
                initialValue="USD"
                rules={[{ required: true, message: "Set a base currency." }]}
              >
                <Input placeholder="USD" />
              </Form.Item>
            </div>

            <Form.Item
              name="startingBalance"
              label="Starting balance"
              initialValue={10000}
              rules={[{ required: true, message: "Set a starting balance." }]}
            >
              <InputNumber
                min={100}
                step={100}
                style={{ width: "100%" }}
                placeholder="10000"
              />
            </Form.Item>

            <Button type="primary" htmlType="submit" loading={isSubmitting}>
              Create paper account
            </Button>
          </Form>
        </div>
      ) : (
        <>
          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>{account.name}</span>
              <Tag color="green">{account.baseCurrency}</Tag>
            </div>

            <Typography.Paragraph className={styles.helper}>
              Latest Binance reference price: {currentPrice != null ? formatPrice(currentPrice) : "—"}.
              Account marked to market at {formatTime(account.lastMarkedToMarketAt)}.
            </Typography.Paragraph>

            <div className={styles.metrics}>
              {accountMetrics.map((metric) => (
                <div key={metric.label} className={styles.metricCard}>
                  <div className={styles.metricLabel}>{metric.label}</div>
                  <div
                    className={cx(
                      styles.metricValue,
                      metric.tone === "positive" ? styles.green : undefined,
                      metric.tone === "negative" ? styles.red : undefined,
                    )}
                  >
                    {metric.value}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>Place paper trade</span>
              <Tag color={tradeDirection === "Buy" ? "green" : "red"}>
                {tradeDirection}
              </Tag>
            </div>

            <Form form={tradeForm} layout="vertical">
              <div className={styles.formGrid}>
                <Form.Item
                  name="quantity"
                  label="Quantity"
                  rules={[{ required: true, message: "Enter a quantity." }]}
                >
                  <InputNumber
                    min={0.0001}
                    step={0.001}
                    style={{ width: "100%" }}
                    placeholder="0.010"
                  />
                </Form.Item>

                <Form.Item label="Symbol">
                  <Input value="BTCUSDT / Binance" readOnly />
                </Form.Item>
              </div>

              <div className={styles.formGrid}>
                <Form.Item name="stopLoss" label="Stop loss">
                  <InputNumber
                    min={0}
                    step={10}
                    style={{ width: "100%" }}
                    placeholder="Optional"
                  />
                </Form.Item>

                <Form.Item name="takeProfit" label="Take profit">
                  <InputNumber
                    min={0}
                    step={10}
                    style={{ width: "100%" }}
                    placeholder="Optional"
                  />
                </Form.Item>
              </div>

              <Form.Item name="notes" label="Notes">
                <Input placeholder="Why are you taking this setup?" />
              </Form.Item>

              <div className={styles.formActions}>
                <Button
                  type="primary"
                  loading={isSubmitting && tradeDirection === "Buy"}
                  onClick={() => void submitTrade("Buy")}
                >
                  Buy BTCUSDT
                </Button>
                <Button
                  danger
                  loading={isSubmitting && tradeDirection === "Sell"}
                  onClick={() => void submitTrade("Sell")}
                >
                  Sell BTCUSDT
                </Button>
              </div>
            </Form>
          </div>

          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>Open positions</span>
              <Tag color="blue">{positions.length}</Tag>
            </div>

            {positions.length === 0 ? (
              <div className={styles.empty}>
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No paper positions are open yet."
                />
              </div>
            ) : (
              <div className={styles.list}>
                {positions.map((position) => (
                  <div key={position.id} className={styles.item}>
                    <div className={styles.itemTop}>
                      <span className={styles.itemTitle}>
                        {position.symbol} {position.direction}
                      </span>
                      <Space>
                        <Tag color={position.direction === "Buy" ? "green" : "red"}>
                          {position.quantity.toFixed(4)}
                        </Tag>
                        <Button
                          size="small"
                          loading={isSubmitting}
                          onClick={() =>
                            void closePosition({
                              positionId: position.id,
                            })
                          }
                        >
                          Close
                        </Button>
                      </Space>
                    </div>
                    <div className={styles.itemMeta}>
                      <span>Entry {formatPrice(position.averageEntryPrice)}</span>
                      <span>Mark {formatPrice(position.currentMarketPrice)}</span>
                      <span
                        className={cx(
                          position.unrealizedProfitLoss >= 0
                            ? styles.green
                            : styles.red,
                        )}
                      >
                        U/P&L {formatPrice(position.unrealizedProfitLoss)}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>Recent orders</span>
              <Tag color="default">{orders.length}</Tag>
            </div>

            {orders.length === 0 ? (
              <div className={styles.empty}>
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No paper orders yet."
                />
              </div>
            ) : (
              <div className={styles.list}>
                {orders.slice(0, 5).map((order) => (
                  <div key={order.id} className={styles.item}>
                    <div className={styles.itemTop}>
                      <span className={styles.itemTitle}>
                        {order.symbol} {order.direction}
                      </span>
                      <Tag color={order.status === "Filled" ? "green" : "blue"}>
                        {order.status}
                      </Tag>
                    </div>
                    <div className={styles.itemMeta}>
                      <span>Qty {order.quantity.toFixed(4)}</span>
                      <span>Fill {formatPrice(order.executedPrice)}</span>
                      <span>{formatTime(order.executedAt ?? order.submittedAt)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>Recent fills</span>
              <Tag color="purple">{fills.length}</Tag>
            </div>

            {fills.length === 0 ? (
              <div className={styles.empty}>
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No paper fills yet."
                />
              </div>
            ) : (
              <div className={styles.list}>
                {fills.slice(0, 5).map((fill) => (
                  <div key={fill.id} className={styles.item}>
                    <div className={styles.itemTop}>
                      <span className={styles.itemTitle}>
                        {fill.symbol} {fill.direction}
                      </span>
                      <Tag color={fill.realizedProfitLoss >= 0 ? "green" : "red"}>
                        {formatPrice(fill.realizedProfitLoss)}
                      </Tag>
                    </div>
                    <div className={styles.itemMeta}>
                      <span>Qty {fill.quantity.toFixed(4)}</span>
                      <span>Price {formatPrice(fill.price)}</span>
                      <span>{formatTime(fill.executedAt)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
