"use client";

import { MessageOutlined, SendOutlined } from "@ant-design/icons";
import { Alert, Button, Drawer, Input, Space, Tabs, Typography } from "antd";
import type { AssistantMessage } from "@/types/assistant";
import { MessageList } from "./message-list";
import { VoicePanel } from "./voice-panel";

type AssistantDrawerProps = {
  isOpen: boolean;
  isSending: boolean;
  isListening: boolean;
  error: string | null;
  draft: string;
  transcript: string;
  speakReplies: boolean;
  messages: AssistantMessage[];
  suggestedPrompts: string[];
  onClose: () => void;
  onDraftChange: (value: string) => void;
  onSubmit: (value: string, voiceMode?: boolean) => void;
  onStartListening: () => void;
  onStopListening: () => void;
  onToggleSpeakReplies: (value: boolean) => void;
};

export function AssistantDrawer({
  isOpen,
  isSending,
  isListening,
  error,
  draft,
  transcript,
  speakReplies,
  messages,
  suggestedPrompts,
  onClose,
  onDraftChange,
  onSubmit,
  onStartListening,
  onStopListening,
  onToggleSpeakReplies,
}: AssistantDrawerProps) {
  return (
    <Drawer
      open={isOpen}
      onClose={onClose}
      width={420}
      placement="right"
      title={<Space><MessageOutlined /><span>Fintex Copilot</span></Space>}
    >
      <Space direction="vertical" size={16} style={{ width: "100%" }}>
        {error ? <Alert type="warning" showIcon message={error} /> : null}
        <Typography.Text type="secondary">
          Ask about the market, your trades, alerts, recommendations, or ask me to create and manage BTC target goals for you.
        </Typography.Text>
        <Space wrap>
          {suggestedPrompts.map((prompt) => (
            <Button key={prompt} onClick={() => onSubmit(prompt)}>
              {prompt}
            </Button>
          ))}
        </Space>
        <Tabs
          items={[
            {
              key: "text",
              label: "Text chat",
              children: (
                <Space direction="vertical" size={16} style={{ width: "100%" }}>
                  <div style={{ maxHeight: 420, overflowY: "auto", paddingRight: 4 }}>
                    <MessageList messages={messages} />
                  </div>
                  <Input.TextArea
                    value={draft}
                    onChange={(event) => onDraftChange(event.target.value)}
                    placeholder="Ask me to explain the verdict, set an alert, place a trade, or create a BTC target goal."
                    autoSize={{ minRows: 4, maxRows: 8 }}
                  />
                  <Button
                    type="primary"
                    icon={<SendOutlined />}
                    loading={isSending}
                    onClick={() => onSubmit(draft)}
                  >
                    Send
                  </Button>
                </Space>
              ),
            },
            {
              key: "voice",
              label: "Voice chat",
              children: (
                <VoicePanel
                  isListening={isListening}
                  isSending={isSending}
                  transcript={transcript}
                  speakReplies={speakReplies}
                  onToggleSpeakReplies={onToggleSpeakReplies}
                  onStartListening={onStartListening}
                  onStopListening={onStopListening}
                />
              ),
            },
          ]}
        />
      </Space>
    </Drawer>
  );
}
