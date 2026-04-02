"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  page: css`
    min-height: 100vh;
    display: grid;
    grid-template-columns: minmax(360px, 500px) 1fr;
    background:
      radial-gradient(circle at 14% 15%, rgba(70, 220, 112, 0.2), transparent 30%),
      radial-gradient(circle at 90% 80%, rgba(19, 156, 76, 0.18), transparent 35%),
      linear-gradient(180deg, #050805 0%, #040604 100%);

    @media (max-width: 980px) {
      grid-template-columns: 1fr;
    }
  `,
  panel: css`
    padding: 56px 44px;
    border-right: 1px solid rgba(74, 186, 105, 0.2);
    background:
      linear-gradient(180deg, rgba(7, 11, 7, 0.9), rgba(5, 7, 5, 0.94)),
      radial-gradient(circle at top left, rgba(63, 188, 99, 0.2), transparent 30%);
    position: relative;
    overflow: hidden;

    &::after {
      content: "";
      position: absolute;
      inset: 28px;
      border: 1px solid rgba(90, 198, 122, 0.16);
      border-radius: 24px;
      pointer-events: none;
    }

    @media (max-width: 980px) {
      display: none;
    }
  `,
  panelShell: css`
    max-width: 420px;
    position: relative;
    z-index: 1;
  `,
  brand: css`
    display: inline-flex;
    align-items: center;
    gap: 14px;
    margin-bottom: 40px;
  `,
  brandBadge: css`
    width: 40px;
    height: 40px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 12px;
    border: 1px solid rgba(104, 230, 142, 0.45);
    background: linear-gradient(145deg, #0f2d15 0%, #0a180d 100%);
    box-shadow:
      0 0 0 1px rgba(17, 42, 24, 0.8),
      0 10px 26px rgba(0, 0, 0, 0.35),
      0 0 24px rgba(64, 196, 110, 0.16);
    color: #8af4ad;
    font-weight: 800;
    font-size: 16px;
  `,
  content: css`
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 40px 24px;

    @media (max-width: 980px) {
      min-height: 100vh;
      padding: 20px 14px;
    }
  `,
  card: css`
    width: min(560px, 100%);
    border-radius: 24px;
    background: linear-gradient(180deg, rgba(10, 14, 10, 0.97), rgba(6, 8, 6, 0.97));
    border: 1px solid rgba(86, 205, 119, 0.26);
    box-shadow:
      0 28px 65px rgba(0, 0, 0, 0.45),
      0 0 0 1px rgba(18, 38, 23, 0.8);
    backdrop-filter: blur(8px);

    .ant-card-body {
      padding: 30px;
    }

    .ant-form-item-label > label {
      color: rgba(208, 229, 212, 0.92);
      font-weight: 500;
      letter-spacing: 0.01em;
    }

    .ant-input-affix-wrapper,
    .ant-input,
    .ant-input-password,
    .ant-input-password .ant-input {
      background: rgba(7, 12, 7, 0.96);
      border-color: rgba(83, 174, 106, 0.3);
      color: #e6f8ea;
      border-radius: 12px;
    }

    .ant-input-affix-wrapper:hover,
    .ant-input:hover,
    .ant-input-password:hover {
      border-color: rgba(104, 220, 136, 0.44);
    }

    .ant-input-affix-wrapper-focused,
    .ant-input:focus,
    .ant-input-focused,
    .ant-input-password-focused {
      border-color: rgba(106, 229, 140, 0.75) !important;
      box-shadow: 0 0 0 3px rgba(104, 218, 132, 0.14) !important;
    }

    .ant-input::placeholder {
      color: rgba(165, 191, 171, 0.62);
    }

    .anticon {
      color: rgba(122, 204, 146, 0.88);
    }

    .ant-checkbox-wrapper {
      color: rgba(205, 226, 210, 0.95);
    }
  `,
  overline: css`
    display: inline-flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 10px;
    color: #8be9a9;
    text-transform: uppercase;
    letter-spacing: 0.11em;
    font-size: 11px;
    font-weight: 700;
  `,
  title: css`
    margin: 0 0 10px !important;
    font-size: clamp(32px, 4vw, 52px) !important;
    line-height: 1.02 !important;
    letter-spacing: -0.03em;
    font-weight: 500 !important;
  `,
  highlightTitle: css`
    margin: 0 0 10px !important;
    font-size: clamp(24px, 2.9vw, 36px) !important;
    line-height: 1.12 !important;
    letter-spacing: -0.02em;
    font-weight: 500 !important;
  `,
  copy: css`
    color: rgba(181, 208, 187, 0.9) !important;
    line-height: 1.75 !important;
    max-width: 360px;
  `,
  miniCard: css`
    padding: 22px;
    border-radius: 18px;
    background: rgba(6, 9, 6, 0.9);
    border: 1px solid rgba(73, 174, 103, 0.22);
  `,
  heading: css`
    margin-bottom: 8px !important;
    color: #f4fff6 !important;
    font-size: clamp(26px, 3vw, 34px) !important;
    letter-spacing: -0.02em;
  `,
  helper: css`
    color: rgba(176, 198, 181, 0.88) !important;
    margin-bottom: 0 !important;
  `,
  nameGrid: css`
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;

    @media (max-width: 560px) {
      grid-template-columns: 1fr;
      gap: 0;
    }
  `,
  submitButton: css`
    height: 46px;
    border-radius: 12px !important;
    font-weight: 600;
    font-size: 15px;
    border: 1px solid rgba(93, 216, 127, 0.65) !important;
    background: linear-gradient(180deg, #2f9a4f 0%, #1a6b34 100%) !important;
    box-shadow: 0 10px 24px rgba(16, 73, 34, 0.45);
  `,
  link: css`
    color: #7ceaa0 !important;
    font-weight: 600;

    &:hover {
      color: #9ef7ba !important;
    }
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
