"use client";

import { Drawer, type DrawerProps } from "antd";

const sharedDrawerStyles: NonNullable<DrawerProps["styles"]> = {
  mask: {
    backdropFilter: "blur(18px)",
    WebkitBackdropFilter: "blur(18px)",
    background: "rgba(3, 6, 8, 0.36)",
  },
  content: {
    background: "rgba(7, 8, 9, 0.96)",
  },
  header: {
    background: "rgba(7, 8, 9, 0.96)",
    borderBottom: "1px solid rgba(255, 255, 255, 0.06)",
    padding: "18px 22px",
  },
  body: {
    padding: "22px",
  },
  footer: {
    background: "rgba(7, 8, 9, 0.96)",
    borderTop: "1px solid rgba(255, 255, 255, 0.06)",
    padding: "16px 22px",
  },
};

export function DashboardDrawerShell({ placement = "right", styles, ...props }: DrawerProps) {
  return (
    <Drawer
      placement={placement}
      styles={{
        ...sharedDrawerStyles,
        ...styles,
      }}
      {...props}
    />
  );
}
