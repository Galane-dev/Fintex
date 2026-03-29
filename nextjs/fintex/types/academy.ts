export type AcademyStage = "IntroCourse" | "TradeAcademy" | "Graduated";

export interface AcademyLesson {
  key: string;
  title: string;
  summary: string;
  contentMarkdown: string;
}

export interface AcademyQuizOption {
  key: string;
  label: string;
}

export interface AcademyQuizQuestion {
  key: string;
  prompt: string;
  options: AcademyQuizOption[];
}

export interface AcademyCourse {
  key: string;
  title: string;
  subtitle: string;
  requiredScorePercent: number;
  lessons: AcademyLesson[];
  quizQuestions: AcademyQuizQuestion[];
}

export interface AcademyStatus {
  academyStage: AcademyStage;
  hasTradeAcademyAccess: boolean;
  canConnectExternalBrokers: boolean;
  introQuizAttemptsCount: number;
  bestIntroQuizScore: number;
  requiredQuizScorePercent: number;
  introQuizPassedAt: string | null;
  academyGraduatedAt: string | null;
  paperStartingBalance: number | null;
  paperEquity: number | null;
  paperGrowthPercent: number;
  growthTargetPercent: number;
  currentLessonKey: string | null;
  completedLessonKeys: string[];
}

export interface AcademyQuizSubmissionResult {
  passed: boolean;
  correctAnswers: number;
  totalQuestions: number;
  scorePercent: number;
  requiredScorePercent: number;
  headline: string;
  summary: string;
  status: AcademyStatus;
}

export interface AcademyQuizAnswerInput {
  questionKey: string;
  selectedOptionKey: string;
}

export interface SaveAcademyLessonProgressInput {
  currentLessonKey: string | null;
  completedLessonKeys: string[];
}
