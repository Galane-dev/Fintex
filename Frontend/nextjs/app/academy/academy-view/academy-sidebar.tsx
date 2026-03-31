"use client";

import { Card, Progress, Tag, Typography } from "antd";
import { FileDoneOutlined, LockOutlined } from "@ant-design/icons";
import type { AcademyLesson } from "@/types/academy";
import { useStyles } from "./style";

interface AcademySidebarProps {
  lessons: AcademyLesson[];
  currentLessonIndex: number;
  completedLessonKeys: string[];
  isQuizUnlocked: boolean;
}

export const AcademySidebar = ({
  lessons,
  currentLessonIndex,
  completedLessonKeys,
  isQuizUnlocked,
}: AcademySidebarProps) => {
  const { styles } = useStyles();
  const completedCount = completedLessonKeys.length;
  const lessonProgress = lessons.length === 0 ? 0 : Math.round((completedCount / lessons.length) * 100);
  const currentLesson = lessons[currentLessonIndex];

  return (
    <Card className={styles.sidebarCard}>
      <div className={styles.sidebarHeader}>
        <Typography.Title level={4} className={styles.sidebarTitle}>
          Learning path
        </Typography.Title>
        <Typography.Paragraph className={styles.sidebarCopy}>
          Move through the lesson sequence first. The quiz appears after the lesson track is complete.
        </Typography.Paragraph>
        <Progress percent={lessonProgress} strokeColor="#1677ff" />
      </div>

      <div className={styles.sidebarSnapshot}>
        <Typography.Text className={styles.sidebarSnapshotLabel}>Current lesson</Typography.Text>
        <Typography.Text className={styles.sidebarSnapshotValue}>
          {currentLesson ? `Lesson ${currentLessonIndex + 1}` : "Waiting"}
        </Typography.Text>
      </div>

      <div className={styles.quizUnlockBox}>
        <div className={styles.quizUnlockTop}>
          <Typography.Text strong>Quiz unlock</Typography.Text>
          {isQuizUnlocked ? <Tag color="green" icon={<FileDoneOutlined />}>Ready</Tag> : <Tag icon={<LockOutlined />}>Locked</Tag>}
        </div>
        <Typography.Paragraph className={styles.quizUnlockCopy}>
          Finish all lessons to open the quiz. If you score below 90%, you retake until you pass.
        </Typography.Paragraph>
      </div>
    </Card>
  );
};
