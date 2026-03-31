"use client";

import { Alert, Skeleton } from "antd";
import type { PaperTradingPanelProps } from "./types";
import { AssessmentModal } from "./assessment-modal";
import { AccountsModal } from "./accounts-modal";
import { PanelSections } from "./panel-sections";
import { RecommendationModal } from "./recommendation-modal";
import { TradeModal } from "./trade-modal";
import { usePaperTradingPanelController } from "./use-paper-trading-panel-controller";
import { usePaperTradingStyles } from "../paper-trading-style";

export type { DashboardPaperTradingActions } from "./types";

export const PaperTradingPanel = ({
  currentPrice,
  registerDashboardActions,
  displayMode = "full",
}: PaperTradingPanelProps) => {
  const { styles } = usePaperTradingStyles();
  const controller = usePaperTradingPanelController({ registerDashboardActions });

  const shouldShowInitialSkeleton =
    displayMode !== "support" &&
    controller.isLoading &&
    !controller.account &&
    controller.positions.length === 0;

  if (shouldShowInitialSkeleton) {
    return <Skeleton active paragraph={{ rows: 8 }} />;
  }

  return (
    <div className={styles.wrapper}>
      {controller.combinedError ? (
        <Alert
          type="warning"
          showIcon
          title={controller.combinedError}
          closable
          onClose={controller.handleClearAnyError}
        />
      ) : null}

      <AssessmentModal controller={controller} />
      <AccountsModal controller={controller} currentPrice={currentPrice} />
      <TradeModal controller={controller} />
      <RecommendationModal controller={controller} />

      <PanelSections controller={controller} currentPrice={currentPrice} />
    </div>
  );
};
