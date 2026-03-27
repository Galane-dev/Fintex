import type { AssistantMessage, AssistantResponse } from "@/types/assistant";
import { getAxiosInstance } from "./axios-instance";
import { normalizeAssistantResponse } from "./assistant";

export const sendAssistantMessage = async (
  message: string,
  conversation: AssistantMessage[],
  voiceMode: boolean,
): Promise<AssistantResponse> => {
  const response = await getAxiosInstance().post("/api/services/app/Assistant/SendMessage", {
    message,
    voiceMode,
    conversation: conversation.slice(-12).map((item) => ({
      role: item.role,
      content: item.content,
    })),
  });

  return normalizeAssistantResponse(response.data.result ?? response.data);
};
