"use client";

import { Card, Empty, Progress, Tag, Typography } from "antd";
import { formatTime } from "@/utils/market-data";
import type { UserProfile } from "@/types/user-profile";
import { useInsightsStyles } from "../style";

export function BehaviorSummaryCard({
  profile,
}: {
  profile: UserProfile | null;
}) {
  const { styles } = useInsightsStyles();

  return (
    <Card className={styles.panel}>
      <Typography.Title level={4} className={styles.panelTitle}>
        Behavior analysis
      </Typography.Title>

      {!profile ? (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Behavior analysis has not loaded yet." />
      ) : (
        <div className={styles.list}>
          <div className={styles.listItem}>
            <div className={styles.itemHeader}>
              <div className={styles.itemTitle}>Behavioral risk</div>
              <Tag color={profile.behavioralRiskScore <= 35 ? "green" : profile.behavioralRiskScore >= 70 ? "red" : "gold"}>
                {profile.behavioralRiskScore.toFixed(1)}
              </Tag>
            </div>
            <Progress
              percent={Math.max(0, Math.min(profile.behavioralRiskScore, 100))}
              showInfo={false}
              strokeColor={profile.behavioralRiskScore <= 35 ? "#9bf2b1" : profile.behavioralRiskScore >= 70 ? "#ff7875" : "#ffd666"}
              trailColor="rgba(255,255,255,0.08)"
            />
            <div className={styles.itemCopy}>{profile.behavioralSummary || "No behavioral summary is available yet."}</div>
          </div>

          <div className={styles.listItem}>
            <div className={styles.itemTitle}>AI profile context</div>
            <div className={styles.statRow}>
              <div className={styles.statPill}>
                <span className={styles.statLabel}>Tolerance</span>
                <span className={styles.statValue}>{profile.riskTolerance.toFixed(1)}</span>
              </div>
              <div className={styles.statPill}>
                <span className={styles.statLabel}>Provider</span>
                <span className={styles.statValue}>{profile.lastAiProvider || "Not set"}</span>
              </div>
            </div>
            <div className={styles.itemCopy}>
              Last analyzed {profile.lastBehavioralAnalysisTime ? formatTime(profile.lastBehavioralAnalysisTime) : "Pending"}.
              {profile.lastAiModel ? ` Model ${profile.lastAiModel}.` : ""}
              {profile.strategyNotes ? ` Strategy notes: ${profile.strategyNotes}` : ""}
            </div>
          </div>
        </div>
      )}
    </Card>
  );
}
