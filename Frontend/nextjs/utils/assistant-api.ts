import type {
  AssistantMessage,
  AssistantRealtimeSession,
  AssistantResponse,
} from "@/types/assistant";
import { getAxiosInstance } from "./axios-instance";
import { unwrapAbpResponse } from "./abp-response";
import {
  normalizeAssistantRealtimeSession,
  normalizeAssistantResponse,
} from "./assistant";

export const sendAssistantMessage = async (
  message: string,
  conversation: AssistantMessage[],
  voiceMode: boolean,
): Promise<AssistantResponse> => {
  const clientTimeZone =
    typeof window === "undefined" ? null : Intl.DateTimeFormat().resolvedOptions().timeZone;
  const clientNowIso = new Date().toISOString();
  const response = await getAxiosInstance().post("/api/services/app/Assistant/SendMessage", {
    message,
    voiceMode,
    clientTimeZone,
    clientNowIso,
    conversation: conversation.slice(-12).map((item) => ({
      role: item.role,
      content: item.content,
    })),
  });

  return normalizeAssistantResponse(response.data.result ?? response.data);
};

export const createAssistantRealtimeSession = async (): Promise<AssistantRealtimeSession> => {
  const clientTimeZone =
    typeof window === "undefined" ? null : Intl.DateTimeFormat().resolvedOptions().timeZone;
  const clientNowIso = new Date().toISOString();
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().post("/api/services/app/Assistant/CreateRealtimeVoiceSession", {
      clientTimeZone,
      clientNowIso,
    }),
    "We could not start a realtime voice session.",
  );

  return normalizeAssistantRealtimeSession(result);
};
