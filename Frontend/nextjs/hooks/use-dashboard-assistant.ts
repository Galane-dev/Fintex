"use client";

import { useCallback, useMemo, useState } from "react";
import type { AssistantMessage } from "@/types/assistant";
import { sendAssistantMessage } from "@/utils/assistant-api";
import { useRealtimeVoiceAssistant } from "./use-realtime-voice-assistant";

type UseDashboardAssistantOptions = {
  onActionRefresh?: () => Promise<void> | void;
};

const createMessage = (
  role: AssistantMessage["role"],
  content: string,
  actionResults?: AssistantMessage["actionResults"],
): AssistantMessage => ({
  id: `${role}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
  role,
  content,
  actionResults,
});

export const useDashboardAssistant = ({ onActionRefresh }: UseDashboardAssistantOptions) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [draft, setDraft] = useState("");
  const [messages, setMessages] = useState<AssistantMessage[]>([
    createMessage(
      "assistant",
      "I can explain the dashboard, inspect your live context, manage alerts, place trades, work with goal automation, review behavior analysis, and validate strategies.",
    ),
  ]);
  const [suggestedPrompts, setSuggestedPrompts] = useState<string[]>([
    "Explain the current verdict.",
    "Set a BTC alert at 70000 and email me.",
    "Give me a trade recommendation right now.",
    "Review my automation rules and goals.",
  ]);

  const appendMessage = useCallback((message: AssistantMessage) => {
    setMessages((current) => [...current, message]);
  }, []);

  const voiceAssistant = useRealtimeVoiceAssistant({
    onActionRefresh,
    onUserMessage: (content) => appendMessage(createMessage("user", content)),
    onAssistantMessage: (content) => appendMessage(createMessage("assistant", content)),
  });

  const submitMessage = useCallback(async (content: string) => {
    if (!content.trim()) {
      return;
    }

    const userMessage = createMessage("user", content.trim());
    const nextConversation = [...messages, userMessage];

    setMessages(nextConversation);
    setDraft("");
    setError(null);
    setIsSending(true);

    try {
      const response = await sendAssistantMessage(content.trim(), nextConversation, false);
      appendMessage(createMessage("assistant", response.reply, response.actionResults));
      setSuggestedPrompts(
        response.suggestedPrompts.length > 0 ? response.suggestedPrompts : suggestedPrompts,
      );

      if (response.actionResults.length > 0 && onActionRefresh) {
        await onActionRefresh();
      }
    } catch (assistantError) {
      setError(
        assistantError instanceof Error
          ? assistantError.message
          : "We could not get a response from the assistant.",
      );
    } finally {
      setIsSending(false);
    }
  }, [appendMessage, messages, onActionRefresh, suggestedPrompts]);

  const actionValues = useMemo(
    () => ({
      open: () => setIsOpen(true),
      close: () => setIsOpen(false),
      setDraft,
      submitMessage,
      startListening: () => {
        setError(null);
        void voiceAssistant.startVoiceChat();
      },
      stopListening: () => voiceAssistant.stopVoiceChat(),
    }),
    [submitMessage, voiceAssistant],
  );

  return {
    isOpen,
    isSending: isSending || voiceAssistant.isConnecting || voiceAssistant.isBusy,
    isListening: voiceAssistant.isConnected,
    isVoiceConnecting: voiceAssistant.isConnecting,
    voiceStatus: voiceAssistant.voiceStatus,
    error: error ?? voiceAssistant.error,
    draft,
    transcript: voiceAssistant.transcript,
    messages,
    suggestedPrompts,
    ...actionValues,
  };
};
