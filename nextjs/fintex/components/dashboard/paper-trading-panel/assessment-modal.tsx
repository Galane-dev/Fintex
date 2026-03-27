"use client";

import { Alert, Button, Modal, Tag } from "antd";
import { formatPrice } from "@/utils/market-data";
import { getRiskTone } from "./helpers";
import type { PaperTradingPanelController } from "./types";
import { usePaperTradingStyles } from "../paper-trading-style";

interface AssessmentModalProps {
  controller: PaperTradingPanelController;
}

export const AssessmentModal = ({ controller }: AssessmentModalProps) => {
  const { styles } = usePaperTradingStyles();
  const { activeFeedback, feedbackTone } = controller;

  return (
    <Modal
      open={controller.isAssessmentOpen && activeFeedback != null}
      onCancel={controller.closeAssessmentModal}
      title="Trade feedback"
      width={680}
      footer={[
        <Button key="close" className={styles.actionButton} onClick={controller.closeAssessmentModal}>
          Close
        </Button>,
        <Button
          key="apply"
          type="primary"
          className={styles.actionButton}
          loading={controller.isBusy}
          onClick={() => void controller.handleApplyAssessmentSuggestions()}
        >
          Apply suggested setup
        </Button>,
      ]}
    >
      {activeFeedback && feedbackTone ? (
        <div className={styles.feedbackBody}>
          <Alert type={feedbackTone} showIcon title={activeFeedback.headline} description={activeFeedback.summary} />
          <div className={styles.feedbackMeta}>
            <Tag color={getRiskTone(activeFeedback.riskLevel)}>{activeFeedback.riskLevel} risk</Tag>
            <Tag color="blue">Score {activeFeedback.riskScore.toFixed(1)}</Tag>
            <Tag color="purple">Market {activeFeedback.marketVerdict}</Tag>
            <Tag color="default">Ref {formatPrice(activeFeedback.referencePrice)}</Tag>
            {activeFeedback.spread != null ? <Tag color="default">Spread {formatPrice(activeFeedback.spread)}</Tag> : null}
          </div>
          <div className={styles.feedbackBlock}>
            <span className={styles.feedbackLabel}>Why this read was given</span>
            <ul className={styles.feedbackList}>{activeFeedback.reasons.map((item) => <li key={item}>{item}</li>)}</ul>
          </div>
          <div className={styles.feedbackBlock}>
            <span className={styles.feedbackLabel}>How to improve the setup</span>
            <ul className={styles.feedbackList}>{activeFeedback.suggestions.map((item) => <li key={item}>{item}</li>)}</ul>
          </div>
          <div className={styles.summaryGrid}>
            <div className={styles.summaryCard}><span className={styles.summaryLabel}>Direction</span><span className={styles.summaryValue}>{activeFeedback.direction}</span></div>
            <div className={styles.summaryCard}><span className={styles.summaryLabel}>Suggested stop loss</span><span className={styles.summaryValue}>{formatPrice(activeFeedback.suggestedStopLoss)}</span></div>
            <div className={styles.summaryCard}><span className={styles.summaryLabel}>Suggested take profit</span><span className={styles.summaryValue}>{formatPrice(activeFeedback.suggestedTakeProfit)}</span></div>
            <div className={styles.summaryCard}><span className={styles.summaryLabel}>Reward to risk</span><span className={styles.summaryValue}>{activeFeedback.suggestedRewardRiskRatio != null ? activeFeedback.suggestedRewardRiskRatio.toFixed(2) : "-"}</span></div>
          </div>
        </div>
      ) : null}
    </Modal>
  );
};
