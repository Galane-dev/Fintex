import type { CreateGoalTargetInput, GoalTarget } from "@/types/goal-automation";
import { getAxiosInstance } from "./axios-instance";
import { normalizeGoalTarget } from "./goal-automation";

export const getMyGoalTargets = async (): Promise<GoalTarget[]> => {
  const response = await getAxiosInstance().get("/api/services/app/GoalAutomation/GetMyGoals");
  const payload = response.data.result ?? response.data;
  const items = Array.isArray(payload?.items)
    ? (payload.items as unknown[])
    : Array.isArray(payload)
      ? (payload as unknown[])
      : [];
  return items.map((item) => normalizeGoalTarget(item as Record<string, unknown>));
};

export const createGoalTarget = async (input: CreateGoalTargetInput) => {
  await getAxiosInstance().post("/api/services/app/GoalAutomation/CreateGoal", input);
};

export const pauseGoalTarget = async (goalId: number) => {
  await getAxiosInstance().post("/api/services/app/GoalAutomation/PauseGoal", { id: goalId });
};

export const resumeGoalTarget = async (goalId: number) => {
  await getAxiosInstance().post("/api/services/app/GoalAutomation/ResumeGoal", { id: goalId });
};

export const cancelGoalTarget = async (goalId: number) => {
  await getAxiosInstance().post("/api/services/app/GoalAutomation/CancelGoal", { id: goalId });
};
