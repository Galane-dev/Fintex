"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  terminal: css`
    height: 100%;
    min-height: 0;
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
  actionBar: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
    flex-wrap: wrap;
  `,
  actionButton: css`
    min-width: 132px;
    height: 40px;
    border-radius: 8px !important;
    font-weight: 600;
    box-shadow: none;
  `,
  buyButton: css`
    background: #4be16b !important;
    border-color: #4be16b !important;
    color: #041106 !important;

    &:hover,
    &:focus {
      background: #6bec86 !important;
      border-color: #6bec86 !important;
      color: #041106 !important;
    }
  `,
  sellButton: css`
    border-radius: 8px !important;
  `,
  liveMetaRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
  `,
  canvasWrap: css`
    position: relative;
    flex: 1;
    min-height: 560px;
    background:
      linear-gradient(180deg, rgba(255, 255, 255, 0.015), rgba(255, 255, 255, 0)),
      #050607;
    overflow: hidden;

    @media (min-width: 1181px) {
      height: 100%;
      min-height: 0;
    }
  `,
  errorWrap: css`
    padding: 14px 18px 0;
  `,
  chartCanvas: css`
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    cursor: crosshair;
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
  legendDotEntry: css`
    width: 8px;
    height: 8px;
    border-radius: 999px;
    background: #93c5fd;
  `,
  legendDotSpread: css`
    width: 8px;
    height: 8px;
    border-radius: 999px;
    background: #c084fc;
  `,
}));
