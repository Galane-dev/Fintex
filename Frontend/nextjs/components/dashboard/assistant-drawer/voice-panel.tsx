"use client";

import { AudioOutlined, PauseCircleOutlined } from "@ant-design/icons";
import { Alert, Button, Space, Tag, Typography } from "antd";
import { getFintexButtonLoading } from "@/components/fintex-loader";
import type { AssistantMessage, AssistantVoiceStatus } from "@/types/assistant";
import { MessageList } from "./message-list";

type VoicePanelProps = {
  messages: AssistantMessage[];
  isListening: boolean;
  isConnecting: boolean;
  isBusy: boolean;
  voiceStatus: AssistantVoiceStatus;
  transcript: string;
  onStartListening: () => void;
  onStopListening: () => void;
};

export function VoicePanel({
  messages,
  isListening,
  isConnecting,
  isBusy,
  voiceStatus,
  transcript,
  onStartListening,
  onStopListening,
}: VoicePanelProps) {
  const statusColor =
    voiceStatus === "connected" || voiceStatus === "working"
      ? "green"
      : voiceStatus === "connecting"
        ? "blue"
        : voiceStatus === "error"
          ? "red"
          : "default";

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Alert
        type="info"
        showIcon
        message="Voice mode connects directly to OpenAI Realtime from the browser for lower-latency speech. Fintex only mints a short-lived client secret and the model uses tool calls to operate your dashboard."
      />
      <Space style={{ justifyContent: "space-between", width: "100%" }} wrap>
        <Typography.Text strong>Voice status</Typography.Text>
        <Tag color={statusColor}>{voiceStatus}</Tag>
      </Space>
      <Space direction="vertical" size={12} style={{ width: "100%" }}>
        <Button
          type="primary"
          icon={isListening ? <PauseCircleOutlined /> : <AudioOutlined />}
          loading={getFintexButtonLoading(isConnecting || isBusy)}
          onClick={isListening ? onStopListening : onStartListening}
        >
          {isListening ? "End voice chat" : "Start realtime voice chat"}
        </Button>
        <Typography.Text type="secondary">
          {transcript
            ? `Latest transcript: ${transcript}`
            : "Once connected, speak naturally. The assistant will answer with live audio and the transcript will appear here."}
        </Typography.Text>
        <div style={{ maxHeight: 300, overflowY: "auto", paddingRight: 4 }}>
          <MessageList messages={messages.slice(-8)} />
        </div>
      </Space>
    </Space>
  );
}
