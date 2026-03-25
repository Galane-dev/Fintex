"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  page: css`
    min-height: 100vh;
    padding: 32px 0 56px;
  `,
  shell: css`
    width: min(1200px, calc(100vw - 32px));
    margin: 0 auto;
  `,
  hero: css`
    margin-bottom: 28px;
    padding: 28px;
    border-radius: 28px;
    background: linear-gradient(135deg, rgba(52, 245, 197, 0.12), rgba(84, 124, 255, 0.14));
    border: 1px solid rgba(82, 242, 201, 0.26);
  `,
  copy: css`
    color: ${token.colorTextSecondary};
    line-height: 1.8;
    max-width: 780px;
  `,
  card: css`
    height: 100%;
    border-radius: 24px;
    background: rgba(8, 13, 20, 0.92);
    border: 1px solid rgba(118, 154, 198, 0.14);
  `,
}));
