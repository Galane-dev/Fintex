"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { Card, Empty, Result, Skeleton } from "antd";
import type { FormInstance } from "antd";
import type { AcademyQuizAnswerInput } from "@/types/academy";
import { ROUTES } from "@/constants/routes";
import { getAcademyCourse, saveAcademyLessonProgress, submitAcademyQuiz } from "@/utils/academy-api";
import { useAcademyStatus } from "@/hooks/use-academy-status";
import { AcademyHeroStrip } from "./academy-hero-strip";
import { AcademyLessonsPanel } from "./academy-lessons-panel";
import { AcademyProgressPanel } from "./academy-progress-panel";
import { AcademyQuizPanel } from "./academy-quiz-panel";
import { AcademySidebar } from "./academy-sidebar";
import { useStyles } from "./style";

type QuizValues = Record<string, string>;

const buildAnswerInputs = (values: QuizValues): AcademyQuizAnswerInput[] =>
  Object.entries(values).map(([questionKey, selectedOptionKey]) => ({
    questionKey,
    selectedOptionKey,
  }));

export const AcademyContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const academy = useAcademyStatus();
  const [course, setCourse] = useState<Awaited<ReturnType<typeof getAcademyCourse>> | null>(null);
  const [isCourseLoading, setIsCourseLoading] = useState(true);
  const [quizError, setQuizError] = useState<string | null>(null);
  const [quizResult, setQuizResult] = useState<Awaited<ReturnType<typeof submitAcademyQuiz>> | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [currentLessonIndex, setCurrentLessonIndex] = useState(0);
  const [completedLessonKeys, setCompletedLessonKeys] = useState<string[]>([]);
  const [isSavingProgress, setIsSavingProgress] = useState(false);
  const [form, setForm] = useState<FormInstance<QuizValues> | null>(null);

  useEffect(() => {
    void getAcademyCourse()
      .then(setCourse)
      .finally(() => setIsCourseLoading(false));
  }, []);

  useEffect(() => {
    if (academy.status?.hasTradeAcademyAccess) {
      router.replace(ROUTES.dashboard);
    }
  }, [academy.status, router]);

  const lessons = course?.lessons ?? [];
  const currentLesson = lessons[currentLessonIndex] ?? null;
  const allLessonsCompleted = lessons.length > 0 && completedLessonKeys.length >= lessons.length;
  const progressPercent = useMemo(() => {
    const score = academy.status?.bestIntroQuizScore ?? 0;
    const target = academy.status?.requiredQuizScorePercent ?? 90;
    return Math.max(0, Math.min(100, (score / target) * 100));
  }, [academy.status]);

  useEffect(() => {
    if (!course || !academy.status) {
      return;
    }

    const completedKeys = academy.status.completedLessonKeys.filter((key) =>
      course.lessons.some((lesson) => lesson.key === key),
    );
    const currentIndex = course.lessons.findIndex((lesson) => lesson.key === academy.status?.currentLessonKey);

    setCompletedLessonKeys(completedKeys);
    if (currentIndex >= 0) {
      setCurrentLessonIndex(currentIndex);
      return;
    }

    if (completedKeys.length >= course.lessons.length) {
      setCurrentLessonIndex(course.lessons.length - 1);
      return;
    }

    setCurrentLessonIndex(completedKeys.length);
  }, [academy.status, course]);

  const syncLessonProgress = async (nextCurrentLessonKey: string | null, nextCompletedLessonKeys: string[]) => {
    setIsSavingProgress(true);

    try {
      const nextStatus = await saveAcademyLessonProgress({
        currentLessonKey: nextCurrentLessonKey,
        completedLessonKeys: nextCompletedLessonKeys,
      });
      academy.setStatus(nextStatus);
    } finally {
      setIsSavingProgress(false);
    }
  };

  const completeCurrentLesson = async () => {
    if (!currentLesson) {
      return;
    }

    const nextCompletedKeys = completedLessonKeys.includes(currentLesson.key)
      ? completedLessonKeys
      : [...completedLessonKeys, currentLesson.key];
    const nextIndex = currentLessonIndex < lessons.length - 1 ? currentLessonIndex + 1 : currentLessonIndex;
    const nextLessonKey = lessons[nextIndex]?.key ?? currentLesson.key;

    setCompletedLessonKeys(nextCompletedKeys);
    setCurrentLessonIndex(nextIndex);
    await syncLessonProgress(nextCompletedKeys.length >= lessons.length ? currentLesson.key : nextLessonKey, nextCompletedKeys);
  };

  const handleSubmitQuiz = async () => {
    if (!form) {
      return;
    }

    const values = await form.validateFields();
    setIsSubmitting(true);
    setQuizError(null);

    try {
      const result = await submitAcademyQuiz(buildAnswerInputs(values));
      setQuizResult(result);
      await academy.refresh();

      if (result.passed) {
        window.setTimeout(() => {
          router.replace(ROUTES.dashboard);
        }, 1400);
      }
    } catch (submitError) {
      setQuizError(submitError instanceof Error ? submitError.message : "We could not submit your quiz.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const isQuizStep = allLessonsCompleted || Boolean(quizResult?.passed);
  const renderCenterContent = () => (
    <>
      <div className={styles.centerHeader}>
        <AcademyHeroStrip currentStep={quizResult?.passed ? 2 : isQuizStep ? 1 : 0} />
      </div>

      <div className={styles.centerStage}>
        {isQuizStep ? (
          <AcademyQuizPanel
            course={course!}
            academyStatus={academy.status}
            quizResult={quizResult}
            quizError={quizError}
            isSubmitting={isSubmitting}
            isStatusLoading={academy.isLoading}
            onSubmit={handleSubmitQuiz}
            onBackToLessons={() => {
              setCurrentLessonIndex(0);
              void syncLessonProgress(lessons[0]?.key ?? null, completedLessonKeys);
            }}
            onFormReady={setForm}
          />
        ) : (
          <AcademyLessonsPanel
            lesson={currentLesson}
            lessonIndex={currentLessonIndex}
            totalLessons={lessons.length}
            completedLessons={completedLessonKeys.length}
            onPrevious={currentLessonIndex > 0 ? () => {
              const nextIndex = currentLessonIndex - 1;
              setCurrentLessonIndex(nextIndex);
              void syncLessonProgress(lessons[nextIndex]?.key ?? null, completedLessonKeys);
            } : null}
            onContinue={() => void completeCurrentLesson()}
          />
        )}
      </div>
    </>
  );

  return (
    <div className={styles.page}>
      <div className={styles.shell}>
        {isCourseLoading ? (
          <Card className={styles.workspaceCard}>
            <Skeleton active paragraph={{ rows: 12 }} />
          </Card>
        ) : !course ? (
          <Card className={styles.workspaceCard}>
            <Empty description="The academy course is unavailable right now." />
          </Card>
        ) : quizResult?.passed ? (
          <Card className={styles.workspaceCard}>
            <Result
              status="success"
              title={quizResult.headline}
              subTitle={`${quizResult.summary} Redirecting you into trade academy now.`}
            />
          </Card>
        ) : (
          <div className={styles.academyGrid}>
            <div className={styles.leftRail}>
              <AcademySidebar
                lessons={lessons}
                currentLessonIndex={currentLessonIndex}
                completedLessonKeys={completedLessonKeys}
                isQuizUnlocked={allLessonsCompleted}
              />
            </div>

            {renderCenterContent()}

            <div className={styles.rightRail}>
              <AcademyProgressPanel
                academyStatus={academy.status}
                completedLessons={completedLessonKeys.length}
                totalLessons={lessons.length}
                quizProgressPercent={progressPercent}
                isSavingProgress={isSavingProgress}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
