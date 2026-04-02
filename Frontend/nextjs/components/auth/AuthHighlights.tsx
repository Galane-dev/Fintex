"use client";

import Link from "next/link";
import { CheckCircleFilled, ThunderboltFilled } from "@ant-design/icons";
import { Space, Typography } from "antd";
import { ROUTES } from "@/constants/routes";
import { useStyles } from "./style";

const highlights = [
  "Realtime market context and confidence scoring",
  "Protected dashboards with role-safe access",
  "A focused interface tuned for active decision making",
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
              <Typography.Text className={styles.overline}>
                <ThunderboltFilled /> Premium access
              </Typography.Text>
              <Typography.Title className={styles.highlightTitle}>
                Authenticate once and step into a workspace built for signal clarity.
              </Typography.Title>
              <Typography.Paragraph className={styles.copy}>
                Fintex authentication uses the same black-and-green brand language as the core platform, so public onboarding and secured trading feel like one seamless product.
              </Typography.Paragraph>
            </div>

            <Space orientation="vertical" size="middle">
              {highlights.map((item) => (
                <Space key={item} align="start">
                  <CheckCircleFilled style={{ color: "#73e996", marginTop: 4 }} />
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
