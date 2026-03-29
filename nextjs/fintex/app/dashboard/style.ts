"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    background:
      radial-gradient(circle at top right, rgba(155, 242, 177, 0.08), transparent 18%),
      #020303;
    padding: 24px 0 36px;
    overflow-x: hidden;

    @media (min-width: 1181px) {
      height: 100vh;
      overflow: hidden;
      padding: 24px 0;
    }
  `,
  shell: css`
    width: min(1480px, calc(100vw - 24px));
    max-width: calc(100vw - 24px);
    margin: 0 auto;

    @media (min-width: 1181px) {
      height: 100%;
      display: flex;
      flex-direction: column;
      min-height: 0;
    }
  `,
  header: css`
    margin-bottom: 18px;
    padding: 10px 0 0;
    border-radius: 22px;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 16px;
    flex-wrap: wrap;
  `,
  headingWrap: css`
    display: grid;
    gap: 4px;
  `,
  eyebrow: css`
    font-size: 11px;
    color: #9bf2b1;
    text-transform: uppercase;
    letter-spacing: 0.12em;
    font-weight: 600;
  `,
  title: css`
    margin: 0 !important;
    color: ${token.colorText} !important;
    font-size: 28px !important;
    letter-spacing: -0.04em;
  `,
  helper: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
  `,
  workspace: css`
    display: grid;
    grid-template-columns: minmax(0, 1.9fr) minmax(340px, 0.86fr);
    gap: 18px;
    align-items: start;
    min-width: 0;

    @media (max-width: 1180px) {
      grid-template-columns: 1fr;
    }

    @media (min-width: 1181px) {
      flex: 1;
      min-height: 0;
      align-items: stretch;
    }
  `,
  chartColumn: css`
    min-width: 0;

    @media (min-width: 1181px) {
      min-height: 0;
      height: 100%;
      display: flex;
      flex-direction: column;
    }
  `,
  sideColumn: css`
    display: grid;
    gap: 18px;
    min-width: 0;

    @media (min-width: 1181px) {
      min-height: 0;
      overflow-y: auto;
      overflow-x: hidden;
      padding-right: 6px;

      &::-webkit-scrollbar {
        width: 10px;
      }

      &::-webkit-scrollbar-thumb {
        background: rgba(255, 255, 255, 0.12);
        border-radius: 999px;
      }
    }
  `,
  dashboardTabs: css`
    .ant-tabs-nav {
      margin-bottom: 18px !important;
    }

    .ant-tabs-tab {
      padding: 8px 0 !important;
    }

    .ant-tabs-tab-btn {
      font-weight: 600;
    }
  `,
  overviewStrip: css`
    margin-bottom: 18px;
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 1100px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
    }
  `,
  overviewCard: css`
    padding: 16px 18px;
    border-radius: 20px;
    background: rgba(7, 8, 9, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.06);
    display: grid;
    gap: 6px;
  `,
  overviewLabel: css`
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: ${token.colorTextSecondary};
  `,
  overviewValue: css`
    color: ${token.colorText};
    font-size: 22px;
    font-weight: 600;
    line-height: 1.2;
  `,
  overviewNote: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  panelCard: css`
    border-radius: 22px !important;
    background: rgba(7, 8, 9, 0.96) !important;
    border: 1px solid rgba(255, 255, 255, 0.06) !important;
    box-shadow: none !important;

    .ant-card-head {
      border-bottom: 1px solid rgba(255, 255, 255, 0.05) !important;
      min-height: 54px;
    }

    .ant-card-head-title {
      color: ${token.colorText};
      font-weight: 600;
    }
  `,
  verdictHero: css`
    display: grid;
    gap: 18px;
  `,
  tabStack: css`
    display: grid;
    gap: 16px;
  `,
  analysisCollapse: css`
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.025);
    border: 1px solid rgba(255, 255, 255, 0.05);
    overflow: hidden;

    .ant-collapse-item {
      border-bottom: 1px solid rgba(255, 255, 255, 0.06);
    }

    .ant-collapse-item:last-child {
      border-bottom: none;
    }

    .ant-collapse-header {
      align-items: center !important;
      padding: 16px 18px !important;
      font-weight: 600;
      color: ${token.colorText} !important;
    }

    .ant-collapse-content {
      background: transparent !important;
      border-top: 1px solid rgba(255, 255, 255, 0.04);
    }

    .ant-collapse-content-box {
      padding: 4px 18px 16px !important;
    }
  `,
  accordionHeader: css`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    min-width: 0;
  `,
  accordionSummary: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
    font-weight: 500;
    white-space: nowrap;
    text-align: right;
  `,
  subPanel: css`
    padding: 18px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.025);
    border: 1px solid rgba(255, 255, 255, 0.05);
  `,
  sectionHeading: css`
    margin-bottom: 14px;
    color: ${token.colorText};
    font-weight: 600;
    font-size: 15px;
  `,
  verdictRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 18px;
  `,
  verdictLabel: css`
    display: grid;
    gap: 6px;
  `,
  verdictValue: css`
    font-size: 38px;
    line-height: 1;
    font-weight: 600;
    letter-spacing: -0.05em;
    color: #9bf2b1;
  `,
  verdictCopy: css`
    color: ${token.colorTextSecondary};
    line-height: 1.75;
  `,
  scoreBlock: css`
    padding: 14px;
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.05);
  `,
  scoreLabel: css`
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: ${token.colorTextSecondary};
  `,
  scoreValue: css`
    margin-top: 8px;
    font-size: 30px;
    font-weight: 600;
    letter-spacing: -0.04em;
    color: ${token.colorText};
  `,
  predictionGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
    }
  `,
  predictionCard: css`
    padding: 14px 16px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 8px;
    min-width: 0;
  `,
  predictionHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
  `,
  predictionLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  predictionDelta: css`
    font-size: 12px;
    font-weight: 600;
    white-space: nowrap;
  `,
  predictionValue: css`
    color: ${token.colorText};
    font-size: 22px;
    line-height: 1.1;
    font-weight: 600;
    letter-spacing: -0.03em;
  `,
  predictionMeta: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px 14px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
    line-height: 1.6;
  `,
  metricList: css`
    display: grid;
    gap: 0;
  `,
  metricRow: css`
    padding: 16px 2px;
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 18px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);

    &:last-child {
      padding-bottom: 0;
      border-bottom: none;
    }
  `,
  metricMeta: css`
    display: grid;
    gap: 6px;
  `,
  metricName: css`
    color: ${token.colorText};
    font-weight: 500;
  `,
  metricNote: css`
    font-size: 12px;
    color: ${token.colorTextSecondary};
    line-height: 1.7;
  `,
  metricValue: css`
    font-size: 22px;
    font-weight: 600;
    color: ${token.colorText};
    line-height: 1.1;
    text-align: right;
  `,
  positive: css`
    color: #7cf0a1 !important;
  `,
  negative: css`
    color: #ff7875 !important;
  `,
  neutral: css`
    color: #d9d9d9 !important;
  `,
  signalList: css`
    display: grid;
    gap: 0;
  `,
  signalItem: css`
    padding: 16px 2px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);

    &:last-child {
      padding-bottom: 0;
      border-bottom: none;
    }
  `,
  signalHeading: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 8px;
  `,
  signalTitle: css`
    color: ${token.colorText};
    font-weight: 500;
  `,
  signalCopy: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    line-height: 1.7 !important;
  `,
  positionsList: css`
    display: grid;
    gap: 14px;
  `,
  positionCard: css`
    padding: 18px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.025);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 14px;
  `,
  positionHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
    flex-wrap: wrap;
  `,
  positionTitle: css`
    color: ${token.colorText};
    font-size: 16px;
    font-weight: 600;
  `,
  positionSubtle: css`
    margin-top: 4px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  positionMetrics: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
    }
  `,
  positionMetric: css`
    display: grid;
    gap: 6px;
  `,
  positionMetricLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  positionMetricValue: css`
    color: ${token.colorText};
    font-size: 16px;
    font-weight: 600;
  `,
  closedTradeReviewPanel: css`
    display: grid;
    gap: 12px;
    padding-top: 14px;
    border-top: 1px solid rgba(255, 255, 255, 0.06);
  `,
  closedTradeReviewLine: css`
    display: grid;
    gap: 6px;
  `,
  closedTradeReviewLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.09em;
  `,
  closedTradeReviewCopy: css`
    margin: 0 !important;
    color: ${token.colorText} !important;
    line-height: 1.7 !important;
  `,
  emptyState: css`
    padding: 20px 0 8px;
  `,
  behaviorPanel: css`
    display: grid;
    gap: 18px;
  `,
  behaviorHero: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
    flex-wrap: wrap;
  `,
  behaviorLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.1em;
  `,
  behaviorValue: css`
    margin-top: 8px;
    color: ${token.colorText};
    font-size: 32px;
    font-weight: 600;
  `,
  behaviorBlock: css`
    display: grid;
    gap: 8px;
  `,
  behaviorSectionTitle: css`
    color: ${token.colorText};
    font-weight: 600;
    font-size: 15px;
  `,
  behaviorGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
    }
  `,
  summaryCard: css`
    padding: 14px 16px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.06);
    display: grid;
    gap: 8px;
  `,
  summaryLabel: css`
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: ${token.colorTextSecondary};
  `,
  summaryValue: css`
    color: ${token.colorText};
    font-size: 16px;
    font-weight: 600;
  `,
}));
