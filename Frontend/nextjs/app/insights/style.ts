"use client";

import { createStyles } from "antd-style";

export const useInsightsStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    background: #040706;
    padding: 24px 0 56px;
    scroll-padding-top: 140px;
  `,
  shell: css`
    width: min(1480px, calc(100vw - 24px));
    margin: 0 auto;
  `,
  pageLayout: css`
    display: grid;
    grid-template-columns: 220px minmax(0, 1fr);
    gap: 18px;

    @media (max-width: 1180px) {
      grid-template-columns: 1fr;
    }
  `,
  leftNav: css`
    position: sticky;
    top: 12px;
    align-self: start;
    border-radius: 16px;
    padding: 12px;
    background: rgba(6, 11, 8, 0.96);
    border: 1px solid rgba(137, 199, 153, 0.2);

    @media (max-width: 1180px) {
      display: none;
    }
  `,
  leftNavTitle: css`
    color: #9de3ad;
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    margin-bottom: 10px;
    padding: 0 6px;
  `,
  leftNavList: css`
    display: grid;
    gap: 6px;
  `,
  leftNavLink: css`
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 8px 10px;
    border-radius: 10px;
    color: ${token.colorTextSecondary};
    text-decoration: none;
    font-size: 13px;
    border: 1px solid transparent;
    transition: all 0.15s ease;

    &:hover {
      color: ${token.colorText};
      background: rgba(121, 216, 143, 0.1);
      border-color: rgba(121, 216, 143, 0.28);
    }
  `,
  mainPane: css`
    min-width: 0;
  `,
  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 20px;
    flex-wrap: wrap;
    position: sticky;
    top: 10px;
    z-index: 30;
    padding: 12px 14px;
    border-radius: 16px;
    background: rgba(5, 10, 7, 0.94);
    border: 1px solid rgba(137, 199, 153, 0.16);
    backdrop-filter: blur(8px);
  `,
  titleWrap: css`
    display: grid;
    gap: 6px;
  `,
  eyebrow: css`
    color: #79d88f;
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
    border-color: rgba(121, 216, 143, 0.26) !important;
    background: #09150d !important;
    box-shadow: 0 10px 24px rgba(0, 0, 0, 0.28);

    &:hover,
    &:focus {
      color: #9bf2b1 !important;
      border-color: rgba(121, 216, 143, 0.42) !important;
      background: #0d1e13 !important;
      transform: translateY(-1px);
    }
  `,
  cardsGrid: css`
    display: grid;
    grid-template-columns: repeat(5, minmax(0, 1fr));
    gap: 14px;
    margin-bottom: 20px;

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
    padding: 18px 18px;
    border-radius: 20px;
    background: rgba(7, 13, 9, 0.96);
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
    color: #8ce3a2;
    background: rgba(121, 216, 143, 0.14);
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
    font-size: 26px;
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
    margin-top: 18px;

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
    background: rgba(8, 14, 10, 0.98) !important;
    border: 1px solid rgba(255, 255, 255, 0.05) !important;
    box-shadow:
      0 20px 36px rgba(0, 0, 0, 0.24),
      inset 0 1px 0 rgba(255, 255, 255, 0.03) !important;
  `,
  heroPanel: css`
    padding: 16px 18px;
    border-radius: 20px;
    background: rgba(7, 14, 10, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    flex-wrap: wrap;
  `,
  heroEyebrow: css`
    color: #8ce3a2;
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
    background: rgba(8, 14, 10, 0.96);
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
    color: #8ce3a2;
    font-size: 18px;
    background: rgba(121, 216, 143, 0.14);
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
    background: rgba(6, 11, 8, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.05);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 14px;
    flex-wrap: wrap;
    position: sticky;
    top: 124px;
    z-index: 25;

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
    background: rgba(255, 255, 255, 0.01);
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
    background: linear-gradient(90deg, #4be16b, #89d39b);
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
    background: #89d39b;
    box-shadow: 0 0 0 5px rgba(121, 216, 143, 0.16);
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
  visualHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
    flex-wrap: wrap;
    margin-bottom: 14px;
  `,
  visualGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 14px;

    @media (max-width: 1100px) {
      grid-template-columns: 1fr;
    }
  `,
  visualPanel: css`
    border-radius: 18px;
    padding: 14px;
    background: rgba(5, 12, 8, 0.96);
    border: 1px solid rgba(255, 255, 255, 0.06);
    min-height: 210px;
  `,
  visualPanelHead: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 12px;
  `,
  visualPanelTitle: css`
    display: inline-flex;
    align-items: center;
    gap: 8px;
    color: ${token.colorText};
    font-size: 13px;
    font-weight: 600;
  `,
  donutWrap: css`
    display: grid;
    grid-template-columns: 150px minmax(0, 1fr);
    align-items: center;
    gap: 14px;

    @media (max-width: 520px) {
      grid-template-columns: 1fr;
      justify-items: center;
    }
  `,
  donutSvg: css`
    width: 150px;
    height: 150px;
  `,
  donutShell: css`
    position: relative;
    width: 150px;
    height: 150px;
  `,
  donutCenter: css`
    position: absolute;
    width: 150px;
    height: 150px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    pointer-events: none;
  `,
  donutValue: css`
    color: ${token.colorText};
    font-size: 28px;
    font-weight: 700;
    line-height: 1.1;
  `,
  donutLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  visualLegend: css`
    display: grid;
    gap: 8px;
    width: 100%;
  `,
  visualLegendRow: css`
    display: grid;
    grid-template-columns: 10px minmax(0, 1fr) auto;
    gap: 8px;
    align-items: center;
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  visualSwatch: css`
    width: 10px;
    height: 10px;
    border-radius: 999px;
  `,
  verticalBars: css`
    min-height: 180px;
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 12px;
    align-items: end;
  `,
  verticalBarCol: css`
    display: grid;
    gap: 8px;
    justify-items: center;
  `,
  verticalBarTrack: css`
    width: 100%;
    height: 120px;
    background: rgba(255, 255, 255, 0.06);
    border-radius: 999px;
    overflow: hidden;
    display: flex;
    align-items: flex-end;
  `,
  verticalBarFill: css`
    width: 100%;
    border-radius: inherit;
    background: linear-gradient(180deg, #78e495, #4dbf6f);
  `,
  verticalBarLabel: css`
    color: ${token.colorText};
    font-size: 12px;
    text-align: center;
  `,
  verticalBarValue: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,
  treemapGrid: css`
    display: grid;
    grid-template-columns: repeat(12, minmax(0, 1fr));
    grid-auto-rows: 22px;
    gap: 10px;
  `,
  treemapTile: css`
    border-radius: 12px;
    border: 1px solid rgba(121, 216, 143, 0.34);
    background: rgba(8, 16, 11, 0.9);
    padding: 10px 10px 8px;
    display: grid;
    align-content: start;
    gap: 6px;
    min-width: 0;
    overflow: hidden;
  `,
  treemapTop: css`
    display: block;
    min-width: 0;
  `,
  treemapScoreWrap: css`
    display: inline-flex;
    width: fit-content;
  `,
  treemapName: css`
    color: ${token.colorText};
    font-size: 12px;
    font-weight: 600;
    line-height: 1.4;
    min-width: 0;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
  `,
  treemapMeta: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  `,
  heatmapWrap: css`
    display: grid;
    gap: 8px;
  `,
  heatmapHeader: css`
    display: grid;
    grid-template-columns: 42px repeat(6, minmax(0, 1fr));
    gap: 6px;
  `,
  heatmapBucket: css`
    color: ${token.colorTextSecondary};
    font-size: 10px;
    text-align: center;
  `,
  heatmapRow: css`
    display: grid;
    grid-template-columns: 42px repeat(6, minmax(0, 1fr));
    gap: 6px;
    align-items: center;
  `,
  heatmapDay: css`
    color: ${token.colorTextSecondary};
    font-size: 11px;
  `,
  heatCell: css`
    height: 18px;
    border-radius: 6px;
    border: 1px solid rgba(255, 255, 255, 0.04);
  `,
  sparkWrap: css`
    height: 168px;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    padding: 8px;
  `,
  sparkSvg: css`
    width: 100%;
    height: 100%;
  `,
  stickySidebar: css`
    position: sticky;
    top: 214px;
    align-self: start;

    @media (max-width: 1080px) {
      position: static;
      top: auto;
    }
  `,
}));
