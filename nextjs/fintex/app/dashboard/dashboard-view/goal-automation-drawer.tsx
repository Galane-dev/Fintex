"use client";

import { Drawer } from "antd";
import type { GoalTarget } from "@/types/goal-automation";
import { TargetsTab, type GoalExecutionTargetOption } from "./targets-tab";

type CreateGoalValues = {
  name?: string;
  executionTarget: string;
  targetType: "PercentGrowth" | "TargetAmount";
  targetPercent?: number;
  targetAmount?: number;
  deadlineLocal: string;
  maxAcceptableRisk: number;
  maxDrawdownPercent: number;
  maxPositionSizePercent: number;
  tradingSession: "AnyTime" | "Europe" | "Us" | "EuropeUsOverlap";
  allowOvernightPositions: boolean;
};

interface GoalAutomationDrawerProps {
  isOpen: boolean;
  isSaving: boolean;
  error: string | null;
  goals: GoalTarget[];
  executionTargets: GoalExecutionTargetOption[];
  onClose: () => void;
  onClearError: () => void;
  onPauseGoal: (goalId: number) => void;
  onResumeGoal: (goalId: number) => void;
  onCancelGoal: (goalId: number) => void;
  onCreateGoal: (values: CreateGoalValues) => Promise<boolean>;
}

export function GoalAutomationDrawer({
  isOpen,
  isSaving,
  error,
  goals,
  executionTargets,
  onClose,
  onClearError,
  onPauseGoal,
  onResumeGoal,
  onCancelGoal,
  onCreateGoal,
}: GoalAutomationDrawerProps) {
  return (
    <Drawer
      open={isOpen}
      onClose={onClose}
      title="Goal automation"
      placement="right"
      width={560}
      destroyOnHidden={false}
    >
      <TargetsTab
        isSaving={isSaving}
        error={error}
        goals={goals}
        executionTargets={executionTargets}
        onClearError={onClearError}
        onPauseGoal={onPauseGoal}
        onResumeGoal={onResumeGoal}
        onCancelGoal={onCancelGoal}
        onCreateGoal={onCreateGoal}
      />
    </Drawer>
  );
}
