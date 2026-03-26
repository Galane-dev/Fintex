import type { UserProfile } from "@/types/user-profile";
import { apiClient } from "./api-client";
import { unwrapAbpResponse } from "./abp-response";
import { normalizeUserProfile } from "./user-profile";

export const getMyUserProfile = async (): Promise<UserProfile> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    apiClient.get("/api/services/app/UserProfile/GetMyProfile"),
    "We could not load your behavior profile.",
  );

  return normalizeUserProfile(result);
};
