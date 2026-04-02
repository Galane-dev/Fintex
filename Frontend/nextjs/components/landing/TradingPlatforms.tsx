"use client";

import { CheckCircleOutlined, CrownOutlined, RocketOutlined } from "@ant-design/icons";
import { Button, Card, Col, Row, Space, Tag, Typography } from "antd";
import { subscriptionPlans } from "@/constants/landing";
import { useStyles } from "./style";

const planIcons = {
  starter: <RocketOutlined />,
  pro: <CheckCircleOutlined />,
  elite: <CrownOutlined />,
} as const;

export function TradingPlatforms() {
  const { styles } = useStyles();

  return (
    <section id="plans" className={styles.sectionMuted}>
      <div className={styles.shell}>
        <div className={styles.sectionHeader}>
          <div className={styles.pill}>Subscription plans</div>
          <Typography.Title className={styles.sectionTitle}>
            Pick the plan view that fits your trading journey.
          </Typography.Title>
          <Typography.Paragraph className={styles.sectionCopy}>
            This section is a simple preview-only pricing layout for the landing page.
            It is intentionally non-functional and does not connect to billing.
          </Typography.Paragraph>
        </div>

        <Row gutter={[18, 18]}>
          {subscriptionPlans.map((plan) => (
            <Col key={plan.key} xs={24} lg={8}>
              <Card className={styles.platformCard}>
                <div className={styles.platformVisual}>
                  <div className={styles.platformMini}>
                    <Space orientation="vertical" size="middle" style={{ width: "100%" }}>
                      <Typography.Text>{planIcons[plan.key]}</Typography.Text>
                      <Tag color="green">Dummy plan</Tag>
                      <Typography.Title level={2} style={{ margin: 0 }}>
                        {plan.price}
                        <Typography.Text type="secondary" style={{ marginLeft: 4 }}>
                          {plan.cadence}
                        </Typography.Text>
                      </Typography.Title>
                    </Space>
                  </div>
                </div>
                <Typography.Title level={3}>{plan.title}</Typography.Title>
                <Typography.Paragraph>{plan.description}</Typography.Paragraph>
                <div className={styles.bulletList}>
                  {plan.bullets.map((bullet) => (
                    <div key={bullet} className={styles.bulletItem}>
                      <span className={styles.bulletDot} />
                      <span>{bullet}</span>
                    </div>
                  ))}
                </div>
                <Button block style={{ marginTop: 24 }} disabled>
                  Choose plan
                </Button>
              </Card>
            </Col>
          ))}
        </Row>
      </div>
    </section>
  );
}
