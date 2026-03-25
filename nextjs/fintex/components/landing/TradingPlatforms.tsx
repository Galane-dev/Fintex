"use client";

import { ApiOutlined, DesktopOutlined, MobileOutlined } from "@ant-design/icons";
import { Button, Card, Col, Row, Space, Statistic, Typography } from "antd";
import { platformShowcases } from "@/constants/landing";
import { useStyles } from "./style";

const platformIcons = {
  web: <DesktopOutlined />,
  mobile: <MobileOutlined />,
  api: <ApiOutlined />,
} as const;

export function TradingPlatforms() {
  const { styles } = useStyles();

  return (
    <section id="platforms" className={styles.sectionMuted}>
      <div className={styles.shell}>
        <div className={styles.sectionHeader}>
          <div className={styles.pill}>Multi-platform</div>
          <Typography.Title className={styles.sectionTitle}>
            One trading brand, flexible across web, mobile, and automation.
          </Typography.Title>
          <Typography.Paragraph className={styles.sectionCopy}>
            The same visual system and confidence-driven data model can scale from public marketing pages to secure application flows.
          </Typography.Paragraph>
        </div>

        <Row gutter={[18, 18]}>
          {platformShowcases.map((platform) => (
            <Col key={platform.key} xs={24} lg={8}>
              <Card className={styles.platformCard}>
                <div className={styles.platformVisual} style={{ background: platform.accent }}>
                  <div className={styles.platformMini}>
                    <Space orientation="vertical" size="middle" style={{ width: "100%" }}>
                      <Typography.Text>{platformIcons[platform.key]}</Typography.Text>
                      <Statistic title="Execution speed" value={platform.key === "api" ? "19ms" : platform.key === "mobile" ? "41ms" : "24ms"} />
                      <Statistic title="Session verdict" value={platform.key === "mobile" ? "Hold" : "Buy"} />
                    </Space>
                  </div>
                </div>
                <Typography.Title level={3}>{platform.title}</Typography.Title>
                <Typography.Paragraph>{platform.description}</Typography.Paragraph>
                <div className={styles.bulletList}>
                  {platform.bullets.map((bullet) => (
                    <div key={bullet} className={styles.bulletItem}>
                      <span className={styles.bulletDot} />
                      <span>{bullet}</span>
                    </div>
                  ))}
                </div>
                <Button block style={{ marginTop: 24 }}>
                  Learn more
                </Button>
              </Card>
            </Col>
          ))}
        </Row>
      </div>
    </section>
  );
}
