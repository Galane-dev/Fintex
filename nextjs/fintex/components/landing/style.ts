"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    background:
      radial-gradient(circle at top right, rgba(52, 245, 197, 0.12), transparent 22%),
      radial-gradient(circle at left center, rgba(85, 110, 255, 0.12), transparent 25%),
      linear-gradient(180deg, #05070b 0%, #070d16 45%, #05070b 100%);
    color: ${token.colorText};
  `,
  shell: css`
    width: min(1220px, calc(100vw - 32px));
    margin: 0 auto;
  `,
  navWrap: css`
    position: sticky;
    top: 0;
    z-index: 20;
    backdrop-filter: blur(18px);
    background: rgba(5, 7, 11, 0.72);
    border-bottom: 1px solid rgba(118, 154, 198, 0.16);
  `,
  navInner: css`
    min-height: 84px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 24px;
  `,
  brand: css`
    display: inline-flex;
    align-items: center;
    gap: 14px;
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
  brandText: css`
    font-size: 24px;
    font-weight: 600;
    letter-spacing: -0.03em;
  `,
  navLinks: css`
    display: flex;
    align-items: center;
    gap: 28px;

    @media (max-width: 900px) {
      display: none;
    }
  `,
  navLink: css`
    color: ${token.colorTextSecondary};
    transition: color 0.2s ease;

    &:hover {
      color: ${token.colorText};
    }
  `,
  navActions: css`
    display: flex;
    align-items: center;
    gap: 12px;

    @media (max-width: 900px) {
      display: none;
    }
  `,
  mobileMenu: css`
    display: none;

    @media (max-width: 900px) {
      display: inline-flex;
    }
  `,
  hero: css`
    position: relative;
    min-height: calc(100vh - 84px);
    overflow: hidden;
  `,
  heroInner: css`
    position: relative;
    z-index: 1;
    display: grid;
    grid-template-columns: minmax(0, 1.1fr) minmax(340px, 0.9fr);
    gap: 48px;
    align-items: center;
    padding: 56px 0 96px;

    @media (max-width: 1080px) {
      grid-template-columns: 1fr;
      padding-top: 32px;
    }
  `,
  heroText: css`
    max-width: 680px;
  `,
  pill: css`
    display: inline-flex;
    align-items: center;
    gap: 10px;
    padding: 10px 16px;
    border-radius: 999px;
    background: rgba(52, 245, 197, 0.08);
    border: 1px solid rgba(52, 245, 197, 0.18);
    color: #7cfbd9;
    font-size: 13px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  heroTitle: css`
    margin: 22px 0 18px !important;
    font-size: clamp(52px, 7vw, 88px) !important;
    line-height: 0.95 !important;
    letter-spacing: -0.06em;
    font-weight: 300 !important;
  `,
  heroAccent: css`
    background: linear-gradient(135deg, #34f5c5 0%, #79c7ff 100%);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
  `,
  heroCopy: css`
    max-width: 620px;
    margin-bottom: 28px !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 18px;
    line-height: 1.8 !important;
  `,
  heroActions: css`
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
    margin-bottom: 34px;
  `,
  metrics: css`
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 16px;
    margin-top: 42px;

    @media (max-width: 720px) {
      grid-template-columns: 1fr;
    }
  `,
  metricCard: css`
    padding: 20px 22px;
    border-radius: 24px;
    background: rgba(10, 16, 24, 0.78);
    border: 1px solid rgba(118, 154, 198, 0.18);
    backdrop-filter: blur(16px);
  `,
  metricValue: css`
    font-size: 30px;
    line-height: 1.1;
    font-weight: 300;
    letter-spacing: -0.04em;
  `,
  metricLabel: css`
    margin-top: 6px;
    color: ${token.colorTextSecondary};
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  heroPanel: css`
    position: relative;
    min-height: 620px;
    border-radius: 32px;
    overflow: hidden;
    background:
      linear-gradient(180deg, rgba(13, 22, 34, 0.48), rgba(5, 7, 11, 0.92)),
      radial-gradient(circle at top left, rgba(52, 245, 197, 0.18), transparent 28%),
      rgba(10, 16, 24, 0.92);
    border: 1px solid rgba(118, 154, 198, 0.16);
    box-shadow: ${token.boxShadowSecondary};
  `,
  chartCanvas: css`
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
  `,
  floatingPanel: css`
    position: absolute;
    right: 24px;
    left: 24px;
    bottom: 24px;
    padding: 22px;
    border-radius: 24px;
    background: rgba(6, 12, 18, 0.88);
    border: 1px solid rgba(118, 154, 198, 0.16);
    backdrop-filter: blur(16px);
  `,
  floatingLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  floatingValue: css`
    margin-top: 8px;
    font-size: 42px;
    font-weight: 300;
    letter-spacing: -0.06em;
  `,
  floatingSubText: css`
    margin-top: 10px !important;
    color: ${token.colorTextSecondary} !important;
    line-height: 1.7 !important;
  `,
  section: css`
    padding: 112px 0;
  `,
  sectionMuted: css`
    padding: 112px 0;
    background: rgba(7, 13, 22, 0.8);
    border-top: 1px solid rgba(118, 154, 198, 0.08);
    border-bottom: 1px solid rgba(118, 154, 198, 0.08);
  `,
  sectionHeader: css`
    max-width: 720px;
    margin-bottom: 40px;
  `,
  sectionTitle: css`
    margin: 18px 0 12px !important;
    font-size: clamp(34px, 4vw, 56px) !important;
    line-height: 1.02 !important;
    letter-spacing: -0.05em;
    font-weight: 300 !important;
  `,
  sectionCopy: css`
    color: ${token.colorTextSecondary} !important;
    font-size: 17px;
    line-height: 1.8 !important;
  `,
  featureCard: css`
    height: 100%;
    border-radius: 28px;
    background: rgba(6, 11, 17, 0.82);
    border: 1px solid rgba(118, 154, 198, 0.16);
    transition:
      transform 0.2s ease,
      border-color 0.2s ease;

    &:hover {
      transform: translateY(-4px);
      border-color: rgba(52, 245, 197, 0.38);
    }
  `,
  featureIcon: css`
    width: 56px;
    height: 56px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 18px;
    background: rgba(52, 245, 197, 0.1);
    border: 1px solid rgba(52, 245, 197, 0.24);
    color: #7cfbd9;
    font-size: 24px;
  `,
  featureTitle: css`
    margin: 20px 0 10px !important;
    font-size: 22px !important;
    font-weight: 500 !important;
    letter-spacing: -0.03em;
  `,
  featureCopy: css`
    color: ${token.colorTextSecondary} !important;
    line-height: 1.8 !important;
  `,
  marketGrid: css`
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 18px;

    @media (max-width: 980px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (max-width: 680px) {
      grid-template-columns: 1fr;
    }
  `,
  marketCard: css`
    border-radius: 24px;
    background: rgba(6, 11, 17, 0.9);
    border: 1px solid rgba(118, 154, 198, 0.16);
  `,
  marketSymbol: css`
    color: ${token.colorTextSecondary};
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  marketPrice: css`
    margin-top: 10px;
    font-size: 34px;
    font-weight: 300;
    letter-spacing: -0.05em;
  `,
  ctaPanel: css`
    margin-top: 28px;
    padding: 28px;
    border-radius: 28px;
    background: linear-gradient(135deg, rgba(52, 245, 197, 0.12), rgba(84, 124, 255, 0.14));
    border: 1px solid rgba(82, 242, 201, 0.26);
  `,
  platformCard: css`
    height: 100%;
    overflow: hidden;
    border-radius: 28px;
    background: rgba(8, 13, 20, 0.92);
    border: 1px solid rgba(118, 154, 198, 0.16);
  `,
  platformVisual: css`
    min-height: 220px;
    padding: 24px;
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
  `,
  platformMini: css`
    width: 100%;
    padding: 18px;
    border-radius: 22px;
    background: rgba(4, 8, 12, 0.7);
    border: 1px solid rgba(245, 251, 255, 0.08);
  `,
  bulletList: css`
    display: grid;
    gap: 10px;
  `,
  bulletItem: css`
    display: flex;
    align-items: center;
    gap: 10px;
    color: ${token.colorTextSecondary};
  `,
  bulletDot: css`
    width: 8px;
    height: 8px;
    border-radius: 999px;
    background: #34f5c5;
    box-shadow: 0 0 12px rgba(52, 245, 197, 0.45);
  `,
  footer: css`
    padding: 72px 0 36px;
    border-top: 1px solid rgba(118, 154, 198, 0.12);
    background: rgba(4, 7, 12, 0.92);
  `,
  footerCopy: css`
    color: ${token.colorTextSecondary} !important;
    line-height: 1.8 !important;
  `,
  footerBottom: css`
    margin-top: 34px;
    padding-top: 24px;
    border-top: 1px solid rgba(118, 154, 198, 0.12);
    color: ${token.colorTextSecondary};
  `,
}));
