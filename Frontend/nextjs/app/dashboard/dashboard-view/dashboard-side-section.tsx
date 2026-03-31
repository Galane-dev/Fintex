"use client";

import { Collapse } from "antd";
import type { ReactNode } from "react";
import { useStyles } from "../style";

type DashboardSideSectionProps = {
  title: string;
  defaultOpen?: boolean;
  children: ReactNode;
};

export function DashboardSideSection({
  title,
  defaultOpen = false,
  children,
}: DashboardSideSectionProps) {
  const { styles } = useStyles();

  return (
    <div className={styles.sideSection}>
      <Collapse
        className={styles.sideSectionCollapse}
        ghost
        defaultActiveKey={defaultOpen ? ["content"] : []}
        items={[
          {
            key: "content",
            label: <span className={styles.sideSectionLabel}>{title}</span>,
            children,
          },
        ]}
      />
    </div>
  );
}
