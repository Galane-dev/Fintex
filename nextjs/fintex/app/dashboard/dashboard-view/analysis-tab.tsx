"use client";

import { Alert, Collapse, Progress, Space, Tag, Typography } from "antd";
import type {
  MarketConnectionStatus,
  MarketInsight,
  MarketPriceProjection,
  MarketVerdictSnapshot,
} from "@/types/market-data";
import {
  formatPrice,
  formatSigned,
  formatSignedPoints,
  getConnectionTone,
  getProjectionMaturityLabel,
  getVerdictStateLabel,
  getVerdictStateTone,
  formatTime,
} from "@/utils/market-data";
import { useStyles } from "../style";

interface CalculationItem {
  name: string;
  note: string;
  value: string;
  tone: "positive" | "negative" | "neutral";
}

interface AnalysisTabProps {
  connectionStatus: MarketConnectionStatus;
  error: string | null;
  latestVerdict: string;
  confidenceScore: number | null;
  oneMinuteRsi: number | null;
  macd: number | null;
  macdSignal: number | null;
  momentum: number | null;
  trendScore: number | null;
  adx: number | null;
  timeframeRsiMap: Record<string, number | null>;
  verdict: MarketVerdictSnapshot | null;
  calculations: CalculationItem[];
  marketSignals: MarketInsight[];
  nextOneMinuteProjection: MarketPriceProjection | null;
  nextFiveMinuteProjection: MarketPriceProjection | null;
}

const projectionTone = (delta: number | null | undefined) => {
  if (delta == null) {
    return "neutral";
  }

  return delta >= 0 ? "positive" : "negative";
};

export function AnalysisTab({
  connectionStatus,
  error,
  latestVerdict,
  confidenceScore,
  oneMinuteRsi,
  macd,
  macdSignal,
  momentum,
  trendScore,
  adx,
  timeframeRsiMap,
  verdict,
  calculations,
  marketSignals,
  nextOneMinuteProjection,
  nextFiveMinuteProjection,
}: AnalysisTabProps) {
  const { styles, cx } = useStyles();

  const renderAccordionLabel = (title: string, summary: string, tone?: "positive" | "negative") => (
    <div className={styles.accordionHeader}>
      <span>{title}</span>
      <span
        className={cx(
          styles.accordionSummary,
          tone === "positive" ? styles.positive : undefined,
          tone === "negative" ? styles.negative : undefined,
        )}
      >
        {summary}
      </span>
    </div>
  );

  const renderProjectionCard = (title: string, projection: MarketPriceProjection | null) => {
    const consensusDelta =
      projection?.consensusPrice != null && verdict?.price != null
        ? projection.consensusPrice - verdict.price
        : null;

    return (
      <div className={styles.predictionCard}>
        <div className={styles.predictionHeader}>
          <span className={styles.predictionLabel}>{title}</span>
          {projection ? (
            <span
              className={cx(
                styles.predictionDelta,
                projectionTone(consensusDelta) === "positive" ? styles.positive : undefined,
                projectionTone(consensusDelta) === "negative" ? styles.negative : undefined,
                projectionTone(consensusDelta) === "neutral" ? styles.neutral : undefined,
              )}
            >
              {formatSignedPoints(consensusDelta)}
            </span>
          ) : (
            <Tag color="default">Waiting</Tag>
          )}
        </div>

        <div className={styles.predictionValue}>
          {projection?.consensusPrice != null ? formatPrice(projection.consensusPrice) : "-"}
        </div>

        {projection ? (
          <>
            <div className={styles.predictionFacts}>
              <div className={styles.predictionFact}>
                <span className={styles.predictionFactLabel}>Model</span>
                <span>{projection.modelName}</span>
              </div>
              <div className={styles.predictionFact}>
                <span className={styles.predictionFactLabel}>Maturity</span>
                <span>{getProjectionMaturityLabel(projection.maturity)}</span>
              </div>
              <div className={styles.predictionFact}>
                <span className={styles.predictionFactLabel}>Confidence</span>
                <span>{projection.confidenceScore != null ? `${projection.confidenceScore.toFixed(0)}%` : "-"}</span>
              </div>
              <div className={styles.predictionFact}>
                <span className={styles.predictionFactLabel}>Bars used</span>
                <span>{projection.barsUsed}</span>
              </div>
            </div>
            <div className={styles.predictionMeta}>
              <span>SMA {formatPrice(projection.smaPrice)}</span>
              <span>EMA {formatPrice(projection.emaPrice)}</span>
              <span>SMMA {formatPrice(projection.smmaPrice)}</span>
            </div>
          </>
        ) : (
          <Typography.Paragraph className={styles.predictionEmptyCopy}>
            The backend has not published this estimate yet. Fintex waits until the verdict engine has enough candle coverage to avoid showing weak early projections.
          </Typography.Paragraph>
        )}
      </div>
    );
  };

  return (
    <div className={styles.tabStack}>
      <div className={styles.subPanel}>
        <div className={styles.verdictHero}>
          <div className={styles.verdictRow}>
            <div className={styles.verdictLabel}>
              <Typography.Text type="secondary">Realtime stance</Typography.Text>
              <div className={styles.verdictValue}>{latestVerdict} bias</div>
            </div>
            <Space wrap>
              <Tag color={getConnectionTone(connectionStatus)}>{connectionStatus}</Tag>
              {verdict ? (
                <Tag color={getVerdictStateTone(verdict.verdictState)}>
                  {getVerdictStateLabel(verdict.verdictState)}
                </Tag>
              ) : null}
            </Space>
          </div>

          <div className={styles.scoreBlock}>
            <div className={styles.scoreLabel}>Confidence score</div>
            <div className={styles.scoreValue}>
              {confidenceScore != null ? confidenceScore.toFixed(1) : "-"}
            </div>
            <Progress
              percent={Math.max(0, Math.min(Math.round(confidenceScore ?? 0), 100))}
              showInfo={false}
              strokeColor="#9bf2b1"
              trailColor="rgba(255,255,255,0.08)"
            />
          </div>

          {verdict ? (
            <div className={styles.verdictStateCard}>
              <div className={styles.verdictStateHeader}>
                <span className={styles.scoreLabel}>Verdict engine state</span>
                <span className={styles.verdictStateTimestamp}>
                  Evaluated {formatTime(verdict.evaluatedAtUtc)}
                </span>
              </div>
              <Typography.Paragraph className={styles.verdictStateCopy}>
                {verdict.verdictStateReason}
              </Typography.Paragraph>
            </div>
          ) : null}

          <div className={styles.predictionGrid}>
            {renderProjectionCard("Estimated next 1 minute", nextOneMinuteProjection)}
            {renderProjectionCard("Estimated next 5 minutes from 5m bars", nextFiveMinuteProjection)}
          </div>

          <Typography.Paragraph className={styles.verdictCopy}>
            Multi-timeframe EMA, RSI, MACD, ATR, ADX, structure, and alignment checks now sit beside backend projections, so the dashboard shows the actual verdict engine output instead of inventing its own forecast.
          </Typography.Paragraph>

          <Space wrap>
            <Tag color="green">MACD {formatSigned(macd)}</Tag>
            <Tag color="blue">Signal {formatSigned(macdSignal)}</Tag>
            <Tag color="lime">Momentum {formatSignedPoints(momentum)}</Tag>
            <Tag color="gold">RSI 1m {oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-"}</Tag>
            <Tag color="blue">Trend {trendScore != null ? formatSigned(trendScore, 0) : "-"}</Tag>
            <Tag color="purple">ADX {adx != null ? adx.toFixed(1) : "-"}</Tag>
          </Space>
        </div>
      </div>

      <Collapse
        className={styles.analysisCollapse}
        ghost
        defaultActiveKey={["rsi"]}
        items={[
          {
            key: "rsi",
            label: renderAccordionLabel(
              "RSI by timeframe",
              `1m ${oneMinuteRsi != null ? oneMinuteRsi.toFixed(1) : "-"}`,
              oneMinuteRsi == null ? undefined : oneMinuteRsi >= 65 ? "positive" : oneMinuteRsi <= 35 ? "negative" : undefined,
            ),
            children: (
              <div className={styles.metricList}>
                {["1m", "5m", "15m", "1h", "4h"].map((timeframeKey) => {
                  const itemValue = timeframeKey === "1m" ? oneMinuteRsi : timeframeRsiMap[timeframeKey] ?? null;

                  return (
                    <div key={timeframeKey} className={styles.metricRow}>
                      <div className={styles.metricMeta}>
                        <span className={styles.metricName}>{timeframeKey}</span>
                        <span className={styles.metricNote}>Wilder RSI based on {timeframeKey} candles</span>
                      </div>
                      <span className={cx(styles.metricValue, itemValue == null ? styles.neutral : itemValue >= 65 ? styles.positive : itemValue <= 35 ? styles.negative : styles.neutral)}>
                        {itemValue != null ? itemValue.toFixed(1) : "-"}
                      </span>
                    </div>
                  );
                })}
              </div>
            ),
          },
          {
            key: "timeframes",
            label: renderAccordionLabel("Timeframe confirmation", verdict?.timeframeAlignmentScore != null ? formatSigned(verdict.timeframeAlignmentScore, 0) : "-"),
            children: (
              <div className={styles.metricList}>
                {(verdict?.timeframeSignals ?? []).map((item) => (
                  <div key={item.timeframe} className={styles.metricRow}>
                    <div className={styles.metricMeta}>
                      <span className={styles.metricName}>{item.timeframe}</span>
                      <span className={styles.metricNote}>Cross-timeframe directional bias</span>
                    </div>
                    <span className={cx(styles.metricValue, item.signal === "Bullish" ? styles.positive : undefined, item.signal === "Bearish" ? styles.negative : undefined, item.signal === "Neutral" ? styles.neutral : undefined)}>
                      {item.biasScore != null ? formatSigned(item.biasScore, 0) : "-"}
                    </span>
                  </div>
                ))}
              </div>
            ),
          },
          {
            key: "calculations",
            label: renderAccordionLabel("Live calculations", `ADX ${adx != null ? adx.toFixed(1) : "-"}`, adx != null && adx >= 25 ? "positive" : undefined),
            children: (
              <div className={styles.metricList}>
                {calculations.map((item) => (
                  <div key={item.name} className={styles.metricRow}>
                    <div className={styles.metricMeta}>
                      <span className={styles.metricName}>{item.name}</span>
                      <span className={styles.metricNote}>{item.note}</span>
                    </div>
                    <span className={cx(styles.metricValue, item.tone === "positive" ? styles.positive : undefined, item.tone === "negative" ? styles.negative : undefined, item.tone === "neutral" ? styles.neutral : undefined)}>
                      {item.value}
                    </span>
                  </div>
                ))}
              </div>
            ),
          },
          {
            key: "signals",
            label: renderAccordionLabel("Signal desk", marketSignals.length > 0 ? marketSignals[0]?.tag ?? "-" : "-"),
            children: (
              <div className={styles.signalList}>
                {marketSignals.map((item) => (
                  <div key={item.title} className={styles.signalItem}>
                    <div className={styles.signalHeading}>
                      <span className={styles.signalTitle}>{item.title}</span>
                      <Tag color={item.tone}>{item.tag}</Tag>
                    </div>
                    <Typography.Paragraph className={styles.signalCopy}>{item.copy}</Typography.Paragraph>
                  </div>
                ))}
              </div>
            ),
          },
        ]}
      />

      {error ? <Alert title={error} type="warning" showIcon /> : null}
    </div>
  );
}
