"use client";

import { Button, Form, Input, InputNumber, Select, Space, Typography } from "antd";
import { DashboardDrawerShell } from "@/components/dashboard/dashboard-drawer-shell";
import type { PaperTradingPanelController } from "./types";
import { usePaperTradingStyles } from "../paper-trading-style";

interface TradeModalProps {
  controller: PaperTradingPanelController;
}

export const TradeModal = ({ controller }: TradeModalProps) => {
  const { styles } = usePaperTradingStyles();

  return (
    <DashboardDrawerShell
      open={controller.isTradeOpen}
      onClose={controller.closeTradeModal}
      title={`${controller.tradeDirection} BTCUSDT`}
      width={720}
      footer={
        <Space>
          <Button className={styles.actionButton} onClick={controller.closeTradeModal}>
            Cancel
          </Button>
          <Button
            type="primary"
            danger={controller.tradeDirection === "Sell"}
            loading={controller.isBusy}
            className={styles.actionButton}
            onClick={() => void controller.submitTrade(controller.tradeDirection)}
          >
            Confirm {controller.tradeDirection}
          </Button>
        </Space>
      }
    >
      <div className={styles.section}>
        <Typography.Paragraph className={styles.helper}>
          Shape the trade first, then submit. If the setup is too risky, Fintex will stop the trade or warn you before it goes through.
        </Typography.Paragraph>

        <Form form={controller.tradeForm} layout="vertical">
          <div className={styles.formGrid}>
            <Form.Item
              name="executionTarget"
              label="Execution destination"
              initialValue={controller.availableExecutionTargets[0]?.value ?? "paper"}
              rules={[{ required: true, message: "Choose where this trade should be sent." }]}
            >
              <Select options={controller.availableExecutionTargets} placeholder="Select destination" />
            </Form.Item>

            <Form.Item name="quantity" label="Quantity" rules={[{ required: true, message: "Enter a quantity." }]}>
              <InputNumber min={0.0001} step={0.001} className={styles.fullWidthInput} placeholder="0.010" />
            </Form.Item>
          </div>

          <Form.Item label="Symbol">
            <Input value="BTCUSDT / Binance" readOnly />
          </Form.Item>

          <div className={styles.formGrid}>
            <Form.Item name="stopLoss" label="Stop loss">
              <InputNumber min={0} step={10} className={styles.fullWidthInput} placeholder="Optional" />
            </Form.Item>

            <Form.Item name="takeProfit" label="Take profit">
              <InputNumber min={0} step={10} className={styles.fullWidthInput} placeholder="Optional" />
            </Form.Item>
          </div>

          <Form.Item name="notes" label="Notes">
            <Input placeholder="Why are you taking this setup?" />
          </Form.Item>
        </Form>
      </div>
    </DashboardDrawerShell>
  );
};
