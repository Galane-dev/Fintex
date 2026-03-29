import type {
  AcademyCourse,
  AcademyQuizAnswerInput,
  AcademyQuizSubmissionResult,
  SaveAcademyLessonProgressInput,
  AcademyStatus,
} from "@/types/academy";
import { getAxiosInstance } from "./axios-instance";
import { unwrapAbpResponse } from "./abp-response";
import {
  normalizeAcademyCourse,
  normalizeAcademyQuizSubmissionResult,
  normalizeAcademyStatus,
} from "./academy";

export const getAcademyCourse = async (): Promise<AcademyCourse> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().get("/api/services/app/Academy/GetIntroCourse"),
    "We could not load the academy lessons.",
  );

  return normalizeAcademyCourse(result);
};

export const getAcademyStatus = async (): Promise<AcademyStatus> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().get("/api/services/app/Academy/GetMyStatus"),
    "We could not load your academy status.",
  );

  return normalizeAcademyStatus(result);
};

export const saveAcademyLessonProgress = async (
  input: SaveAcademyLessonProgressInput,
): Promise<AcademyStatus> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().post("/api/services/app/Academy/SaveIntroLessonProgress", input),
    "We could not save your academy lesson progress.",
  );

  return normalizeAcademyStatus(result);
};

export const submitAcademyQuiz = async (
  answers: AcademyQuizAnswerInput[],
): Promise<AcademyQuizSubmissionResult> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().post("/api/services/app/Academy/SubmitIntroQuiz", {
      answers,
    }),
    "We could not submit your academy quiz.",
  );

  return normalizeAcademyQuizSubmissionResult(result);
};
