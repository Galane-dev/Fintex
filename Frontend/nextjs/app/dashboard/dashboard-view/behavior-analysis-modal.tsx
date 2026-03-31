"use client";

import { Alert, Skeleton, Space, Tag, Typography } from "antd";
import { DashboardDrawerShell } from "@/components/dashboard/dashboard-drawer-shell";
import type { UserProfile } from "@/types/user-profile";
import { formatTime } from "@/utils/market-data";
import { useStyles } from "../style";

interface BehaviorAnalysisModalProps {
  isOpen: boolean;
  isLoading: boolean;
  error: string | null;
  profile: UserProfile | null;
  onClose: () => void;
}

export function BehaviorAnalysisModal({
  isOpen,
  isLoading,
  error,
  profile,
  onClose,
}: BehaviorAnalysisModalProps) {
  const { styles } = useStyles();

  const content = isLoading ? (
    <Skeleton active paragraph={{ rows: 7 }} />
  ) : error ? (
    <Alert type="warning" showIcon title={error} />
  ) : profile ? (
    <div className={styles.behaviorPanel}>
      <div className={styles.behaviorHero}>
        <div>
          <div className={styles.behaviorLabel}>Behavioral risk score</div>
          <div className={styles.behaviorValue}>{profile.behavioralRiskScore.toFixed(1)}</div>
        </div>
        <Space wrap>
          <Tag color={profile.isAiInsightsEnabled ? "green" : "default"}>
            {profile.isAiInsightsEnabled ? "AI insights on" : "AI insights off"}
          </Tag>
          <Tag color="blue">{profile.preferredBaseCurrency || "USD"}</Tag>
          <Tag color="purple">
            Last analysis {profile.lastBehavioralAnalysisTime ? formatTime(profile.lastBehavioralAnalysisTime) : "Pending"}
          </Tag>
        </Space>
      </div>

      <Section title="Behavior summary">
        {profile.behavioralSummary || "Your behavioral summary has not been generated yet. As you place more simulator trades, this panel will become more useful."}
      </Section>

      <div className={styles.behaviorGrid}>
        <SummaryCard label="Favorite symbols" value={profile.favoriteSymbols || "BTCUSDT"} />
        <SummaryCard label="Risk tolerance" value={profile.riskTolerance.toFixed(2)} />
        <SummaryCard label="AI provider" value={profile.lastAiProvider || "Not set"} />
        <SummaryCard label="AI model" value={profile.lastAiModel || "Not set"} />
      </div>

      <Section title="Strategy notes">
        {profile.strategyNotes || "No strategy notes saved yet."}
      </Section>
    </div>
  ) : (
    <Alert type="info" showIcon title="Behavior analysis is not available yet." />
  );

  return (
    <DashboardDrawerShell open={isOpen} onClose={onClose} title="My behavior analysis" width={720}>
      {content}
    </DashboardDrawerShell>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  const { styles } = useStyles();

  return (
    <div className={styles.behaviorBlock}>
      <div className={styles.behaviorSectionTitle}>{title}</div>
      <Typography.Paragraph className={styles.signalCopy}>{children}</Typography.Paragraph>
    </div>
  );
}

function SummaryCard({ label, value }: { label: string; value: string }) {
  const { styles } = useStyles();

  return (
    <div className={styles.summaryCard}>
      <span className={styles.summaryLabel}>{label}</span>
      <span className={styles.summaryValue}>{value}</span>
    </div>
  );
}
