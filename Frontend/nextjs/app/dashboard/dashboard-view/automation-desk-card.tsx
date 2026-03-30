"use client";

import { BellOutlined, FlagOutlined, RobotOutlined } from "@ant-design/icons";
import { Button, Card, Space, Tag, Typography } from "antd";
import { useStyles } from "../style";

type AutomationDeskCardProps = {
  alertCount: number;
  automationCount: number;
  goalCount: number;
  onOpenAlerts: () => void;
  onOpenGoals: () => void;
};

export function AutomationDeskCard({
  alertCount,
  automationCount,
  goalCount,
  onOpenAlerts,
  onOpenGoals,
}: AutomationDeskCardProps) {
  const { styles } = useStyles();

  return (
    <Card className={styles.panelCard}>
      <div className={styles.automationDesk}>
        <div className={styles.automationSummary}>
          <div className={styles.automationMetric}>
            <span className={styles.automationLabel}>Price alerts</span>
            <span className={styles.automationValue}>{alertCount}</span>
          </div>
          <div className={styles.automationMetric}>
            <span className={styles.automationLabel}>Auto executions</span>
            <span className={styles.automationValue}>{automationCount}</span>
          </div>
          <div className={styles.automationMetric}>
            <span className={styles.automationLabel}>Goal autopilots</span>
            <span className={styles.automationValue}>{goalCount}</span>
          </div>
        </div>

        <Space wrap>
          <Tag color="blue" icon={<BellOutlined />}>
            Watches BTC triggers
          </Tag>
          <Tag color="purple" icon={<RobotOutlined />}>
            Executes only after rule checks pass
          </Tag>
        </Space>

        <Typography.Paragraph className={styles.automationCopy}>
          Alerts, direct trigger rules, and goal autopilot all rely on the same BTC verdict stack,
          so the automation desk is the fastest way to see what is armed right now.
        </Typography.Paragraph>

        <Space wrap>
          <Button icon={<BellOutlined />} onClick={onOpenAlerts}>
            Open alerts
          </Button>
          <Button icon={<FlagOutlined />} onClick={onOpenGoals}>
            Open goal automation
          </Button>
        </Space>
      </div>
    </Card>
  );
}
