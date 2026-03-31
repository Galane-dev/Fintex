import type {
  AcademyCourse,
  AcademyLesson,
  AcademyQuizOption,
  AcademyQuizQuestion,
  AcademyQuizSubmissionResult,
  AcademyStatus,
} from "@/types/academy";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

const getNullableString = (value: unknown) => (value == null ? null : getString(value));

const getArray = (value: unknown) => (Array.isArray(value) ? value : []);

const normalizeOption = (value: Record<string, unknown>): AcademyQuizOption => ({
  key: getString(value.key ?? value.Key),
  label: getString(value.label ?? value.Label),
});

const normalizeQuestion = (value: Record<string, unknown>): AcademyQuizQuestion => ({
  key: getString(value.key ?? value.Key),
  prompt: getString(value.prompt ?? value.Prompt),
  options: getArray(value.options ?? value.Options).map((item) =>
    normalizeOption(item as Record<string, unknown>),
  ),
});

const normalizeLesson = (value: Record<string, unknown>): AcademyLesson => ({
  key: getString(value.key ?? value.Key),
  title: getString(value.title ?? value.Title),
  summary: getString(value.summary ?? value.Summary),
  contentMarkdown: getString(value.contentMarkdown ?? value.ContentMarkdown),
});

export const normalizeAcademyStatus = (value: Record<string, unknown>): AcademyStatus => ({
  academyStage: getString(value.academyStage ?? value.AcademyStage) as AcademyStatus["academyStage"],
  hasTradeAcademyAccess: Boolean(value.hasTradeAcademyAccess ?? value.HasTradeAcademyAccess),
  canConnectExternalBrokers: Boolean(value.canConnectExternalBrokers ?? value.CanConnectExternalBrokers),
  introQuizAttemptsCount: getNumber(value.introQuizAttemptsCount ?? value.IntroQuizAttemptsCount),
  bestIntroQuizScore: getNumber(value.bestIntroQuizScore ?? value.BestIntroQuizScore),
  requiredQuizScorePercent: getNumber(value.requiredQuizScorePercent ?? value.RequiredQuizScorePercent),
  introQuizPassedAt: getNullableString(value.introQuizPassedAt ?? value.IntroQuizPassedAt),
  academyGraduatedAt: getNullableString(value.academyGraduatedAt ?? value.AcademyGraduatedAt),
  paperStartingBalance:
    value.paperStartingBalance == null && value.PaperStartingBalance == null
      ? null
      : getNumber(value.paperStartingBalance ?? value.PaperStartingBalance),
  paperEquity:
    value.paperEquity == null && value.PaperEquity == null
      ? null
      : getNumber(value.paperEquity ?? value.PaperEquity),
  paperGrowthPercent: getNumber(value.paperGrowthPercent ?? value.PaperGrowthPercent),
  growthTargetPercent: getNumber(value.growthTargetPercent ?? value.GrowthTargetPercent),
  currentLessonKey:
    value.currentLessonKey == null && value.CurrentLessonKey == null
      ? null
      : getString(value.currentLessonKey ?? value.CurrentLessonKey),
  completedLessonKeys: getArray(value.completedLessonKeys ?? value.CompletedLessonKeys).map((item) =>
    getString(item),
  ),
});

export const normalizeAcademyCourse = (value: Record<string, unknown>): AcademyCourse => ({
  key: getString(value.key ?? value.Key),
  title: getString(value.title ?? value.Title),
  subtitle: getString(value.subtitle ?? value.Subtitle),
  requiredScorePercent: getNumber(value.requiredScorePercent ?? value.RequiredScorePercent),
  lessons: getArray(value.lessons ?? value.Lessons).map((item) =>
    normalizeLesson(item as Record<string, unknown>),
  ),
  quizQuestions: getArray(value.quizQuestions ?? value.QuizQuestions).map((item) =>
    normalizeQuestion(item as Record<string, unknown>),
  ),
});

export const normalizeAcademyQuizSubmissionResult = (
  value: Record<string, unknown>,
): AcademyQuizSubmissionResult => ({
  passed: Boolean(value.passed ?? value.Passed),
  correctAnswers: getNumber(value.correctAnswers ?? value.CorrectAnswers),
  totalQuestions: getNumber(value.totalQuestions ?? value.TotalQuestions),
  scorePercent: getNumber(value.scorePercent ?? value.ScorePercent),
  requiredScorePercent: getNumber(value.requiredScorePercent ?? value.RequiredScorePercent),
  headline: getString(value.headline ?? value.Headline),
  summary: getString(value.summary ?? value.Summary),
  status: normalizeAcademyStatus((value.status ?? value.Status ?? {}) as Record<string, unknown>),
});
