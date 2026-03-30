"use client";

import { createContext } from "react";
import type { GoalAutomationActions, GoalAutomationState } from "@/types/goal-automation";

export const GoalAutomationStateContext = createContext<GoalAutomationState | null>(null);
export const GoalAutomationActionContext = createContext<GoalAutomationActions | null>(null);
