import type { AssistantActionResult, AssistantResponse } from "@/types/assistant";

const normalizeActionResult = (
  payload: Record<string, unknown>,
): AssistantActionResult => ({
  actionType: String(payload.actionType ?? payload.ActionType ?? ""),
  status: String(payload.status ?? payload.Status ?? "completed"),
  title: String(payload.title ?? payload.Title ?? "Action update"),
  summary: String(payload.summary ?? payload.Summary ?? ""),
});

export const normalizeAssistantResponse = (
  payload: Record<string, unknown>,
): AssistantResponse => {
  const voiceReply = payload.voiceReply ?? payload.VoiceReply;
  const provider = payload.provider ?? payload.Provider;
  const model = payload.model ?? payload.Model;
  const suggestedPrompts = payload.suggestedPrompts ?? payload.SuggestedPrompts;
  const actionResults = payload.actionResults ?? payload.ActionResults;

  return {
    reply: String(payload.reply ?? payload.Reply ?? "I'm here to help."),
    voiceReply: typeof voiceReply === "string" ? voiceReply : null,
    usedAi: Boolean(payload.usedAi ?? payload.UsedAi),
    provider: typeof provider === "string" ? provider : null,
    model: typeof model === "string" ? model : null,
    suggestedPrompts: Array.isArray(suggestedPrompts)
      ? suggestedPrompts.map((item) => String(item))
      : [],
    actionResults: Array.isArray(actionResults)
      ? actionResults.map((item) =>
          normalizeActionResult(item as Record<string, unknown>),
        )
      : [],
  };
};
