"use client";

import { AudioOutlined, PauseCircleOutlined } from "@ant-design/icons";
import { Alert, Button, Space, Switch, Typography } from "antd";

type VoicePanelProps = {
  isListening: boolean;
  isSending: boolean;
  transcript: string;
  speakReplies: boolean;
  onToggleSpeakReplies: (value: boolean) => void;
  onStartListening: () => void;
  onStopListening: () => void;
};

export function VoicePanel({
  isListening,
  isSending,
  transcript,
  speakReplies,
  onToggleSpeakReplies,
  onStartListening,
  onStopListening,
}: VoicePanelProps) {
  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Alert
        type="info"
        showIcon
        message="Voice mode listens through your browser microphone and reads the assistant reply aloud."
      />
      <Space style={{ justifyContent: "space-between", width: "100%" }}>
        <Typography.Text>Speak replies aloud</Typography.Text>
        <Switch checked={speakReplies} onChange={onToggleSpeakReplies} />
      </Space>
      <Space direction="vertical" size={12} style={{ width: "100%" }}>
        <Button
          type="primary"
          icon={isListening ? <PauseCircleOutlined /> : <AudioOutlined />}
          loading={isSending}
          onClick={isListening ? onStopListening : onStartListening}
        >
          {isListening ? "Stop listening" : "Start voice chat"}
        </Button>
        <Typography.Text type="secondary">
          {transcript ? `Live transcript: ${transcript}` : "Your transcript will appear here while you speak."}
        </Typography.Text>
      </Space>
    </Space>
  );
}
