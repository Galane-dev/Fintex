"use client";

import Link from "next/link";
import { LogoutOutlined, ThunderboltFilled } from "@ant-design/icons";
import { Button, Card, Col, List, Row, Space, Statistic, Tag, Typography } from "antd";
import { ROUTES } from "@/constants/routes";
import { withAuth } from "@/hoc/withAuth";
import { useAuth } from "@/hooks/useAuth";
import { useStyles } from "./style";

const protectedCards = [
  { title: "Trend score", value: 54.2, suffix: "/100" },
  { title: "Confidence", value: 87.4, suffix: "/100" },
  { title: "Realtime verdict", value: "Buy bias" },
  { title: "Active watchlists", value: 4 },
];

const dashboardFeed = [
  { symbol: "BTC/USD", verdict: "Buy", note: "Momentum and MACD remain aligned above the session baseline." },
  { symbol: "ETH/USD", verdict: "Hold", note: "Confidence is high, but the short-term move is cooling into resistance." },
  { symbol: "EUR/USD", verdict: "Sell", note: "Rate of change turned negative while volatility expanded." },
];

function DashboardView() {
  const { styles } = useStyles();
  const { signOut, user } = useAuth();

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        <div className={styles.hero}>
          <Space orientation="vertical" size="middle" style={{ width: "100%" }}>
            <Space align="center" wrap style={{ justifyContent: "space-between", width: "100%" }}>
              <div>
                <Typography.Text type="success">
                  <ThunderboltFilled /> Protected workspace
                </Typography.Text>
                <Typography.Title level={1} style={{ margin: "8px 0 12px" }}>
                  Welcome back, {user?.firstName ?? "Trader"}
                </Typography.Title>
                <Typography.Paragraph className={styles.copy}>
                  This page is wrapped in the auth HOC. Unauthenticated users are redirected to the sign-in screen before they can access protected trading content.
                </Typography.Paragraph>
              </div>
              <Space wrap>
                <Link href={ROUTES.home}>
                  <Button>Back to landing</Button>
                </Link>
                <Button type="primary" icon={<LogoutOutlined />} onClick={signOut}>
                  Sign out
                </Button>
              </Space>
            </Space>
          </Space>
        </div>

        <Row gutter={[18, 18]}>
          {protectedCards.map((item) => (
            <Col key={item.title} xs={24} md={12} xl={6}>
              <Card className={styles.card}>
                <Statistic title={item.title} value={item.value} suffix={item.suffix} />
              </Card>
            </Col>
          ))}
        </Row>

        <Row gutter={[18, 18]} style={{ marginTop: 10 }}>
          <Col xs={24} xl={16}>
            <Card className={styles.card} title="Realtime verdict feed">
              <List
                itemLayout="vertical"
                dataSource={dashboardFeed}
                renderItem={(item) => (
                  <List.Item key={item.symbol}>
                    <Space orientation="vertical" size="small">
                      <Space>
                        <Typography.Text strong>{item.symbol}</Typography.Text>
                        <Tag color={item.verdict === "Buy" ? "green" : item.verdict === "Sell" ? "red" : "gold"}>
                          {item.verdict}
                        </Tag>
                      </Space>
                      <Typography.Text type="secondary">{item.note}</Typography.Text>
                    </Space>
                  </List.Item>
                )}
              />
            </Card>
          </Col>
          <Col xs={24} xl={8}>
            <Card className={styles.card} title="Protected access behavior">
              <Space orientation="vertical" size="middle">
                <Typography.Paragraph>
                  `withAuth` checks the auth session after hydration and redirects guests to `/auth/sign-in`.
                </Typography.Paragraph>
                <Typography.Paragraph>
                  Auth pages simulate session creation locally so the UI flow can be designed before backend auth is connected.
                </Typography.Paragraph>
              </Space>
            </Card>
          </Col>
        </Row>
      </div>
    </div>
  );
}

const ProtectedDashboardPage = withAuth(DashboardView);

export default ProtectedDashboardPage;
