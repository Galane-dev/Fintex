"use client";

import { PropsWithChildren } from "react";
import { ConfigProvider, theme } from "antd";
import { StyleProvider } from "antd-style";
import { AuthProvider } from "@/providers/auth-provider";

const appTheme = {
  algorithm: theme.darkAlgorithm,
  token: {
    colorPrimary: "#34f5c5",
    colorInfo: "#34f5c5",
    colorSuccess: "#34f5c5",
    colorBgBase: "#05070b",
    colorBgContainer: "#0a1018",
    colorBgElevated: "#0c1420",
    colorBorder: "rgba(118, 154, 198, 0.24)",
    colorTextBase: "#f5fbff",
    colorText: "#f5fbff",
    colorTextSecondary: "#9bb0c7",
    colorTextTertiary: "#6d8098",
    colorLink: "#75f9d5",
    borderRadius: 20,
    wireframe: false,
    fontFamily: "var(--font-inter), Inter, sans-serif",
    boxShadowSecondary: "0 30px 80px rgba(0, 0, 0, 0.35)",
  },
  components: {
    Layout: {
      bodyBg: "#05070b",
      siderBg: "#05070b",
      headerBg: "rgba(5, 7, 11, 0.82)",
    },
    Card: {
      colorBgContainer: "rgba(10, 16, 24, 0.88)",
    },
    Button: {
      controlHeight: 48,
      borderRadius: 999,
      fontWeight: 600,
    },
    Input: {
      controlHeight: 48,
      activeBorderColor: "#34f5c5",
      hoverBorderColor: "#34f5c5",
    },
    Menu: {
      darkItemBg: "transparent",
      darkItemSelectedBg: "rgba(52, 245, 197, 0.14)",
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
