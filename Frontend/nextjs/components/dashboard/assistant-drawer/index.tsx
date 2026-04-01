"use client";

import { MessageOutlined, SendOutlined } from "@ant-design/icons";
import { Alert, Button, Input, Space, Tabs, Typography } from "antd";
import { DashboardDrawerShell } from "@/components/dashboard/dashboard-drawer-shell";
import { getFintexButtonLoading } from "@/components/fintex-loader";
import type { AssistantMessage, AssistantVoiceStatus } from "@/types/assistant";
import { MessageList } from "./message-list";
import { VoicePanel } from "./voice-panel";

type AssistantDrawerProps = {
  isOpen: boolean;
  isSending: boolean;
  isListening: boolean;
  isVoiceConnecting: boolean;
  voiceStatus: AssistantVoiceStatus;
  error: string | null;
  draft: string;
  transcript: string;
  messages: AssistantMessage[];
  suggestedPrompts: string[];
  onClose: () => void;
  onDraftChange: (value: string) => void;
  onSubmit: (value: string) => void;
  onStartListening: () => void;
  onStopListening: () => void;
};

export function AssistantDrawer({
  isOpen,
  isSending,
  isListening,
  isVoiceConnecting,
  voiceStatus,
  error,
  draft,
  transcript,
  messages,
  suggestedPrompts,
  onClose,
  onDraftChange,
  onSubmit,
  onStartListening,
  onStopListening,
}: AssistantDrawerProps) {
  return (
    <DashboardDrawerShell
      open={isOpen}
      onClose={onClose}
      width={420}
      title={<Space><MessageOutlined /><span>Fintex Copilot</span></Space>}
    >
      <Space direction="vertical" size={16} style={{ width: "100%" }}>
        {error ? <Alert type="warning" showIcon message={error} /> : null}
        <Typography.Text type="secondary">
          Ask about the market, your paper or live trades, alerts, automation rules, goals, behavior analysis, or strategy validation. Voice chat now runs as a direct realtime audio session.
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
                    placeholder="Ask me to explain the verdict, manage alerts, place trades, review automation, or create and manage BTC goals."
                    autoSize={{ minRows: 4, maxRows: 8 }}
                  />
                  <Button
                    type="primary"
                    icon={<SendOutlined />}
                    loading={getFintexButtonLoading(isSending)}
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
                  messages={messages}
                  isListening={isListening}
                  isConnecting={isVoiceConnecting}
                  isBusy={isSending}
                  voiceStatus={voiceStatus}
                  transcript={transcript}
                  onStartListening={onStartListening}
                  onStopListening={onStopListening}
                />
              ),
            },
          ]}
        />
      </Space>
    </DashboardDrawerShell>
  );
}
