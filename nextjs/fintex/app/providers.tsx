"use client";

import { PropsWithChildren } from "react";
import { ConfigProvider, theme } from "antd";
import { StyleProvider } from "antd-style";
import { AuthProvider } from "@/providers/auth-provider";

const appTheme = {
  algorithm: theme.darkAlgorithm,
  token: {
    colorPrimary: "#4be16b",
    colorInfo: "#4be16b",
    colorSuccess: "#4be16b",
    colorBgBase: "#000000",
    colorBgContainer: "#000000",
    colorBgElevated: "#080908",
    colorBorder: "rgba(124, 148, 130, 0.22)",
    colorTextBase: "#f1f5f0",
    colorText: "#f1f5f0",
    colorTextSecondary: "#9ca89c",
    colorTextTertiary: "#6f7d70",
    colorLink: "#d7fbe0",
    borderRadius: 20,
    wireframe: false,
    fontFamily: "var(--font-inter), Inter, sans-serif",
    boxShadowSecondary: "0 28px 72px rgba(0, 0, 0, 0.42)",
  },
  components: {
    Layout: {
      bodyBg: "#000000",
      siderBg: "#040705",
      headerBg: "rgba(4, 7, 5, 0.84)",
    },
    Card: {
      colorBgContainer: "rgba(6, 6, 6, 0.17)",
    },
    Button: {
      controlHeight: 48,
      borderRadius: 999,
      fontWeight: 600,
    },
    Input: {
      controlHeight: 48,
      activeBorderColor: "#9bf2b1",
      hoverBorderColor: "#9bf2b1",
    },
    Menu: {
      darkItemBg: "transparent",
      darkItemSelectedBg: "rgba(0, 235, 59, 0.18)",
    },
  },
} as const;

export function Providers({ children }: PropsWithChildren) {
  return (
    <StyleProvider hashPriority="high">
      <ConfigProvider theme={appTheme}>
        <AuthProvider>{children}</AuthProvider>
      </ConfigProvider>
    </StyleProvider>
  );
}
