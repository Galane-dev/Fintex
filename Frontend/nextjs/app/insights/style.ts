"use client";

import { createStyles } from "antd-style";

export const useInsightsStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    background:
      radial-gradient(circle at top right, rgba(155, 242, 177, 0.08), transparent 20%),
      #020303;
    padding: 24px 0 40px;
  `,
  shell: css`
    width: min(1480px, calc(100vw - 24px));
    margin: 0 auto;
  `,
  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 20px;
    flex-wrap: wrap;
  `,
  titleWrap: css`
    display: grid;
    gap: 6px;
  `,
  eyebrow: css`
    color: #9bf2b1;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.12em;
    font-weight: 600;
  `,
  title: css`
    margin: 0 !important;
    color: ${token.colorText} !important;
    font-size: 30px !important;
    letter-spacing: -0.04em;
  `,
  subtitle: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    max-width: 620px;
  `,
  heroGrid: css`
    display: grid;
    grid-template-columns: minmax(0, 1.45fr) minmax(260px, 0.75fr);
    gap: 16px;
    margin-bottom: 16px;

    @media (max-width: 980px) {
      grid-template-columns: 1fr;
    }
  `,
  actions: css`
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
    justify-content: flex-end;
  `,
  brandHomeButton: css`
    min-width: 36px !important;
    width: 36px;
    height: 36px;
    padding: 0 !important;
    border-radius: 12px !important;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
    font-size: 15px;
    letter-spacing: -0.08em;
    color: #eaffef !important;
    border-color: rgba(155, 242, 177, 0.2) !important;
    background:
      radial-gradient(circle at 28% 24%, rgba(155, 242, 177, 0.22), transparent 42%),
      linear-gradient(180deg, rgba(18, 26, 21, 0.98), rgba(7, 10, 8, 0.98)) !important;
    box-shadow:
      0 10px 24px rgba(0, 0, 0, 0.28),
      inset 0 1px 0 rgba(255, 255, 255, 0.04);
    text-shadow: 0 0 16px rgba(155, 242, 177, 0.16);
    overflow: hidden;
    position: relative;

    &::after {
      content: "";
      position: absolute;
      inset: 0;
      border-radius: inherit;
      background: linear-gradient(135deg, rgba(255, 255, 255, 0.08), transparent 55%);
      pointer-events: none;
    }

    &:hover,
    &:focus {
      color: #9bf2b1 !important;
      border-color: rgba(155, 242, 177, 0.38) !important;
      background:
        radial-gradient(circle at 28% 24%, rgba(155, 242, 177, 0.28), transparent 44%),
        linear-gradient(180deg, rgba(21, 31, 25, 0.98), rgba(8, 12, 9, 0.98)) !important;
      transform: translateY(-1px);
    }
  `,
  cardsGrid: css`
    display: grid;
    grid-template-columns: repeat(5, minmax(0, 1fr));
    gap: 12px;
    margin-bottom: 18px;

    @media (max-width: 1280px) {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }

    @media (max-width: 860px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (max-width: 560px) {
      grid-template-columns: 1fr;
    }
  `,
  overviewCard: css`
    padding: 16px 18px;
    border-radius: 20px;
    background:
      radial-gradient(circle at top right, rgba(155, 242, 177, 0.1), transparent 35%),
      rgba(7, 8, 9, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 10px;
  `,
  overviewTopRow: css`
    display: flex;
    align-items: center;
    gap: 10px;
  `,
  overviewIcon: css`
    width: 34px;
    height: 34px;
    border-radius: 12px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    color: #9bf2b1;
    background: rgba(155, 242, 177, 0.12);
    font-size: 16px;
  `,
  overviewLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  overviewValue: css`
    color: ${token.colorText};
    font-size: 28px;
    font-weight: 600;
    line-height: 1.1;
  `,
  overviewBottomRow: css`
    display: grid;
    gap: 6px;
  `,
  overviewNote: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  overviewProgress: css`
    .ant-progress-outer {
      width: 100%;
    }
  `,
  layout: css`
    display: grid;
    grid-template-columns: minmax(0, 1.25fr) minmax(340px, 0.85fr);
    gap: 18px;

    @media (max-width: 1080px) {
      grid-template-columns: 1fr;
    }
  `,
  column: css`
    display: grid;
    gap: 18px;
    align-content: start;
  `,
  panel: css`
    border-radius: 24px !important;
    background:
      linear-gradient(180deg, rgba(10, 13, 11, 0.98), rgba(6, 8, 7, 0.98)) !important;
    border: 1px solid rgba(255, 255, 255, 0.05) !important;
    box-shadow: none !important;
  `,
  heroPanel: css`
    padding: 16px 18px;
    border-radius: 20px;
    background:
      radial-gradient(circle at top right, rgba(155, 242, 177, 0.14), transparent 34%),
      rgba(8, 11, 9, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    flex-wrap: wrap;
  `,
  heroEyebrow: css`
    color: #9bf2b1;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.12em;
    font-weight: 600;
    margin-bottom: 6px;
  `,
  heroTitle: css`
    margin: 0 !important;
    color: ${token.colorText} !important;
    font-size: 20px !important;
  `,
  spotlightCard: css`
    padding: 18px;
    border-radius: 20px;
    background:
      linear-gradient(180deg, rgba(18, 23, 20, 0.98), rgba(8, 11, 9, 0.98));
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: flex;
    align-items: flex-start;
    gap: 14px;
  `,
  spotlightIcon: css`
    width: 42px;
    height: 42px;
    border-radius: 14px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    color: #9bf2b1;
    font-size: 18px;
    background: rgba(155, 242, 177, 0.12);
    flex-shrink: 0;
  `,
  spotlightBody: css`
    display: grid;
    gap: 6px;
  `,
  spotlightLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  spotlightValue: css`
    color: ${token.colorText};
    font-size: 20px;
    font-weight: 600;
    letter-spacing: -0.03em;
  `,
  spotlightNote: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
    line-height: 1.7;
  `,
  filterBar: css`
    margin-bottom: 18px;
    padding: 14px 16px;
    border-radius: 18px;
    background: rgba(8, 11, 9, 0.94);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 14px;
    flex-wrap: wrap;

    .ant-segmented {
      background: rgba(255, 255, 255, 0.03);
    }
  `,
  filterGroup: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,
  filterLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
    font-weight: 600;
  `,
  panelTitle: css`
    color: ${token.colorText};
    font-size: 16px;
    font-weight: 600;
  `,
  metaRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px 14px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
    margin-bottom: 16px;
  `,
  chartWrap: css`
    height: 240px;
    border-radius: 18px;
    background:
      linear-gradient(180deg, rgba(155, 242, 177, 0.04), rgba(255, 255, 255, 0.01));
    border: 1px solid rgba(255, 255, 255, 0.05);
    padding: 14px;
    margin-bottom: 14px;
  `,
  barList: css`
    display: grid;
    gap: 12px;
  `,
  row: css`
    display: grid;
    gap: 6px;
  `,
  rowLabel: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    color: ${token.colorText};
    font-size: 13px;
    font-weight: 500;
  `,
  track: css`
    height: 10px;
    border-radius: 999px;
    overflow: hidden;
    background: rgba(255, 255, 255, 0.06);
  `,
  fill: css`
    height: 100%;
    border-radius: inherit;
    background: linear-gradient(90deg, #4be16b, #9bf2b1);
  `,
  list: css`
    display: grid;
    gap: 12px;
  `,
  listItem: css`
    padding: 14px 16px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.025);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 8px;
  `,
  timelineItem: css`
    position: relative;
    padding: 14px 16px 14px 40px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.025);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 8px;
  `,
  timelineDot: css`
    position: absolute;
    top: 18px;
    left: 16px;
    width: 10px;
    height: 10px;
    border-radius: 999px;
    background: #9bf2b1;
    box-shadow: 0 0 0 5px rgba(155, 242, 177, 0.14);
  `,
  itemHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 10px;
  `,
  itemTitle: css`
    color: ${token.colorText};
    font-size: 14px;
    font-weight: 600;
  `,
  itemCopy: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
    line-height: 1.7;
  `,
  itemMeta: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  statRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  `,
  statPill: css`
    min-width: 110px;
    padding: 10px 12px;
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: grid;
    gap: 4px;
  `,
  statLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 10px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  statValue: css`
    color: ${token.colorText};
    font-size: 14px;
    font-weight: 600;
  `,
}));
