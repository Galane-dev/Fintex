"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  terminal: css`
    height: 100%;
    border-radius: 24px;
    background: #050607;
    border: 1px solid rgba(255, 255, 255, 0.07);
    overflow: hidden;
    display: flex;
    flex-direction: column;
  `,
  header: css`
    padding: 18px 20px 14px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);
    display: grid;
    gap: 14px;
  `,
  symbolRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
  `,
  symbolWrap: css`
    display: flex;
    align-items: baseline;
    gap: 12px;
    flex-wrap: wrap;
  `,
  symbol: css`
    font-size: 24px;
    font-weight: 600;
    letter-spacing: -0.04em;
    color: ${token.colorText};
  `,
  price: css`
    font-size: 20px;
    font-weight: 500;
    color: #c9f9d4;
  `,
  positive: css`
    color: #7cf0a1 !important;
  `,
  negative: css`
    color: #ff7875 !important;
  `,
  metaRow: css`
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 900px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  `,
  liveMetaRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
  `,
  statTile: css`
    padding: 10px 12px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.05);
  `,
  statLabel: css`
    font-size: 11px;
    color: ${token.colorTextSecondary};
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  statValue: css`
    margin-top: 6px;
    font-size: 15px;
    font-weight: 500;
    color: ${token.colorText};
  `,
  canvasWrap: css`
    position: relative;
    flex: 1;
    min-height: 560px;
    background:
      linear-gradient(180deg, rgba(255, 255, 255, 0.015), rgba(255, 255, 255, 0)),
      #050607;
  `,
  errorWrap: css`
    padding: 14px 18px 0;
  `,
  chartCanvas: css`
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
  `,
  footerBar: css`
    padding: 14px 18px;
    border-top: 1px solid rgba(255, 255, 255, 0.06);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
  `,
  legend: css`
    display: flex;
    align-items: center;
    gap: 18px;
    flex-wrap: wrap;
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  legendItem: css`
    display: inline-flex;
    align-items: center;
    gap: 8px;
  `,
  legendDotBull: css`
    width: 8px;
    height: 8px;
    border-radius: 999px;
    background: #00c853;
  `,
  legendDotBear: css`
    width: 8px;
    height: 8px;
    border-radius: 999px;
    background: #d6f49e;
  `,
  legendDotSignal: css`
    width: 8px;
    height: 8px;
    border-radius: 999px;
    background: #60a5fa;
  `,
}));
