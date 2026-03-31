import type { UserProfile } from "@/types/user-profile";
import { getAxiosInstance } from "./axios-instance";
import { unwrapAbpResponse } from "./abp-response";
import { normalizeUserProfile } from "./user-profile";

export const getMyUserProfile = async (): Promise<UserProfile> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().get("/api/services/app/UserProfile/GetMyProfile"),
    "We could not load your behavior profile.",
  );

  return normalizeUserProfile(result);
};

export const refreshMyBehavioralProfile = async (): Promise<UserProfile> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().post("/api/services/app/AiAnalysis/RefreshMyBehavioralProfile"),
    "We could not refresh your behavior analysis.",
  );

  return normalizeUserProfile(result);
};
