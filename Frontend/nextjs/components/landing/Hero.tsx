"use client";

import Link from "next/link";
import { ArrowRightOutlined, RiseOutlined } from "@ant-design/icons";
import { Button, Space, Tag, Typography } from "antd";
import { heroMetrics } from "@/constants/landing";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import { AnimatedChart } from "./AnimatedChart";
import { useStyles } from "./style";

export function Hero() {
  const { styles } = useStyles();
  const { isAuthenticated } = useAuth();

  return (
    <section className={styles.hero}>
      <div className={styles.shell}>
        <div className={styles.heroInner}>
          <div className={styles.heroText}>
            <div className={styles.pill}>
              <RiseOutlined />
              Trade smarter, not harder
            </div>

            <Typography.Title className={styles.heroTitle}>
              Invest with <span className={styles.heroAccent}>live intelligence</span>
              <br />
              and decisive execution.
            </Typography.Title>

            <Typography.Paragraph className={styles.heroCopy}>
              FinteX brings real-time market calculations, premium multi-asset workflows, and
              conviction-driven trading tools into one dark, focused platform built for fast-moving markets.
            </Typography.Paragraph>

            <div className={styles.heroActions}>
              <Link href={isAuthenticated ? ROUTES.dashboard : ROUTES.signUp}>
                <Button type="primary" size="large" icon={<ArrowRightOutlined />}>
                  {isAuthenticated ? "Open dashboard" : "Create a free account"}
                </Button>
              </Link>
              <a href="#features">
                <Button size="large">Explore platform</Button>
              </a>
            </div>

            <div className={styles.metrics}>
              {heroMetrics.map((metric) => (
                <div key={metric.label} className={styles.metricCard}>
                  <div className={styles.metricValue}>{metric.value}</div>
                  <div className={styles.metricLabel}>{metric.label}</div>
                </div>
              ))}
            </div>
          </div>

          <div className={styles.heroPanel}>
            <AnimatedChart />
            <div className={styles.chartFadeLeft} />
            <div className={styles.floatingPanel}>
              <Tag color="green">Market verdict: Buy bias</Tag>
              <div className={styles.floatingValue}>87.4</div>
              <div className={styles.floatingLabel}>Confidence score</div>
              <Typography.Paragraph className={styles.floatingSubText}>
                Real-time calculations like EMA, RSI, MACD, momentum, and trend score surface
                strong entries without overwhelming the trader.
              </Typography.Paragraph>
              <Space wrap>
                <Tag color="green">MACD +1.42</Tag>
                <Tag color="gold">RSI 61.8</Tag>
                <Tag color="lime">Trend +54</Tag>
              </Space>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
