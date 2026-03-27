"use client";

import { useCallback, useMemo, useRef, useState } from "react";
import type { AssistantMessage } from "@/types/assistant";
import { sendAssistantMessage } from "@/utils/assistant-api";

type UseDashboardAssistantOptions = {
  onActionRefresh?: () => Promise<void> | void;
};

const recognitionName = "webkitSpeechRecognition";
type BrowserSpeechRecognitionConstructor = new () => SpeechRecognition;

const createMessage = (role: AssistantMessage["role"], content: string, actionResults?: AssistantMessage["actionResults"]): AssistantMessage => ({
  id: `${role}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
  role,
  content,
  actionResults,
});

export const useDashboardAssistant = ({ onActionRefresh }: UseDashboardAssistantOptions) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [isListening, setIsListening] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [draft, setDraft] = useState("");
  const [transcript, setTranscript] = useState("");
  const [speakReplies, setSpeakReplies] = useState(true);
  const [messages, setMessages] = useState<AssistantMessage[]>([
    createMessage("assistant", "I can explain the dashboard, set alerts, get recommendations, and place trades for you."),
  ]);
  const [suggestedPrompts, setSuggestedPrompts] = useState<string[]>([
    "Explain the current verdict.",
    "Set a BTC alert at 70000 and email me.",
    "Give me a trade recommendation right now.",
    "Place a small paper BTC buy.",
  ]);
  const recognitionRef = useRef<SpeechRecognition | null>(null);

  const speak = useCallback((value: string | null) => {
    if (!value || typeof window === "undefined" || !("speechSynthesis" in window) || !speakReplies) {
      return;
    }

    window.speechSynthesis.cancel();
    window.speechSynthesis.speak(new SpeechSynthesisUtterance(value));
  }, [speakReplies]);

  const submitMessage = useCallback(async (content: string, voiceMode = false) => {
    if (!content.trim()) {
      return;
    }

    const userMessage = createMessage("user", content.trim());
    setMessages((current) => [...current, userMessage]);
    setDraft("");
    setTranscript("");
    setError(null);
    setIsSending(true);

    try {
      const response = await sendAssistantMessage(content.trim(), [...messages, userMessage], voiceMode);
      setMessages((current) => [
        ...current,
        createMessage("assistant", response.reply, response.actionResults),
      ]);
      setSuggestedPrompts(response.suggestedPrompts.length > 0 ? response.suggestedPrompts : suggestedPrompts);
      speak(response.voiceReply ?? response.reply);

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
  }, [messages, onActionRefresh, speak, suggestedPrompts]);

  const stopListening = useCallback(() => {
    recognitionRef.current?.stop();
    recognitionRef.current = null;
    setIsListening(false);
  }, []);

  const startListening = useCallback(() => {
    const SpeechRecognitionConstructor =
      typeof window === "undefined"
        ? null
        : (window.SpeechRecognition ??
            (window as Window & typeof globalThis & { [recognitionName]?: BrowserSpeechRecognitionConstructor })[
              recognitionName
            ] ??
            null);

    if (!SpeechRecognitionConstructor) {
      setError("Voice input is not supported in this browser.");
      return;
    }

    const recognition = new SpeechRecognitionConstructor();
    recognition.lang = "en-US";
    recognition.interimResults = true;
    recognition.continuous = false;

    recognition.onresult = (event) => {
      const nextTranscript = Array.from(event.results)
        .map((result) => result[0]?.transcript ?? "")
        .join(" ")
        .trim();

      setTranscript(nextTranscript);

      const lastResult = event.results[event.results.length - 1];
      if (lastResult?.isFinal && nextTranscript) {
        void submitMessage(nextTranscript, true);
      }
    };

    recognition.onerror = () => {
      setError("Voice input stopped unexpectedly.");
      setIsListening(false);
    };

    recognition.onend = () => {
      setIsListening(false);
      recognitionRef.current = null;
    };

    setTranscript("");
    setError(null);
    setIsListening(true);
    recognitionRef.current = recognition;
    recognition.start();
  }, [submitMessage]);

  const actionValues = useMemo(
    () => ({
      open: () => setIsOpen(true),
      close: () => setIsOpen(false),
      setDraft,
      setSpeakReplies,
      submitMessage,
      startListening,
      stopListening,
    }),
    [startListening, stopListening, submitMessage],
  );

  return {
    isOpen,
    isSending,
    isListening,
    error,
    draft,
    transcript,
    speakReplies,
    messages,
    suggestedPrompts,
    ...actionValues,
  };
};
