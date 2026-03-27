export type AssistantActionResult = {
  actionType: string;
  status: string;
  title: string;
  summary: string;
};

export type AssistantResponse = {
  reply: string;
  voiceReply: string | null;
  usedAi: boolean;
  provider: string | null;
  model: string | null;
  suggestedPrompts: string[];
  actionResults: AssistantActionResult[];
};

export type AssistantMessage = {
  id: string;
  role: "user" | "assistant";
  content: string;
  actionResults?: AssistantActionResult[];
};
