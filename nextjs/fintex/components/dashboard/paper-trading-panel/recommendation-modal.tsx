"use client";

import { Alert, Button, Modal, Skeleton, Tag, Typography } from "antd";
import { formatPrice, formatTime } from "@/utils/market-data";
import { getRecommendationActionTone, getRiskTone } from "./helpers";
import type { PaperTradingPanelController } from "./types";
import { usePaperTradingStyles } from "../paper-trading-style";

interface RecommendationModalProps {
  controller: PaperTradingPanelController;
}

export const RecommendationModal = ({ controller }: RecommendationModalProps) => {
  const { styles } = usePaperTradingStyles();
  const recommendation = controller.recommendation;

  const renderSuggestedTrade = () => {
    if (!recommendation || recommendation.recommendedAction === "Hold") {
      return (
        <Typography.Paragraph className={styles.helper}>
          Stand aside for now. The model does not see a clean buy or sell setup yet.
        </Typography.Paragraph>
      );
    }

    return (
      <div className={styles.summaryGrid}>
        <div className={styles.summaryCard}><span className={styles.summaryLabel}>Action</span><span className={styles.summaryValue}>{recommendation.recommendedAction} BTCUSDT</span></div>
        <div className={styles.summaryCard}><span className={styles.summaryLabel}>Quantity</span><span className={styles.summaryValue}>{controller.effectiveQuantity.toFixed(4)}</span></div>
        <div className={styles.summaryCard}><span className={styles.summaryLabel}>Entry reference</span><span className={styles.summaryValue}>{formatPrice(recommendation.referencePrice)}</span></div>
        <div className={styles.summaryCard}><span className={styles.summaryLabel}>Spread</span><span className={styles.summaryValue}>{recommendation.spread != null ? formatPrice(recommendation.spread) : "-"}</span></div>
        <div className={styles.summaryCard}><span className={styles.summaryLabel}>Suggested stop loss</span><span className={styles.summaryValue}>{formatPrice(recommendation.suggestedStopLoss)}</span></div>
        <div className={styles.summaryCard}><span className={styles.summaryLabel}>Suggested take profit</span><span className={styles.summaryValue}>{formatPrice(recommendation.suggestedTakeProfit)}</span></div>
      </div>
    );
  };

  return (
    <Modal
      open={controller.isRecommendationOpen}
      onCancel={controller.closeRecommendationModal}
      title="Trade recommendation"
      width={680}
      footer={[
        <Button key="close" className={styles.actionButton} onClick={controller.closeRecommendationModal}>Close</Button>,
        <Button key="place" type="primary" danger={recommendation?.riskLevel === "High"} disabled={controller.isRecommendationLoading || !recommendation || recommendation.recommendedAction === "Hold"} loading={controller.isBusy || controller.isRecommendationLoading} className={styles.actionButton} onClick={() => void controller.handlePlaceSuggestedTrade()}>Place suggested trade</Button>,
      ]}
    >
      {controller.isRecommendationLoading ? (
        <div className={styles.feedbackBody}>
          <Alert type="info" showIcon title="Building your recommendation" description="Fintex is checking the technical setup, refreshing cached headlines if needed, and blending the market read with the latest Bitcoin and US Dollar news." />
          <Skeleton active paragraph={{ rows: 8 }} />
        </div>
      ) : controller.recommendationRequestError ? (
        <Alert type="warning" showIcon title={controller.recommendationRequestError} />
      ) : recommendation && controller.recommendationTone ? (
        <div className={styles.feedbackBody}>
          <Alert type={controller.recommendationTone} showIcon title={recommendation.headline} description={recommendation.summary} />
          <div className={styles.feedbackMeta}>
            <Tag color={getRiskTone(recommendation.riskLevel)}>{recommendation.riskLevel} risk</Tag>
            <Tag color={getRecommendationActionTone(recommendation.recommendedAction)}>{recommendation.recommendedAction}</Tag>
            <Tag color="blue">Score {recommendation.riskScore.toFixed(1)}</Tag>
            <Tag color="default">Ref {formatPrice(recommendation.referencePrice)}</Tag>
            {recommendation.spread != null ? <Tag color="purple">Spread {formatPrice(recommendation.spread)}</Tag> : null}
          </div>
          <div className={styles.feedbackBlock}><span className={styles.feedbackLabel}>Suggested trade</span>{renderSuggestedTrade()}</div>
          <div className={styles.feedbackBlock}><span className={styles.feedbackLabel}>Why now</span><ul className={styles.feedbackList}>{recommendation.reasons.map((item) => <li key={item}>{item}</li>)}</ul></div>
          <div className={styles.feedbackBlock}><span className={styles.feedbackLabel}>News context</span><Typography.Paragraph className={styles.helper}>{recommendation.newsSummary || "No material cached Bitcoin or US Dollar headlines were added to this recommendation yet."}</Typography.Paragraph><div className={styles.feedbackMeta}><Tag color="blue">Sentiment {recommendation.newsSentiment || "Neutral"}</Tag>{recommendation.newsImpactScore != null ? <Tag color="gold">Impact {recommendation.newsImpactScore.toFixed(1)}</Tag> : null}{recommendation.newsRecommendedAction ? <Tag color={getRecommendationActionTone(recommendation.newsRecommendedAction)}>News bias {recommendation.newsRecommendedAction}</Tag> : null}{recommendation.newsLastUpdatedAt ? <Tag color="default">Updated {formatTime(recommendation.newsLastUpdatedAt)}</Tag> : null}</div>{recommendation.newsHeadlines.length > 0 ? <ul className={styles.feedbackList}>{recommendation.newsHeadlines.map((item) => <li key={item}>{item}</li>)}</ul> : null}</div>
          <div className={styles.feedbackBlock}><span className={styles.feedbackLabel}>Economic calendar</span><Typography.Paragraph className={styles.helper}>{recommendation.economicCalendarSummary || "No nearby CPI, NFP, or FOMC event is currently shading this setup."}</Typography.Paragraph><div className={styles.feedbackMeta}>{recommendation.economicCalendarRiskScore != null ? <Tag color="orange">Macro risk {recommendation.economicCalendarRiskScore.toFixed(1)}</Tag> : null}{recommendation.economicCalendarNextEventAtUtc ? <Tag color="gold">Next event {formatTime(recommendation.economicCalendarNextEventAtUtc)}</Tag> : null}</div>{recommendation.economicCalendarEvents.length > 0 ? <ul className={styles.feedbackList}>{recommendation.economicCalendarEvents.map((item) => <li key={`${item.title}-${item.occursAtUtc}`}>{item.title} · {formatTime(item.occursAtUtc)} · impact {item.impactScore.toFixed(0)} · {item.source}</li>)}</ul> : null}</div>
          <div className={styles.feedbackBlock}><span className={styles.feedbackLabel}>Suggested improvements</span><ul className={styles.feedbackList}>{recommendation.suggestions.map((item) => <li key={item}>{item}</li>)}</ul></div>
          <div className={styles.feedbackMeta}>{recommendation.suggestedStopLoss != null ? <Tag color="red">Suggested SL {formatPrice(recommendation.suggestedStopLoss)}</Tag> : null}{recommendation.suggestedTakeProfit != null ? <Tag color="green">Suggested TP {formatPrice(recommendation.suggestedTakeProfit)}</Tag> : null}{recommendation.confidenceScore != null ? <Tag color="blue">Confidence {recommendation.confidenceScore.toFixed(1)}</Tag> : null}{recommendation.newsImpactScore != null ? <Tag color="gold">News impact {recommendation.newsImpactScore.toFixed(1)}</Tag> : null}</div>
        </div>
      ) : (
        <Alert type="info" showIcon title="No recommendation is ready yet." description="Try requesting a recommendation again once live market data is flowing." />
      )}
    </Modal>
  );
};
