"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    display: grid;
    grid-template-columns: minmax(360px, 520px) 1fr;
    background:
      radial-gradient(circle at top left, rgba(52, 245, 197, 0.16), transparent 24%),
      radial-gradient(circle at bottom right, rgba(121, 199, 255, 0.14), transparent 28%),
      linear-gradient(180deg, #05070b 0%, #07111a 100%);

    @media (max-width: 980px) {
      grid-template-columns: 1fr;
    }
  `,
  panel: css`
    padding: 48px;
    border-right: 1px solid rgba(118, 154, 198, 0.14);
    background:
      linear-gradient(180deg, rgba(8, 13, 20, 0.92), rgba(5, 7, 11, 0.94)),
      radial-gradient(circle at top left, rgba(52, 245, 197, 0.12), transparent 24%);

    @media (max-width: 980px) {
      padding: 32px 20px 12px;
      border-right: 0;
      border-bottom: 1px solid rgba(118, 154, 198, 0.14);
    }
  `,
  panelShell: css`
    max-width: 420px;
  `,
  brand: css`
    display: inline-flex;
    align-items: center;
    gap: 14px;
    margin-bottom: 40px;
  `,
  brandBadge: css`
    width: 42px;
    height: 42px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 14px;
    background: linear-gradient(135deg, #34f5c5 0%, #79c7ff 100%);
    color: #051017;
    font-weight: 800;
    font-size: 18px;
  `,
  content: css`
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 48px 20px;
  `,
  card: css`
    width: min(520px, 100%);
    border-radius: 28px;
    background: rgba(9, 14, 22, 0.9);
    border: 1px solid rgba(118, 154, 198, 0.16);
    box-shadow: 0 30px 80px rgba(0, 0, 0, 0.35);
  `,
  title: css`
    margin: 0 0 10px !important;
    font-size: clamp(34px, 4.2vw, 56px) !important;
    line-height: 0.98 !important;
    letter-spacing: -0.05em;
    font-weight: 300 !important;
  `,
  copy: css`
    color: ${token.colorTextSecondary} !important;
    line-height: 1.8 !important;
    max-width: 360px;
  `,
  miniCard: css`
    padding: 18px;
    border-radius: 22px;
    background: rgba(7, 12, 19, 0.88);
    border: 1px solid rgba(118, 154, 198, 0.12);
  `,
  heading: css`
    margin-bottom: 8px !important;
  `,
  helper: css`
    color: ${token.colorTextSecondary} !important;
  `,
  footerRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 16px;
    margin-top: 8px;

    @media (max-width: 560px) {
      flex-direction: column;
      align-items: flex-start;
    }
  `,
}));
