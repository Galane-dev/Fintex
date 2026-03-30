"use client";

import { Button, Card, Space, Typography } from "antd";
import { ArrowLeftOutlined, ArrowRightOutlined, ReadOutlined } from "@ant-design/icons";
import type { AcademyLesson } from "@/types/academy";
import { useStyles } from "./style";

interface AcademyLessonsPanelProps {
  lesson: AcademyLesson | null;
  lessonIndex: number;
  totalLessons: number;
  completedLessons: number;
  onPrevious: (() => void) | null;
  onContinue: () => void;
}

const formatLessonParagraphs = (contentMarkdown: string) =>
  contentMarkdown
    .split("\n")
    .map((line) => line.replace(/^- /, "").trim())
    .filter(Boolean);

export const AcademyLessonsPanel = ({
  lesson,
  lessonIndex,
  totalLessons,
  completedLessons,
  onPrevious,
  onContinue,
}: AcademyLessonsPanelProps) => {
  const { styles } = useStyles();

  if (!lesson) {
    return null;
  }

  const isLastLesson = lessonIndex === totalLessons - 1;

  return (
    <Card className={styles.workspaceCard}>
      <div className={styles.stageHeader}>
        <div>
          <Typography.Text className={styles.stageEyebrow}>Lesson {lessonIndex + 1} of {totalLessons}</Typography.Text>
          <Typography.Title level={3} className={styles.stageTitle}>
            {lesson.title}
          </Typography.Title>
          <Typography.Paragraph className={styles.stageCopy}>
            {lesson.summary}
          </Typography.Paragraph>
        </div>
        <div className={styles.stageBadge}>
          <ReadOutlined />
          <span>{completedLessons} completed</span>
        </div>
      </div>

      <div className={styles.lessonBody}>
        {formatLessonParagraphs(lesson.contentMarkdown).map((paragraph) => (
          <Typography.Paragraph key={paragraph} className={styles.lessonParagraph}>
            {paragraph}
          </Typography.Paragraph>
        ))}
      </div>

      <div className={styles.lessonFooter}>
        <Space direction="vertical" size="small">
          <Typography.Text type="secondary">
            Work through the material in order. The quiz only unlocks after you finish the lesson sequence.
          </Typography.Text>
        </Space>
        <Space wrap>
          {onPrevious ? (
            <Button icon={<ArrowLeftOutlined />} onClick={onPrevious}>
              Previous lesson
            </Button>
          ) : null}
          <Button type="primary" icon={<ArrowRightOutlined />} onClick={onContinue}>
            {isLastLesson ? "Finish lessons and open quiz" : "Continue to next lesson"}
          </Button>
        </Space>
      </div>
    </Card>
  );
};
