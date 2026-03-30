"use client";

import { Card, Col, Row, Typography } from "antd";
import {
  GlobalOutlined,
  LineChartOutlined,
  LockOutlined,
  RobotOutlined,
  SafetyCertificateOutlined,
  ThunderboltFilled,
} from "@ant-design/icons";
import { landingFeatures } from "@/constants/landing";
import { useStyles } from "./style";

const iconMap = {
  FlashFilled: <ThunderboltFilled />,
  LineChartOutlined: <LineChartOutlined />,
  SafetyCertificateOutlined: <SafetyCertificateOutlined />,
  GlobalOutlined: <GlobalOutlined />,
  RobotOutlined: <RobotOutlined />,
  LockOutlined: <LockOutlined />,
} as const;

export function Features() {
  const { styles } = useStyles();

  return (
    <section id="features" className={styles.sectionMuted}>
      <div className={styles.shell}>
        <div className={styles.sectionHeader}>
          <div className={styles.pill}>Platform features</div>
          <Typography.Title className={styles.sectionTitle}>
            Built for traders who want speed, clarity, and confidence.
          </Typography.Title>
          <Typography.Paragraph className={styles.sectionCopy}>
            The landing experience mirrors the trading product: bold data surfaces, clear hierarchy,
            and sharp access to the tools that matter in real time.
          </Typography.Paragraph>
        </div>

        <Row gutter={[18, 18]}>
          {landingFeatures.map((feature) => (
            <Col key={feature.key} xs={24} md={12} xl={8}>
              <Card className={styles.featureCard}>
                <span className={styles.featureIcon}>{iconMap[feature.icon]}</span>
                <Typography.Title level={3} className={styles.featureTitle}>
                  {feature.title}
                </Typography.Title>
                <Typography.Paragraph className={styles.featureCopy}>
                  {feature.description}
                </Typography.Paragraph>
              </Card>
            </Col>
          ))}
        </Row>
      </div>
    </section>
  );
}
