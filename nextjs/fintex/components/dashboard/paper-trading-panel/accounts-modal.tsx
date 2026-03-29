"use client";

import { Alert, Button, Empty, Form, Input, InputNumber, Modal, Select, Skeleton, Space, Tabs, Tag, Typography } from "antd";
import { getExternalBrokerEnvironmentLabel, maskExternalBrokerKey } from "@/utils/external-broker";
import { formatPrice, formatTime } from "@/utils/market-data";
import type { PaperTradingPanelController } from "./types";
import { usePaperTradingStyles } from "../paper-trading-style";

interface AccountsModalProps {
  controller: PaperTradingPanelController;
  currentPrice: number | null;
}

export const AccountsModal = ({ controller, currentPrice }: AccountsModalProps) => {
  const { styles, cx } = usePaperTradingStyles();
  const { account, accountMetrics, academyStatus, sortedConnections } = controller;

  const renderPaperAccount = () =>
    !account ? (
      <div className={styles.section}>
        <Typography.Paragraph className={styles.helper}>Create your internal paper account first. This simulator stays in place as the trading academy while live broker connectivity grows alongside it.</Typography.Paragraph>
        <Form form={controller.accountForm} layout="vertical" onFinish={(values) => void controller.handleAccountCreate(values)}>
          <div className={styles.formGrid}>
            <Form.Item name="name" label="Account name" initialValue="Fintex Academy" rules={[{ required: true, message: "Give your academy account a name." }]}><Input placeholder="Fintex Academy" /></Form.Item>
            <Form.Item name="baseCurrency" label="Base currency" initialValue="USD" rules={[{ required: true, message: "Set a base currency." }]}><Input placeholder="USD" /></Form.Item>
          </div>
          <Form.Item name="startingBalance" label="Starting balance" initialValue={10000} rules={[{ required: true, message: "Set a starting balance." }]}><InputNumber min={100} step={100} className={styles.fullWidthInput} placeholder="10000" /></Form.Item>
          <Button type="primary" htmlType="submit" loading={controller.isSubmitting} className={styles.actionButton}>Create paper academy account</Button>
        </Form>
      </div>
    ) : (
      <div className={styles.section}>
        <div className={styles.sectionHeader}><span className={styles.sectionTitle}>{account.name}</span><Tag color="green">{account.baseCurrency}</Tag></div>
        <Typography.Paragraph className={styles.helper}>Your internal academy account remains available for safe practice, journaling, and future trading-school progression even when real broker links are added.</Typography.Paragraph>
        <div className={styles.metrics}>{accountMetrics.map((metric) => <div key={metric.label} className={styles.metricCard}><div className={styles.metricLabel}>{metric.label}</div><div className={cx(styles.metricValue, metric.tone === "positive" ? styles.green : undefined, metric.tone === "negative" ? styles.red : undefined)}>{metric.value}</div></div>)}</div>
        <div className={styles.summaryGrid}><div className={styles.summaryCard}><span className={styles.summaryLabel}>Reference price</span><span className={styles.summaryValue}>{currentPrice != null ? formatPrice(currentPrice) : "-"}</span></div><div className={styles.summaryCard}><span className={styles.summaryLabel}>Marked to market</span><span className={styles.summaryValue}>{formatTime(account.lastMarkedToMarketAt)}</span></div></div>
      </div>
    );

  const renderExternalBroker = () =>
    !controller.canConnectExternalBrokers ? (
      <div className={styles.section}>
        <Alert
          type="info"
          showIcon
          message="External brokers are locked during trade academy"
          description={`Pass the intro academy quiz, then grow your paper account by ${academyStatus?.growthTargetPercent ?? 75}% to unlock Alpaca and future live brokers. Current growth: ${(academyStatus?.paperGrowthPercent ?? 0).toFixed(1)}%.`}
        />
      </div>
    ) : (
      <div className={styles.section}><div className={styles.brokerHero}><div><div className={styles.sectionTitle}>Connect Alpaca through the Trading API</div><Typography.Paragraph className={styles.helper}>Fintex validates your Alpaca API key and secret against Alpaca&apos;s account endpoint, then keeps that live or paper account alongside your in-app academy account.</Typography.Paragraph></div><Space wrap><Tag color="green">Alpaca</Tag><Tag color="blue">Trading API</Tag></Space></div><Form form={controller.externalBrokerForm} layout="vertical" onFinish={(values) => void controller.handleConnectExternalBroker(values)}><div className={styles.formGrid}><Form.Item name="displayName" label="Display name" initialValue="Alpaca Paper" rules={[{ required: true, message: "Name this broker connection." }]}><Input placeholder="Alpaca Paper" /></Form.Item><Form.Item name="apiKey" label="API key ID" rules={[{ required: true, message: "Enter your Alpaca API key." }]}><Input placeholder="PK..." /></Form.Item></div><div className={styles.formGrid}><Form.Item name="apiSecret" label="API secret key" rules={[{ required: true, message: "Enter your Alpaca API secret." }]}><Input.Password placeholder="Alpaca API secret" /></Form.Item><Form.Item name="environment" label="Environment" initialValue="paper" rules={[{ required: true, message: "Choose the Alpaca environment." }]}><Select options={[{ label: "Paper", value: "paper" }, { label: "Live", value: "live" }]} /></Form.Item></div><Button type="primary" htmlType="submit" loading={controller.isExternalSubmitting} className={styles.actionButton}>Connect Alpaca</Button></Form><div className={styles.section}><div className={styles.sectionHeader}><span className={styles.sectionTitle}>Connected broker accounts</span><Tag color="default">{sortedConnections.length}</Tag></div>{controller.isExternalLoading && sortedConnections.length === 0 ? <Skeleton active paragraph={{ rows: 4 }} /> : sortedConnections.length === 0 ? <div className={styles.empty}><Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No external broker has been linked yet." /></div> : <div className={styles.list}>{sortedConnections.map((connection) => <div key={connection.id} className={styles.item}><div className={styles.itemTop}><div><div className={styles.itemTitle}>{connection.displayName}</div><div className={styles.itemMeta}><span>Key {maskExternalBrokerKey(connection.accountLogin)}</span><span>{getExternalBrokerEnvironmentLabel(connection)}</span>{connection.brokerCompany ? <span>{connection.brokerCompany}</span> : null}</div></div><Space wrap><Tag color={controller.getConnectionStatusTone(connection.status)}>{connection.status}</Tag>{connection.isActive ? <Button loading={controller.isExternalSubmitting} className={styles.actionButton} onClick={() => void controller.handleDisconnectExternalBroker(connection.id)}>Disconnect</Button> : null}</Space></div><div className={styles.summaryGrid}><div className={styles.summaryCard}><span className={styles.summaryLabel}>Account name</span><span className={styles.summaryValue}>{connection.brokerAccountName || "-"}</span></div><div className={styles.summaryCard}><span className={styles.summaryLabel}>Currency / leverage</span><span className={styles.summaryValue}>{connection.brokerAccountCurrency || "-"}{connection.brokerLeverage != null ? ` / 1:${connection.brokerLeverage}` : ""}</span></div><div className={styles.summaryCard}><span className={styles.summaryLabel}>Balance / equity</span><span className={styles.summaryValue}>{connection.lastKnownBalance != null ? formatPrice(connection.lastKnownBalance) : "-"}{connection.lastKnownEquity != null ? ` / ${formatPrice(connection.lastKnownEquity)}` : ""}</span></div><div className={styles.summaryCard}><span className={styles.summaryLabel}>Last validated</span><span className={styles.summaryValue}>{connection.lastValidatedAt ? formatTime(connection.lastValidatedAt) : "-"}</span></div></div>{connection.lastError ? <Alert type="warning" showIcon title={connection.lastError} /> : null}</div>)}</div>}</div></div>
    );

  return (
    <Modal open={controller.isAccountsOpen} onCancel={controller.closeAccountsModal} title="Accounts" width={760} footer={null}>
      <Tabs className={styles.accountTabs} items={[{ key: "paper-academy", label: "Paper academy", children: renderPaperAccount() }, { key: "external-broker", label: "External broker", children: renderExternalBroker() }]} />
    </Modal>
  );
};
