"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    background:
      radial-gradient(circle at top right, rgba(155, 242, 177, 0.08), transparent 18%),
      #020303;
    padding: 24px 0 36px;
  `,
  shell: css`
    width: min(1480px, calc(100vw - 28px));
    margin: 0 auto;
  `,
  header: css`
    margin-bottom: 18px;
    padding: 16px 18px;
    border-radius: 22px;
    background: rgba(7, 8, 9, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.06);
    display: flex;
    align-items: center;
    justify-content: space-between;
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

    @media (max-width: 1180px) {
      grid-template-columns: 1fr;
    }
  `,
  chartColumn: css`
    min-width: 0;

    @media (min-width: 1181px) {
      position: sticky;
      top: 24px;
      align-self: start;
    }
  `,
  sideColumn: css`
    display: grid;
    gap: 18px;
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
  metricList: css`
    display: grid;
    gap: 12px;
  `,
  metricRow: css`
    padding: 14px 16px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  `,
  metricMeta: css`
    display: grid;
    gap: 5px;
  `,
  metricName: css`
    color: ${token.colorText};
    font-weight: 500;
  `,
  metricNote: css`
    font-size: 12px;
    color: ${token.colorTextSecondary};
  `,
  metricValue: css`
    font-size: 18px;
    font-weight: 600;
    color: ${token.colorText};
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
    gap: 12px;
  `,
  signalItem: css`
    padding: 14px 16px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
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
  miniStrip: css`
    margin-top: 18px;
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 900px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  `,
  miniCard: css`
    padding: 14px 16px;
    border-radius: 18px;
    background: rgba(7, 8, 9, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.06);
  `,
  miniLabel: css`
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: ${token.colorTextSecondary};
  `,
  miniValue: css`
    margin-top: 8px;
    color: ${token.colorText};
    font-size: 20px;
    font-weight: 600;
  `,
}));
