"use client";

import { useEffect } from "react";
import { Alert, Button, Card, Flex, Form, Radio, Space, Statistic, Typography } from "antd";
import type { FormInstance } from "antd";
import type { AcademyCourse, AcademyQuizSubmissionResult, AcademyStatus } from "@/types/academy";
import { useStyles } from "./style";

type QuizValues = Record<string, string>;

interface AcademyQuizPanelProps {
  course: AcademyCourse;
  academyStatus: AcademyStatus | null;
  quizResult: AcademyQuizSubmissionResult | null;
  quizError: string | null;
  isSubmitting: boolean;
  isStatusLoading: boolean;
  onSubmit: () => void;
  onBackToLessons: () => void;
  onFormReady: (form: FormInstance<QuizValues>) => void;
}

export const AcademyQuizPanel = ({
  course,
  academyStatus,
  quizResult,
  quizError,
  isSubmitting,
  isStatusLoading,
  onSubmit,
  onBackToLessons,
  onFormReady,
}: AcademyQuizPanelProps) => {
  const { styles } = useStyles();
  const [form] = Form.useForm<QuizValues>();

  useEffect(() => {
    onFormReady(form);
  }, [form, onFormReady]);

  return (
    <Card className={styles.workspaceCard}>
      <div className={styles.stageHeader}>
        <div>
          <Typography.Text className={styles.stageEyebrow}>Final step</Typography.Text>
          <Typography.Title level={3} className={styles.stageTitle}>
            Intro quiz
          </Typography.Title>
          <Typography.Paragraph className={styles.stageCopy}>
            You have completed the lessons. Now score at least {course.requiredScorePercent}% to unlock trade academy.
          </Typography.Paragraph>
        </div>
      </div>

      <div className={styles.quizSummary}>
        <div className={styles.resultCard}>
          <Statistic title="Required score" value={course.requiredScorePercent} suffix="%" />
        </div>
        <div className={styles.resultCard}>
          <Statistic title="Best score so far" value={academyStatus?.bestIntroQuizScore ?? 0} suffix="%" />
        </div>
        <div className={styles.resultCard}>
          <Statistic title="Attempts" value={academyStatus?.introQuizAttemptsCount ?? 0} />
        </div>
      </div>

      {quizResult ? (
        <Alert
          type={quizResult.passed ? "success" : "warning"}
          showIcon
          message={quizResult.headline}
          description={`${quizResult.summary} You scored ${quizResult.scorePercent.toFixed(1)}% (${quizResult.correctAnswers}/${quizResult.totalQuestions}).`}
          style={{ marginBottom: 16 }}
        />
      ) : null}
      {quizError ? <Alert type="error" showIcon message={quizError} style={{ marginBottom: 16 }} /> : null}

      <Form form={form} layout="vertical">
        <Space direction="vertical" size="large" style={{ width: "100%" }}>
          {course.quizQuestions.map((question) => (
            <div key={question.key} className={styles.quizQuestionCard}>
              <Form.Item
                name={question.key}
                label={question.prompt}
                rules={[{ required: true, message: "Choose one answer." }]}
                style={{ marginBottom: 0 }}
              >
                <Radio.Group className={styles.quizRadioGroup}>
                  <Space direction="vertical" size="middle">
                    {question.options.map((option) => (
                      <Radio key={option.key} value={option.key}>
                        {option.label}
                      </Radio>
                    ))}
                  </Space>
                </Radio.Group>
              </Form.Item>
            </div>
          ))}
        </Space>
      </Form>

      <Flex justify="space-between" align="center" gap={16} wrap="wrap" className={styles.lessonFooter}>
        <Button onClick={onBackToLessons}>Review lessons again</Button>
        <Button type="primary" loading={isSubmitting || isStatusLoading} onClick={() => void onSubmit()}>
          Submit quiz
        </Button>
      </Flex>
    </Card>
  );
};
