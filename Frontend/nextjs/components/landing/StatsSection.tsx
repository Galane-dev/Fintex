"use client";

import Link from "next/link";
import { ArrowRightOutlined, SwapOutlined } from "@ant-design/icons";
import { Button, Card, Flex, Tag, Typography } from "antd";
import { marketSnapshots } from "@/constants/landing";
import { ROUTES } from "@/constants/routes";
import { useStyles } from "./style";

export function StatsSection() {
  const { styles } = useStyles();

  return (
    <section id="markets" className={styles.section}>
      <div className={styles.shell}>
        <div className={styles.sectionHeader}>
          <div className={styles.pill}>
            <SwapOutlined />
            Live markets
          </div>
          <Typography.Title className={styles.sectionTitle}>
            Real-time market data framed for action, not clutter.
          </Typography.Title>
          <Typography.Paragraph className={styles.sectionCopy}>
            Traders can spot movement, verdict bias, and momentum at a glance before drilling deeper into protected analytics pages.
          </Typography.Paragraph>
        </div>

        <div className={styles.marketGrid}>
          {marketSnapshots.map((item) => {
            const positive = item.change.startsWith("+");
            const tagColor = item.verdict === "Buy" ? "green" : item.verdict === "Sell" ? "red" : "gold";

            return (
              <Card key={item.symbol} className={styles.marketCard}>
                <Flex justify="space-between" align="center">
                  <span className={styles.marketSymbol}>{item.symbol}</span>
                  <Tag color={tagColor}>{item.verdict}</Tag>
                </Flex>
                <div className={styles.marketPrice}>{item.price}</div>
                <Typography.Text type={positive ? undefined : "danger"}>
                  {item.change}
                </Typography.Text>
              </Card>
            );
          })}
        </div>

        <div className={styles.ctaPanel}>
          <Flex gap={24} justify="space-between" align="center" wrap="wrap">
            <div>
              <Typography.Title level={3} style={{ margin: 0 }}>
                Move from public market view to protected trading workspace.
              </Typography.Title>
              <Typography.Paragraph style={{ margin: "10px 0 0" }}>
                Sign in to access dashboards, live verdicts, and account-scoped trading surfaces.
              </Typography.Paragraph>
            </div>
            <Link href={ROUTES.signUp}>
              <Button type="primary" size="large" icon={<ArrowRightOutlined />}>
                Create account
              </Button>
            </Link>
          </Flex>
        </div>
      </div>
    </section>
  );
}
