"use client";

import Link from "next/link";
import { CheckCircleFilled, ThunderboltFilled } from "@ant-design/icons";
import { Space, Typography } from "antd";
import { ROUTES } from "@/constants/routes";
import { useStyles } from "./style";

const highlights = [
  "Realtime trend and confidence calculations",
  "Protected dashboards with auth-first access",
  "A premium dark interface designed for focus",
];

export function AuthHighlights() {
  const { styles } = useStyles();

  return (
    <div className={styles.panel}>
      <div className={styles.panelShell}>
        <Link href={ROUTES.home} className={styles.brand}>
          <span className={styles.brandBadge}>F</span>
          <Typography.Title level={3} style={{ margin: 0 }}>
            FinteX
          </Typography.Title>
        </Link>

        <div className={styles.miniCard}>
          <Space orientation="vertical" size="large">
            <div>
              <Typography.Text type="success">
                <ThunderboltFilled /> Premium access
              </Typography.Text>
              <Typography.Title className={styles.title}>
                Enter a trading workspace that stays sharp under pressure.
              </Typography.Title>
              <Typography.Paragraph className={styles.copy}>
                The auth flow uses the same visual language as the landing page, so the public product story and secure app experience feel like one system.
              </Typography.Paragraph>
            </div>

            <Space orientation="vertical" size="middle">
              {highlights.map((item) => (
                <Space key={item} align="start">
                  <CheckCircleFilled style={{ color: "#34f5c5", marginTop: 4 }} />
                  <Typography.Text>{item}</Typography.Text>
                </Space>
              ))}
            </Space>
          </Space>
        </div>
      </div>
    </div>
  );
}
