"use client";

import { useCallback, useState } from "react";
import type {
  StrategyValidationResult,
  ValidateStrategyInput,
} from "@/types/strategy-validation";
import {
  getMyStrategyValidationHistory,
  validateMyStrategy,
} from "@/utils/strategy-validation-api";

export const useDashboardStrategyValidation = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoadingHistory, setIsLoadingHistory] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [history, setHistory] = useState<StrategyValidationResult[]>([]);
  const [latestResult, setLatestResult] = useState<StrategyValidationResult | null>(null);

  const loadHistory = useCallback(async () => {
    setIsLoadingHistory(true);
    setError(null);

    try {
      const items = await getMyStrategyValidationHistory();
      setHistory(items);
      setLatestResult((current) => current ?? items[0] ?? null);
    } catch (validationError) {
      setError(
        validationError instanceof Error
          ? validationError.message
          : "We could not load your strategy validation history.",
      );
    } finally {
      setIsLoadingHistory(false);
    }
  }, []);

  const open = useCallback(async () => {
    setIsOpen(true);
    await loadHistory();
  }, [loadHistory]);

  const close = useCallback(() => {
    setIsOpen(false);
  }, []);

  const submit = useCallback(async (input: ValidateStrategyInput) => {
    setIsSubmitting(true);
    setError(null);

    try {
      const result = await validateMyStrategy(input);
      setLatestResult(result);
      setHistory((current) => [result, ...current.filter((item) => item.id !== result.id)].slice(0, 8));
      return result;
    } catch (validationError) {
      setError(
        validationError instanceof Error
          ? validationError.message
          : "We could not validate that strategy.",
      );
      return null;
    } finally {
      setIsSubmitting(false);
    }
  }, []);

  return {
    isOpen,
    isLoadingHistory,
    isSubmitting,
    error,
    history,
    latestResult,
    open,
    close,
    submit,
  };
};
