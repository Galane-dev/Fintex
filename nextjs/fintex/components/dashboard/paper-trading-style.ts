"use client";

import { createStyles } from "antd-style";

export const usePaperTradingStyles = createStyles(({ css, token }) => ({
  wrapper: css`
    display: grid;
    gap: 16px;
  `,
  helper: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    line-height: 1.7 !important;
  `,
  metrics: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
  `,
  metricCard: css`
    padding: 14px 16px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
  `,
  metricLabel: css`
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: ${token.colorTextSecondary};
  `,
  metricValue: css`
    margin-top: 8px;
    color: ${token.colorText};
    font-size: 22px;
    font-weight: 600;
  `,
  green: css`
    color: #7cf0a1 !important;
  `,
  red: css`
    color: #ff7875 !important;
  `,
  dim: css`
    color: ${token.colorTextSecondary} !important;
  `,
  formGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
    }
  `,
  formActions: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
  `,
  section: css`
    display: grid;
    gap: 12px;
  `,
  sectionHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  `,
  sectionTitle: css`
    color: ${token.colorText};
    font-weight: 600;
  `,
  list: css`
    display: grid;
    gap: 10px;
  `,
  item: css`
    padding: 14px 16px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 8px;
  `,
  itemTop: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
  `,
  itemTitle: css`
    color: ${token.colorText};
    font-weight: 600;
  `,
  itemMeta: css`
    display: flex;
    flex-wrap: wrap;
    gap: 8px 14px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  empty: css`
    padding: 20px 0 8px;
  `,
}));
