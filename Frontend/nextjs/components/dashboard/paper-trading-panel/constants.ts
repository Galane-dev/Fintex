"use client";

export const PAPER_EXECUTION_TARGET = "paper";

export const isPaperExecutionTarget = (value: string | undefined) =>
  !value || value === PAPER_EXECUTION_TARGET;

export const getExternalConnectionIdFromTarget = (value: string | undefined) => {
  if (!value || !value.startsWith("alpaca:")) {
    return null;
  }

  const parsed = Number(value.slice("alpaca:".length));
  return Number.isFinite(parsed) ? parsed : null;
};
