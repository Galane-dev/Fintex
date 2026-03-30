"use client";

import { Alert, Card, Space, Statistic } from "antd";
import { CheckCircleOutlined, LockOutlined, RiseOutlined } from "@ant-design/icons";
import type { AcademyStatus } from "@/types/academy";
import { useStyles } from "./style";

interface AcademyProgressPanelProps {
  academyStatus: AcademyStatus | null;
  completedLessons: number;
  totalLessons: number;
  quizProgressPercent: number;
  isSavingProgress: boolean;
}

export const AcademyProgressPanel = ({
  academyStatus,
  completedLessons,
  totalLessons,
  quizProgressPercent,
  isSavingProgress,
}: AcademyProgressPanelProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.progressCard}>
      <div className={styles.progressStats}>
        <Statistic title="Best quiz score" value={academyStatus?.bestIntroQuizScore ?? 0} suffix="%" />
        <Statistic title="Attempts" value={academyStatus?.introQuizAttemptsCount ?? 0} />
        <Statistic title="Stage" value={academyStatus?.academyStage ?? "IntroCourse"} />
        <Statistic title="Lessons complete" value={completedLessons} suffix={`/ ${totalLessons}`} />
      </div>
      <Space direction="vertical" size="small" style={{ width: "100%" }}>
        <Alert
          type="success"
          showIcon
          icon={<CheckCircleOutlined />}
          message={`Quiz progress ${quizProgressPercent.toFixed(0)}%`}
          description={`Your best score is ${academyStatus?.bestIntroQuizScore ?? 0}% and you need ${academyStatus?.requiredQuizScorePercent ?? 90}% to unlock trade academy.`}
        />
        <Alert
          type="info"
          showIcon
          icon={<LockOutlined />}
          message="Academy unlock rule"
          description={`You need at least ${academyStatus?.requiredQuizScorePercent ?? 90}% to enter trade academy.`}
        />
        <Alert
          type="success"
          showIcon
          icon={<RiseOutlined />}
          message="Live broker unlock rule"
          description="External brokers stay locked until your academy paper account grows by 75% from its starting balance."
        />
        {isSavingProgress ? <Alert type="info" showIcon message="Saving lesson progress..." /> : null}
      </Space>
    </Card>
  );
};
