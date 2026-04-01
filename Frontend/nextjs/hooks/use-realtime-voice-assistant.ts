"use client";

import { useCallback, useMemo, useRef, useState } from "react";
import type { AssistantVoiceStatus } from "@/types/assistant";
import { createAssistantRealtimeSession } from "@/utils/assistant-api";
import {
  assistantRealtimeTools,
  createAssistantRealtimeToolHandlers,
} from "@/utils/assistant-realtime-tools";

type UseRealtimeVoiceAssistantOptions = {
  onUserMessage: (content: string) => void;
  onAssistantMessage: (content: string) => void;
  onActionRefresh?: () => Promise<void> | void;
};

type RealtimeServerEvent = {
  type?: string;
  name?: string;
  call_id?: string;
  arguments?: string;
  transcript?: string;
  error?: {
    message?: string;
  };
};

const TRANSCRIPTION_MODEL = "gpt-4o-mini-transcribe";

export const useRealtimeVoiceAssistant = ({
  onUserMessage,
  onAssistantMessage,
  onActionRefresh,
}: UseRealtimeVoiceAssistantOptions) => {
  const [status, setStatus] = useState<AssistantVoiceStatus>("idle");
  const [transcript, setTranscript] = useState("");
  const [error, setError] = useState<string | null>(null);
  const peerConnectionRef = useRef<RTCPeerConnection | null>(null);
  const dataChannelRef = useRef<RTCDataChannel | null>(null);
  const audioElementRef = useRef<HTMLAudioElement | null>(null);
  const localStreamRef = useRef<MediaStream | null>(null);
  const activeToolCallsRef = useRef(0);

  const toolHandlers = useMemo(
    () => createAssistantRealtimeToolHandlers({ onActionRefresh }),
    [onActionRefresh],
  );

  const sendClientEvent = useCallback((payload: Record<string, unknown>) => {
    if (dataChannelRef.current?.readyState !== "open") {
      return;
    }

    dataChannelRef.current.send(JSON.stringify(payload));
  }, []);

  const cleanupConnection = useCallback((nextStatus: AssistantVoiceStatus = "idle") => {
    dataChannelRef.current?.close();
    dataChannelRef.current = null;
    peerConnectionRef.current?.close();
    peerConnectionRef.current = null;

    localStreamRef.current?.getTracks().forEach((track) => track.stop());
    localStreamRef.current = null;

    if (audioElementRef.current) {
      audioElementRef.current.pause();
      audioElementRef.current.srcObject = null;
      audioElementRef.current = null;
    }

    activeToolCallsRef.current = 0;
    setStatus(nextStatus);
  }, []);

  const handleToolCall = useCallback(async (event: RealtimeServerEvent) => {
    if (!event.name || !event.call_id) {
      return;
    }

    const handler = toolHandlers[event.name];
    activeToolCallsRef.current += 1;
    setStatus("working");

    try {
      const parsedArguments = event.arguments ? JSON.parse(event.arguments) as Record<string, unknown> : {};
      const result = handler
        ? await handler(parsedArguments)
        : { status: "ignored", summary: `No tool handler exists for ${event.name}.` };

      sendClientEvent({
        type: "conversation.item.create",
        item: {
          type: "function_call_output",
          call_id: event.call_id,
          output: JSON.stringify(result),
        },
      });
    } catch (toolError) {
      sendClientEvent({
        type: "conversation.item.create",
        item: {
          type: "function_call_output",
          call_id: event.call_id,
          output: JSON.stringify({
            status: "failed",
            message: toolError instanceof Error ? toolError.message : "Tool execution failed.",
          }),
        },
      });
    } finally {
      activeToolCallsRef.current = Math.max(0, activeToolCallsRef.current - 1);
      setStatus(activeToolCallsRef.current > 0 ? "working" : "connected");
      sendClientEvent({ type: "response.create" });
    }
  }, [sendClientEvent, toolHandlers]);

  const handleServerEvent = useCallback((event: RealtimeServerEvent) => {
    switch (event.type) {
      case "conversation.item.input_audio_transcription.completed":
        if (event.transcript) {
          setTranscript(event.transcript);
          onUserMessage(event.transcript);
        }
        return;
      case "response.audio_transcript.done":
        if (event.transcript) {
          onAssistantMessage(event.transcript);
        }
        return;
      case "response.function_call_arguments.done":
        void handleToolCall(event);
        return;
      case "error":
        setError(event.error?.message ?? "Realtime voice chat failed.");
        setStatus("error");
        return;
      default:
        return;
    }
  }, [handleToolCall, onAssistantMessage, onUserMessage]);

  const stopVoiceChat = useCallback(() => {
    cleanupConnection("idle");
    setTranscript("");
    setError(null);
  }, [cleanupConnection]);

  const startVoiceChat = useCallback(async () => {
    if (status === "connecting" || status === "connected" || status === "working") {
      return;
    }

    setStatus("connecting");
    setTranscript("");
    setError(null);

    try {
      const session = await createAssistantRealtimeSession();
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const peerConnection = new RTCPeerConnection();
      const remoteAudio = new Audio();
      remoteAudio.autoplay = true;
      remoteAudio.setAttribute("playsinline", "true");
      audioElementRef.current = remoteAudio;
      localStreamRef.current = stream;
      peerConnectionRef.current = peerConnection;

      stream.getTracks().forEach((track) => {
        peerConnection.addTrack(track, stream);
      });

      peerConnection.ontrack = (rtcEvent) => {
        audioElementRef.current!.srcObject = rtcEvent.streams[0];
        void audioElementRef.current!.play().catch(() => undefined);
      };

      peerConnection.onconnectionstatechange = () => {
        const connectionState = peerConnection.connectionState;
        if (connectionState === "connected") {
          setStatus(activeToolCallsRef.current > 0 ? "working" : "connected");
        }

        if (connectionState === "failed" || connectionState === "closed") {
          cleanupConnection("error");
          setError("Realtime voice chat disconnected.");
        }
      };

      const dataChannel = peerConnection.createDataChannel("oai-events");
      dataChannelRef.current = dataChannel;

      dataChannel.addEventListener("open", () => {
        sendClientEvent({
          type: "session.update",
          session: {
            instructions: session.instructions,
            voice: session.voice,
            audio: {
              input: {
                transcription: { model: TRANSCRIPTION_MODEL },
                turn_detection: {
                  type: "server_vad",
                  create_response: true,
                  interrupt_response: true,
                  threshold: 0.45,
                  prefix_padding_ms: 250,
                  silence_duration_ms: 350,
                },
              },
            },
            tools: assistantRealtimeTools,
            tool_choice: "auto",
          },
        });
      });

      dataChannel.addEventListener("message", (messageEvent) => {
        try {
          handleServerEvent(JSON.parse(String(messageEvent.data)) as RealtimeServerEvent);
        } catch {
          // Ignore malformed server events and keep the session alive.
        }
      });

      const offer = await peerConnection.createOffer();
      await peerConnection.setLocalDescription(offer);

      const response = await fetch(`https://api.openai.com/v1/realtime?model=${encodeURIComponent(session.model)}`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${session.clientSecret}`,
          "Content-Type": "application/sdp",
          "OpenAI-Beta": "realtime=v1",
        },
        body: offer.sdp ?? "",
      });

      if (!response.ok) {
        throw new Error("OpenAI Realtime did not accept the WebRTC offer.");
      }

      const answerSdp = await response.text();
      await peerConnection.setRemoteDescription({ type: "answer", sdp: answerSdp });
    } catch (voiceError) {
      cleanupConnection("error");
      setError(
        voiceError instanceof Error
          ? voiceError.message
          : "We could not start realtime voice chat.",
      );
    }
  }, [cleanupConnection, handleServerEvent, sendClientEvent, status]);

  return {
    error,
    transcript,
    voiceStatus: status,
    isConnecting: status === "connecting",
    isConnected: status === "connected" || status === "working",
    isBusy: status === "working",
    startVoiceChat,
    stopVoiceChat,
  };
};
