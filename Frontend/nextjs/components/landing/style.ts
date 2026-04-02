"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    background:
      radial-gradient(circle at 78% 12%, rgba(0, 0, 0, 0.16), transparent 24%),
      linear-gradient(180deg, #000000 0%, #000000 46%, #000000 100%);
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
    background: rgba(4, 7, 5, 0.78);
    border-bottom: 1px solid rgba(0, 0, 0, 0.16);
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
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.16);
    border: 1px solid rgba(177, 245, 195, 0.34);
    color: #effcf1;
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
    background: rgba(3, 101, 29, 0.1);
    border: 1px solid rgba(0, 255, 68, 0.24);
    color: #4be16b;
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
    color: #4be16b;
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
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.8);
    border: 1px solid rgba(146, 145, 145, 0.16);
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
    border-radius: 36px;
    overflow: hidden;
    background: transparent;
  `,
  chartCanvas: css`
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    opacity: 0.96;
    mask-image: linear-gradient(180deg, transparent 0%, rgba(0, 0, 0, 0.88) 14%, rgba(0, 0, 0, 0.96) 78%, transparent 100%);
  `,
  chartFadeLeft: css`
    position: absolute;
    inset: 0 auto 0 0;
    width: 10%;
    pointer-events: none;
    background: linear-gradient(90deg, rgba(0, 0, 0, 0.98) 0%, rgba(1, 1, 1, 0.74) 42%, rgba(0, 0, 0, 0) 100%);
  `,
  floatingPanel: css`
    position: absolute;
    right: 24px;
    left: 15px;
    bottom: 0px;
    padding: 22px;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0);
    border: 1px solid rgba(26, 165, 11, 0.14);
    backdrop-filter: blur(5px);
    box-shadow: inset 0 1px 0 rgba(19, 19, 19, 0.03);
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
    background: rgba(4, 4, 4, 0.8);
    border-top: 1px solid rgba(111, 132, 117, 0.08);
    border-bottom: 1px solid rgba(111, 132, 117, 0.08);
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
    background: rgba(0, 0, 0, 0.84);
    border: 1px solid rgba(111, 132, 117, 0.14);
    transition:
      transform 0.2s ease,
      border-color 0.2s ease;

    &:hover {
      transform: translateY(-4px);
      border-color: rgba(177, 245, 195, 0.34);
    }
  `,
  featureIcon: css`
    width: 56px;
    height: 56px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 18px;
    background: rgba(0, 0, 0, 0.12);
    border: 1px solid rgba(30, 241, 86, 0.28);
    color: #4be16b;
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
    background: rgba(7, 12, 9, 0.92);
    border: 1px solid rgba(111, 132, 117, 0.14);
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
    background: rgba(0, 0, 0, 0.92);
    border: 1px solid rgba(177, 245, 195, 0.2);
  `,
  platformCard: css`
    height: 100%;
    overflow: hidden;
    border-radius: 28px;
    background: rgba(0, 0, 0, 0.92);
    border: 1px solid rgba(111, 132, 117, 0.14);
  `,
  platformVisual: css`
    min-height: 220px;
    padding: 24px;
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    background: rgba(10, 12, 14, 0.82);
  `,
  platformMini: css`
    width: 100%;
    padding: 18px;
    border-radius: 22px;
    background: rgba(5, 9, 6, 0.72);
    border: 1px solid rgba(241, 245, 240, 0.08);
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
    background: #4be16b;
    box-shadow: 0 0 14px rgba(20, 20, 20, 0.28);
  `,
  footer: css`
    padding: 72px 0 36px;
    border-top: 1px solid rgba(111, 132, 117, 0.12);
    background: rgba(4, 7, 5, 0.94);
  `,
  footerCopy: css`
    color: ${token.colorTextSecondary} !important;
    line-height: 1.8 !important;
  `,
  footerBottom: css`
    margin-top: 34px;
    padding-top: 24px;
    border-top: 1px solid rgba(111, 132, 117, 0.12);
    color: ${token.colorTextSecondary};
  `,
}));
