"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Alert,
  Button,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Skeleton,
  Space,
  Tabs,
  Tag,
  Typography,
  message,
} from "antd";
import { useExternalBrokerAccounts } from "@/hooks/useExternalBrokerAccounts";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import {
  EXTERNAL_BROKER_PLATFORM_DIRECT_API,
  EXTERNAL_BROKER_PROVIDER_ALPACA,
} from "@/types/external-broker";
import {
  getExternalBrokerEnvironmentLabel,
  getExternalBrokerStatusTone,
  maskExternalBrokerKey,
} from "@/utils/external-broker";
import { formatPrice, formatTime } from "@/utils/market-data";
import { usePaperTradingStyles } from "./paper-trading-style";

interface PaperTradingPanelProps {
  currentPrice: number | null;
  registerDashboardActions?: (actions: DashboardPaperTradingActions) => void;
  displayMode?: "full" | "support";
}

export interface DashboardPaperTradingActions {
  hasAccount: boolean;
  openAccounts: () => void;
  openRecommendation: () => void;
  openTrade: (direction: "Buy" | "Sell") => void;
}

export function PaperTradingPanel({
  currentPrice,
  registerDashboardActions,
  displayMode = "full",
}: PaperTradingPanelProps) {
  const { styles, cx } = usePaperTradingStyles();
  const {
    snapshot,
    isLoading,
    isSubmitting,
    error,
    latestAssessment,
    recommendation,
    createAccount,
    placeOrder,
    getRecommendation,
    closePosition,
    clearError,
    clearFeedback,
  } = usePaperTrading();
  const {
    connections,
    isLoading: isExternalLoading,
    isSubmitting: isExternalSubmitting,
    error: externalBrokerError,
    connectAccount,
    disconnectAccount,
    clearError: clearExternalBrokerError,
  } = useExternalBrokerAccounts();
  const [accountForm] = Form.useForm();
  const [tradeForm] = Form.useForm();
  const [externalBrokerForm] = Form.useForm();
  const [tradeDirection, setTradeDirection] = useState<"Buy" | "Sell">("Buy");
  const [isAccountsOpen, setIsAccountsOpen] = useState(false);
  const [isTradeOpen, setIsTradeOpen] = useState(false);
  const [isRecommendationOpen, setIsRecommendationOpen] = useState(false);
  const [isAssessmentOpen, setIsAssessmentOpen] = useState(false);

  const account = snapshot?.account ?? null;
  const positions = snapshot?.positions ?? [];
  const orders = snapshot?.recentOrders ?? [];
  const fills = snapshot?.recentFills ?? [];
  const activeFeedback = latestAssessment;
  const watchedQuantity = Form.useWatch("quantity", tradeForm);
  const effectiveQuantity = watchedQuantity ?? 0.01;
  const sortedConnections = useMemo(
    () =>
      [...connections].sort((left, right) => {
        if (left.isActive === right.isActive) {
          return right.id - left.id;
        }

        return left.isActive ? -1 : 1;
      }),
    [connections],
  );

  const feedbackTone = useMemo(() => {
    if (!activeFeedback) {
      return null;
    }

    if (activeFeedback.riskLevel === "High") {
      return "error" as const;
    }

    if (activeFeedback.riskLevel === "Medium") {
      return "warning" as const;
    }

    return "success" as const;
  }, [activeFeedback]);

  const recommendationTone = useMemo(() => {
    if (!recommendation) {
      return null;
    }

    if (recommendation.riskLevel === "High") {
      return "error" as const;
    }

    if (recommendation.riskLevel === "Medium") {
      return "warning" as const;
    }

    return "success" as const;
  }, [recommendation]);

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

  const requestRecommendation = useCallback(async () => {
    const values = tradeForm.getFieldsValue();

    const result = await getRecommendation({
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      quantity: values.quantity ?? null,
      stopLoss: values.stopLoss ?? null,
      takeProfit: values.takeProfit ?? null,
    });

    if (!result) {
      return;
    }

    tradeForm.setFieldsValue({
      stopLoss: values.stopLoss ?? result.suggestedStopLoss ?? undefined,
      takeProfit: values.takeProfit ?? result.suggestedTakeProfit ?? undefined,
    });
    setTradeDirection(result.recommendedAction === "Sell" ? "Sell" : "Buy");
    setIsRecommendationOpen(true);
  }, [getRecommendation, tradeForm]);

  const openAccountsModal = useCallback(() => {
    setIsAccountsOpen(true);
  }, []);

  const openTradeModal = useCallback(
    (direction: "Buy" | "Sell") => {
      setTradeDirection(direction);

      if (!account) {
        setIsAccountsOpen(true);
        message.info("Create your paper account before placing simulated trades.");
        return;
      }

      setIsTradeOpen(true);
    },
    [account],
  );

  const openRecommendationModal = useCallback(async () => {
    await requestRecommendation();
  }, [requestRecommendation]);

  useEffect(() => {
    if (!registerDashboardActions) {
      return;
    }

    registerDashboardActions({
      hasAccount: account != null,
      openAccounts: openAccountsModal,
      openRecommendation: () => {
        void openRecommendationModal();
      },
      openTrade: openTradeModal,
    });
  }, [
    account,
    openAccountsModal,
    openRecommendationModal,
    openTradeModal,
    registerDashboardActions,
  ]);

  const handleAccountCreate = async (values: {
    name: string;
    baseCurrency: string;
    startingBalance: number;
  }) => {
    await createAccount(values);
    accountForm.resetFields();
    setIsAccountsOpen(false);
    message.success("Paper account created.");
  };

  const handleConnectExternalBroker = async (values: {
    displayName: string;
    apiKey: string;
    apiSecret: string;
    environment: "paper" | "live";
  }) => {
    const connection = await connectAccount({
      displayName: values.displayName,
      provider: EXTERNAL_BROKER_PROVIDER_ALPACA,
      platform: EXTERNAL_BROKER_PLATFORM_DIRECT_API,
      apiKey: values.apiKey,
      apiSecret: values.apiSecret,
      isPaperEnvironment: values.environment !== "live",
    });

    if (!connection) {
      return;
    }

    externalBrokerForm.resetFields();
    message.success("Alpaca account connected.");
  };

  const handleDisconnectExternalBroker = async (id: number) => {
    await disconnectAccount({ id });
    message.success("External broker disconnected.");
  };

  const submitTrade = async (direction: "Buy" | "Sell") => {
    setTradeDirection(direction);
    const values = await tradeForm.validateFields();

    const result = await placeOrder({
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      direction,
      quantity: values.quantity,
      stopLoss: values.stopLoss ?? null,
      takeProfit: values.takeProfit ?? null,
      notes: values.notes ?? "",
    });

    if (!result) {
      return;
    }

    if (result.wasExecuted) {
      tradeForm.resetFields(["quantity", "stopLoss", "takeProfit", "notes"]);
      setIsTradeOpen(false);
      setIsAssessmentOpen(false);
      message.success(`${direction} paper trade placed.`);
      return;
    }

    setIsAssessmentOpen(true);
  };

  const handleApplyAssessmentSuggestions = async () => {
    if (!activeFeedback) {
      return;
    }

    const currentValues = tradeForm.getFieldsValue();
    const quantity = currentValues.quantity ?? 0.01;
    const direction = activeFeedback.direction;

    tradeForm.setFieldsValue({
      quantity,
      stopLoss: activeFeedback.suggestedStopLoss ?? currentValues.stopLoss ?? undefined,
      takeProfit: activeFeedback.suggestedTakeProfit ?? currentValues.takeProfit ?? undefined,
    });

    const result = await placeOrder({
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      direction,
      quantity,
      stopLoss: activeFeedback.suggestedStopLoss ?? currentValues.stopLoss ?? null,
      takeProfit: activeFeedback.suggestedTakeProfit ?? currentValues.takeProfit ?? null,
      notes: currentValues.notes ?? `Assessment-guided ${direction} setup`,
    });

    if (!result) {
      return;
    }

    if (result.wasExecuted) {
      setIsAssessmentOpen(false);
      setIsTradeOpen(false);
      tradeForm.resetFields(["quantity", "stopLoss", "takeProfit", "notes"]);
      message.success("Suggested trade plan placed.");
      return;
    }

    setIsAssessmentOpen(true);
  };

  const handlePlaceSuggestedTrade = async () => {
    if (!recommendation) {
      return;
    }

    if (recommendation.recommendedAction === "Hold") {
      message.info("This recommendation is a hold, so there is no trade to place yet.");
      return;
    }

    if (!account) {
      setIsRecommendationOpen(false);
      setIsAccountsOpen(true);
      message.info("Create your paper account before placing the suggested trade.");
      return;
    }

    const values = tradeForm.getFieldsValue();
    const quantity = values.quantity ?? 0.01;
    const direction = recommendation.recommendedAction as "Buy" | "Sell";
    setTradeDirection(direction);

    tradeForm.setFieldsValue({
      quantity,
      stopLoss: recommendation.suggestedStopLoss ?? values.stopLoss ?? undefined,
      takeProfit: recommendation.suggestedTakeProfit ?? values.takeProfit ?? undefined,
    });

    const result = await placeOrder({
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      direction,
      quantity,
      stopLoss: recommendation.suggestedStopLoss ?? values.stopLoss ?? null,
      takeProfit: recommendation.suggestedTakeProfit ?? values.takeProfit ?? null,
      notes: values.notes ?? `Suggested ${direction} setup`,
    });

    if (!result) {
      return;
    }

    if (result.wasExecuted) {
      setIsRecommendationOpen(false);
      setIsTradeOpen(false);
      tradeForm.resetFields(["quantity", "stopLoss", "takeProfit", "notes"]);
      message.success("Suggested trade placed.");
      return;
    }

    message.warning(result.assessment.headline);
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

      {displayMode === "support" ? null : activeFeedback && feedbackTone ? (
        <Alert
          type={feedbackTone}
          showIcon
          closable
          onClose={clearFeedback}
          title={activeFeedback.headline}
          description={
            <div className={styles.feedbackBody}>
              <Typography.Paragraph className={styles.helper}>
                {activeFeedback.summary}
              </Typography.Paragraph>

              <div className={styles.feedbackMeta}>
                <Tag
                  color={
                    activeFeedback.riskLevel === "High"
                      ? "red"
                      : activeFeedback.riskLevel === "Medium"
                        ? "gold"
                        : "green"
                  }
                >
                  {activeFeedback.riskLevel} risk
                </Tag>
                <Tag color="blue">Score {activeFeedback.riskScore.toFixed(1)}</Tag>
                <Tag color="purple">Market {activeFeedback.marketVerdict}</Tag>
                <Tag color="default">
                  Ref {formatPrice(activeFeedback.referencePrice)}
                </Tag>
                {activeFeedback.spread != null ? (
                  <Tag color="default">
                    Spread {formatPrice(activeFeedback.spread)}
                  </Tag>
                ) : null}
              </div>

              <div className={styles.feedbackBlock}>
                <span className={styles.feedbackLabel}>Why this read was given</span>
                <ul className={styles.feedbackList}>
                  {activeFeedback.reasons.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
              </div>

              <div className={styles.feedbackBlock}>
                <span className={styles.feedbackLabel}>How to improve the setup</span>
                <ul className={styles.feedbackList}>
                  {activeFeedback.suggestions.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
              </div>

              <div className={styles.feedbackMeta}>
                {activeFeedback.suggestedStopLoss != null ? (
                  <Tag color="red">
                    Suggested SL {formatPrice(activeFeedback.suggestedStopLoss)}
                  </Tag>
                ) : null}
                {activeFeedback.suggestedTakeProfit != null ? (
                  <Tag color="green">
                    Suggested TP {formatPrice(activeFeedback.suggestedTakeProfit)}
                  </Tag>
                ) : null}
                {"suggestedRewardRiskRatio" in activeFeedback &&
                activeFeedback.suggestedRewardRiskRatio != null ? (
                  <Tag color="gold">
                    R:R {activeFeedback.suggestedRewardRiskRatio.toFixed(2)}
                  </Tag>
                ) : null}
              </div>
            </div>
          }
        />
      ) : null}

      <Modal
        open={isAssessmentOpen && activeFeedback != null}
        onCancel={() => setIsAssessmentOpen(false)}
        title="Trade feedback"
        width={680}
        footer={[
          <Button
            key="close"
            className={styles.actionButton}
            onClick={() => setIsAssessmentOpen(false)}
          >
            Close
          </Button>,
          <Button
            key="apply"
            type="primary"
            className={styles.actionButton}
            loading={isSubmitting}
            onClick={() => void handleApplyAssessmentSuggestions()}
          >
            Apply suggested setup
          </Button>,
        ]}
      >
        {activeFeedback && feedbackTone ? (
          <div className={styles.feedbackBody}>
            <Alert
              type={feedbackTone}
              showIcon
              title={activeFeedback.headline}
              description={activeFeedback.summary}
            />

            <div className={styles.feedbackMeta}>
              <Tag
                color={
                  activeFeedback.riskLevel === "High"
                    ? "red"
                    : activeFeedback.riskLevel === "Medium"
                      ? "gold"
                      : "green"
                }
              >
                {activeFeedback.riskLevel} risk
              </Tag>
              <Tag color="blue">Score {activeFeedback.riskScore.toFixed(1)}</Tag>
              <Tag color="purple">Market {activeFeedback.marketVerdict}</Tag>
              <Tag color="default">Ref {formatPrice(activeFeedback.referencePrice)}</Tag>
              {activeFeedback.spread != null ? (
                <Tag color="default">Spread {formatPrice(activeFeedback.spread)}</Tag>
              ) : null}
            </div>

            <div className={styles.feedbackBlock}>
              <span className={styles.feedbackLabel}>Why this read was given</span>
              <ul className={styles.feedbackList}>
                {activeFeedback.reasons.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </div>

            <div className={styles.feedbackBlock}>
              <span className={styles.feedbackLabel}>How to improve the setup</span>
              <ul className={styles.feedbackList}>
                {activeFeedback.suggestions.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </div>

            <div className={styles.summaryGrid}>
              <div className={styles.summaryCard}>
                <span className={styles.summaryLabel}>Direction</span>
                <span className={styles.summaryValue}>{activeFeedback.direction}</span>
              </div>
              <div className={styles.summaryCard}>
                <span className={styles.summaryLabel}>Suggested stop loss</span>
                <span className={styles.summaryValue}>
                  {formatPrice(activeFeedback.suggestedStopLoss)}
                </span>
              </div>
              <div className={styles.summaryCard}>
                <span className={styles.summaryLabel}>Suggested take profit</span>
                <span className={styles.summaryValue}>
                  {formatPrice(activeFeedback.suggestedTakeProfit)}
                </span>
              </div>
              <div className={styles.summaryCard}>
                <span className={styles.summaryLabel}>Reward to risk</span>
                <span className={styles.summaryValue}>
                  {activeFeedback.suggestedRewardRiskRatio != null
                    ? activeFeedback.suggestedRewardRiskRatio.toFixed(2)
                    : "-"}
                </span>
              </div>
            </div>
          </div>
        ) : null}
      </Modal>

      <Modal
        open={isAccountsOpen}
        onCancel={() => setIsAccountsOpen(false)}
        title="Accounts"
        width={760}
        footer={null}
      >
        <Tabs
          className={styles.accountTabs}
          items={[
            {
              key: "paper-academy",
              label: "Paper academy",
              children: !account ? (
                <div className={styles.section}>
                  <Typography.Paragraph className={styles.helper}>
                    Create your internal paper account first. This simulator stays in place as the trading academy while live broker connectivity grows alongside it.
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
                        initialValue="Fintex Academy"
                        rules={[{ required: true, message: "Give your academy account a name." }]}
                      >
                        <Input placeholder="Fintex Academy" />
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
                        className={styles.fullWidthInput}
                        placeholder="10000"
                      />
                    </Form.Item>

                    <Button
                      type="primary"
                      htmlType="submit"
                      loading={isSubmitting}
                      className={styles.actionButton}
                    >
                      Create paper academy account
                    </Button>
                  </Form>
                </div>
              ) : (
                <div className={styles.section}>
                  <div className={styles.sectionHeader}>
                    <span className={styles.sectionTitle}>{account.name}</span>
                    <Tag color="green">{account.baseCurrency}</Tag>
                  </div>

                  <Typography.Paragraph className={styles.helper}>
                    Your internal academy account remains available for safe practice, journaling, and future trading-school progression even when real broker links are added.
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

                  <div className={styles.summaryGrid}>
                    <div className={styles.summaryCard}>
                      <span className={styles.summaryLabel}>Reference price</span>
                      <span className={styles.summaryValue}>
                        {currentPrice != null ? formatPrice(currentPrice) : "-"}
                      </span>
                    </div>
                    <div className={styles.summaryCard}>
                      <span className={styles.summaryLabel}>Marked to market</span>
                      <span className={styles.summaryValue}>
                        {formatTime(account.lastMarkedToMarketAt)}
                      </span>
                    </div>
                  </div>
                </div>
              ),
            },
            {
              key: "external-broker",
              label: "External broker",
              children: (
                <div className={styles.section}>
                  {externalBrokerError ? (
                    <Alert
                      type="warning"
                      showIcon
                      title={externalBrokerError}
                      closable
                      onClose={clearExternalBrokerError}
                    />
                  ) : null}

                  <div className={styles.brokerHero}>
                    <div>
                      <div className={styles.sectionTitle}>Connect Alpaca through the Trading API</div>
                      <Typography.Paragraph className={styles.helper}>
                        Fintex validates your Alpaca API key and secret against Alpaca&apos;s account endpoint, then keeps that live or paper account alongside your in-app academy account.
                      </Typography.Paragraph>
                    </div>

                    <Space wrap>
                      <Tag color="green">Alpaca</Tag>
                      <Tag color="blue">Trading API</Tag>
                    </Space>
                  </div>

                  <Form
                    form={externalBrokerForm}
                    layout="vertical"
                    onFinish={(values) =>
                      void handleConnectExternalBroker(
                        values as {
                          displayName: string;
                          apiKey: string;
                          apiSecret: string;
                          environment: "paper" | "live";
                        },
                      )
                    }
                  >
                    <div className={styles.formGrid}>
                      <Form.Item
                        name="displayName"
                        label="Display name"
                        initialValue="Alpaca Paper"
                        rules={[{ required: true, message: "Name this broker connection." }]}
                      >
                        <Input placeholder="Alpaca Paper" />
                      </Form.Item>

                      <Form.Item
                        name="apiKey"
                        label="API key ID"
                        rules={[{ required: true, message: "Enter your Alpaca API key." }]}
                      >
                        <Input placeholder="PK..." />
                      </Form.Item>
                    </div>

                    <div className={styles.formGrid}>
                      <Form.Item
                        name="apiSecret"
                        label="API secret key"
                        rules={[{ required: true, message: "Enter your Alpaca API secret." }]}
                      >
                        <Input.Password placeholder="Alpaca API secret" />
                      </Form.Item>

                      <Form.Item
                        name="environment"
                        label="Environment"
                        initialValue="paper"
                        rules={[{ required: true, message: "Choose the Alpaca environment." }]}
                      >
                        <Select
                          options={[
                            { label: "Paper", value: "paper" },
                            { label: "Live", value: "live" },
                          ]}
                        />
                      </Form.Item>
                    </div>

                    <Button
                      type="primary"
                      htmlType="submit"
                      loading={isExternalSubmitting}
                      className={styles.actionButton}
                    >
                      Connect Alpaca
                    </Button>
                  </Form>

                  <div className={styles.section}>
                    <div className={styles.sectionHeader}>
                      <span className={styles.sectionTitle}>Connected broker accounts</span>
                      <Tag color="default">{sortedConnections.length}</Tag>
                    </div>

                    {isExternalLoading && sortedConnections.length === 0 ? (
                      <Skeleton active paragraph={{ rows: 4 }} />
                    ) : sortedConnections.length === 0 ? (
                      <div className={styles.empty}>
                        <Empty
                          image={Empty.PRESENTED_IMAGE_SIMPLE}
                          description="No external broker has been linked yet."
                        />
                      </div>
                    ) : (
                      <div className={styles.list}>
                        {sortedConnections.map((connection) => (
                            <div key={connection.id} className={styles.item}>
                              <div className={styles.itemTop}>
                                <div>
                                  <div className={styles.itemTitle}>{connection.displayName}</div>
                                  <div className={styles.itemMeta}>
                                    <span>Key {maskExternalBrokerKey(connection.accountLogin)}</span>
                                    <span>{getExternalBrokerEnvironmentLabel(connection)}</span>
                                    {connection.brokerCompany ? (
                                      <span>{connection.brokerCompany}</span>
                                    ) : null}
                                  </div>
                                </div>

                              <Space wrap>
                                <Tag color={getExternalBrokerStatusTone(connection.status)}>
                                  {connection.status}
                                </Tag>
                                {connection.isActive ? (
                                  <Button
                                    loading={isExternalSubmitting}
                                    className={styles.actionButton}
                                    onClick={() =>
                                      void handleDisconnectExternalBroker(connection.id)
                                    }
                                  >
                                    Disconnect
                                  </Button>
                                ) : null}
                              </Space>
                            </div>

                            <div className={styles.summaryGrid}>
                              <div className={styles.summaryCard}>
                                <span className={styles.summaryLabel}>Account name</span>
                                <span className={styles.summaryValue}>
                                  {connection.brokerAccountName || "-"}
                                </span>
                              </div>
                              <div className={styles.summaryCard}>
                                <span className={styles.summaryLabel}>Currency / leverage</span>
                                <span className={styles.summaryValue}>
                                  {connection.brokerAccountCurrency || "-"}
                                  {connection.brokerLeverage != null
                                    ? ` / 1:${connection.brokerLeverage}`
                                    : ""}
                                </span>
                              </div>
                              <div className={styles.summaryCard}>
                                <span className={styles.summaryLabel}>Balance / equity</span>
                                <span className={styles.summaryValue}>
                                  {connection.lastKnownBalance != null
                                    ? formatPrice(connection.lastKnownBalance)
                                    : "-"}
                                  {connection.lastKnownEquity != null
                                    ? ` / ${formatPrice(connection.lastKnownEquity)}`
                                    : ""}
                                </span>
                              </div>
                              <div className={styles.summaryCard}>
                                <span className={styles.summaryLabel}>Last validated</span>
                                <span className={styles.summaryValue}>
                                  {connection.lastValidatedAt
                                    ? formatTime(connection.lastValidatedAt)
                                    : "-"}
                                </span>
                              </div>
                            </div>

                            {connection.lastError ? (
                              <Alert type="warning" showIcon title={connection.lastError} />
                            ) : null}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              ),
            },
          ]}
        />
      </Modal>

      <Modal
        open={isTradeOpen}
        onCancel={() => setIsTradeOpen(false)}
        title={`${tradeDirection} BTCUSDT`}
        width={680}
        footer={[
          <Button
            key="cancel"
            className={styles.actionButton}
            onClick={() => setIsTradeOpen(false)}
          >
            Cancel
          </Button>,
          <Button
            key="submit"
            type="primary"
            danger={tradeDirection === "Sell"}
            loading={isSubmitting}
            className={styles.actionButton}
            onClick={() => void submitTrade(tradeDirection)}
          >
            Confirm {tradeDirection}
          </Button>,
        ]}
      >
        <div className={styles.section}>
          <Typography.Paragraph className={styles.helper}>
            Shape the trade first, then submit. If the setup is too risky, Fintex will stop the trade or warn you before it goes through.
          </Typography.Paragraph>

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
                  className={styles.fullWidthInput}
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
                  className={styles.fullWidthInput}
                  placeholder="Optional"
                />
              </Form.Item>

              <Form.Item name="takeProfit" label="Take profit">
                <InputNumber
                  min={0}
                  step={10}
                  className={styles.fullWidthInput}
                  placeholder="Optional"
                />
              </Form.Item>
            </div>

            <Form.Item name="notes" label="Notes">
              <Input placeholder="Why are you taking this setup?" />
            </Form.Item>
          </Form>
        </div>
      </Modal>

      <Modal
        open={isRecommendationOpen && recommendation != null}
        onCancel={() => setIsRecommendationOpen(false)}
        title="Trade recommendation"
        width={680}
        footer={[
          <Button
            key="close"
            className={styles.actionButton}
            onClick={() => setIsRecommendationOpen(false)}
          >
            Close
          </Button>,
          <Button
            key="place"
            type="primary"
            danger={recommendation?.riskLevel === "High"}
            disabled={recommendation?.recommendedAction === "Hold"}
            loading={isSubmitting}
            className={styles.actionButton}
            onClick={() => void handlePlaceSuggestedTrade()}
          >
            Place suggested trade
          </Button>,
        ]}
      >
        {recommendation && recommendationTone ? (
          <div className={styles.feedbackBody}>
            <Alert
              type={recommendationTone}
              showIcon
              title={recommendation.headline}
              description={recommendation.summary}
            />

            <div className={styles.feedbackMeta}>
              <Tag
                color={
                  recommendation.riskLevel === "High"
                    ? "red"
                    : recommendation.riskLevel === "Medium"
                      ? "gold"
                      : "green"
                }
              >
                {recommendation.riskLevel} risk
              </Tag>
              <Tag
                color={
                  recommendation.recommendedAction === "Sell"
                    ? "red"
                    : recommendation.recommendedAction === "Buy"
                      ? "green"
                      : "default"
                }
              >
                {recommendation.recommendedAction}
              </Tag>
              <Tag color="blue">Score {recommendation.riskScore.toFixed(1)}</Tag>
              <Tag color="default">Ref {formatPrice(recommendation.referencePrice)}</Tag>
              {recommendation.spread != null ? (
                <Tag color="purple">Spread {formatPrice(recommendation.spread)}</Tag>
              ) : null}
            </div>

            <div className={styles.feedbackBlock}>
              <span className={styles.feedbackLabel}>Suggested trade</span>
              {recommendation.recommendedAction === "Hold" ? (
                <Typography.Paragraph className={styles.helper}>
                  Stand aside for now. The model does not see a clean buy or sell setup yet.
                </Typography.Paragraph>
              ) : (
                <div className={styles.summaryGrid}>
                  <div className={styles.summaryCard}>
                    <span className={styles.summaryLabel}>Action</span>
                    <span className={styles.summaryValue}>
                      {recommendation.recommendedAction} BTCUSDT
                    </span>
                  </div>
                  <div className={styles.summaryCard}>
                    <span className={styles.summaryLabel}>Quantity</span>
                    <span className={styles.summaryValue}>{effectiveQuantity.toFixed(4)}</span>
                  </div>
                  <div className={styles.summaryCard}>
                    <span className={styles.summaryLabel}>Entry reference</span>
                    <span className={styles.summaryValue}>
                      {formatPrice(recommendation.referencePrice)}
                    </span>
                  </div>
                  <div className={styles.summaryCard}>
                    <span className={styles.summaryLabel}>Spread</span>
                    <span className={styles.summaryValue}>
                      {recommendation.spread != null ? formatPrice(recommendation.spread) : "-"}
                    </span>
                  </div>
                  <div className={styles.summaryCard}>
                    <span className={styles.summaryLabel}>Suggested stop loss</span>
                    <span className={styles.summaryValue}>
                      {formatPrice(recommendation.suggestedStopLoss)}
                    </span>
                  </div>
                  <div className={styles.summaryCard}>
                    <span className={styles.summaryLabel}>Suggested take profit</span>
                    <span className={styles.summaryValue}>
                      {formatPrice(recommendation.suggestedTakeProfit)}
                    </span>
                  </div>
                </div>
              )}
            </div>

            <div className={styles.feedbackBlock}>
              <span className={styles.feedbackLabel}>Why now</span>
              <ul className={styles.feedbackList}>
                {recommendation.reasons.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </div>

            <div className={styles.feedbackBlock}>
              <span className={styles.feedbackLabel}>Suggested improvements</span>
              <ul className={styles.feedbackList}>
                {recommendation.suggestions.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </div>

            <div className={styles.feedbackMeta}>
              {recommendation.suggestedStopLoss != null ? (
                <Tag color="red">
                  Suggested SL {formatPrice(recommendation.suggestedStopLoss)}
                </Tag>
              ) : null}
              {recommendation.suggestedTakeProfit != null ? (
                <Tag color="green">
                  Suggested TP {formatPrice(recommendation.suggestedTakeProfit)}
                </Tag>
              ) : null}
              {recommendation.confidenceScore != null ? (
                <Tag color="blue">
                  Confidence {recommendation.confidenceScore.toFixed(1)}
                </Tag>
              ) : null}
            </div>
          </div>
        ) : null}
      </Modal>

      {displayMode === "support" ? null : !account ? (
        <div className={styles.section}>
          <Typography.Paragraph className={styles.helper}>
            Create your internal paper account to unlock simulator trading. Buy, sell, recommendation, and account actions now live in the chart header so the workspace stays focused.
          </Typography.Paragraph>
          <Button
            type="primary"
            className={styles.actionButton}
            onClick={openAccountsModal}
          >
            Open accounts
          </Button>
        </div>
      ) : (
        <>
          <div className={styles.section}>
            <div className={styles.sectionHeader}>
              <span className={styles.sectionTitle}>{account.name}</span>
              <Tag color="green">{account.baseCurrency}</Tag>
            </div>

            <Typography.Paragraph className={styles.helper}>
              Latest Binance reference price: {currentPrice != null ? formatPrice(currentPrice) : "-"}.
              {" "}Account marked to market at {formatTime(account.lastMarkedToMarketAt)}.
            </Typography.Paragraph>

            <div className={styles.inlineActions}>
              <Button className={styles.actionButton} onClick={openAccountsModal}>
                Manage account
              </Button>
              <Button
                className={styles.actionButton}
                onClick={() => {
                  void openRecommendationModal();
                }}
              >
                Get recommendation
              </Button>
            </div>

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
                          position.unrealizedProfitLoss >= 0 ? styles.green : styles.red,
                        )}
                      >
                        U/P&amp;L {formatPrice(position.unrealizedProfitLoss)}
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
