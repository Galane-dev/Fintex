"use client";

import { Avatar, List, Space, Tag, Typography } from "antd";
import type { AssistantMessage } from "@/types/assistant";

type MessageListProps = {
  messages: AssistantMessage[];
};

export function MessageList({ messages }: MessageListProps) {
  return (
    <List
      dataSource={messages}
      renderItem={(message) => (
        <List.Item>
          <List.Item.Meta
            avatar={<Avatar>{message.role === "assistant" ? "AI" : "You"}</Avatar>}
            title={<Typography.Text strong>{message.role === "assistant" ? "Fintex Copilot" : "You"}</Typography.Text>}
            description={
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Typography.Text>{message.content}</Typography.Text>
                {message.actionResults?.map((item) => (
                  <Space key={`${message.id}-${item.actionType}`} wrap>
                    <Tag color={item.status === "completed" ? "green" : item.status === "failed" ? "red" : "orange"}>
                      {item.status}
                    </Tag>
                    <Typography.Text strong>{item.title}</Typography.Text>
                    <Typography.Text type="secondary">{item.summary}</Typography.Text>
                  </Space>
                ))}
              </Space>
            }
          />
        </List.Item>
      )}
    />
  );
}
