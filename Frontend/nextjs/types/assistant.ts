export type AssistantActionResult = {
  actionType: string;
  status: string;
  title: string;
  summary: string;
};

export type AssistantVoiceStatus =
  | "idle"
  | "connecting"
  | "connected"
  | "working"
  | "error";

export type AssistantResponse = {
  reply: string;
  voiceReply: string | null;
  usedAi: boolean;
  provider: string | null;
  model: string | null;
  suggestedPrompts: string[];
  actionResults: AssistantActionResult[];
};

export type AssistantRealtimeSession = {
  clientSecret: string;
  expiresAtUtc: string | null;
  model: string;
  voice: string;
  instructions: string;
};

export type AssistantMessage = {
  id: string;
  role: "user" | "assistant";
  content: string;
  actionResults?: AssistantActionResult[];
};
