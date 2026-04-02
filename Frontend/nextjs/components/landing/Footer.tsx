"use client";

import Link from "next/link";
import { Col, Row, Space, Typography } from "antd";
import { footerGroups } from "@/constants/landing";
import { useStyles } from "./style";

export function Footer() {
  const { styles } = useStyles();

  return (
    <footer id="security" className={styles.footer}>
      <div className={styles.shell}>
        <Row gutter={[32, 32]}>
          <Col xs={24} lg={8}>
            <Space orientation="vertical" size="middle">
              <div className={styles.brand}>
                <span className={styles.brandBadge}>F</span>
                <Typography.Text className={styles.brandText}>FinteX</Typography.Text>
              </div>
              <Typography.Paragraph className={styles.footerCopy}>
                FinteX is designed for traders who want decisive information, disciplined access control,
                and a premium experience from the first visit through the authenticated workspace.
              </Typography.Paragraph>
            </Space>
          </Col>

          {footerGroups.map((group) => (
            <Col key={group.title} xs={24} md={12} lg={8}>
              <Typography.Title level={5}>{group.title}</Typography.Title>
              <Space orientation="vertical" size="small">
                {group.links.map((link) => (
                  <Link key={link.label} href={link.href}>
                    <Typography.Text type="secondary">{link.label}</Typography.Text>
                  </Link>
                ))}
              </Space>
            </Col>
          ))}
        </Row>

        <div className={styles.footerBottom}>
          <Row gutter={[18, 12]} justify="space-between">
            <Col>
              <Typography.Text type="secondary">Copyright 2026 FinteX. All rights reserved.</Typography.Text>
            </Col>
            <Col>
              <Typography.Text type="secondary">
                Risk warning: trading and investing involve risk. Only commit capital you can afford to lose.
              </Typography.Text>
            </Col>
          </Row>
        </div>
      </div>
    </footer>
  );
}
