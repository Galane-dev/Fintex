"use client";

import { useCallback, useState } from "react";
import type { EconomicCalendarInsight } from "@/types/economic-calendar";
import { getBitcoinUsdRiskInsight } from "@/utils/economic-calendar-api";

export const useDashboardEconomicCalendar = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [insight, setInsight] = useState<EconomicCalendarInsight | null>(null);

  const open = useCallback(async () => {
    setIsOpen(true);
    setIsLoading(true);
    setError(null);

    try {
      const nextInsight = await getBitcoinUsdRiskInsight();
      setInsight(nextInsight);
    } catch (calendarError) {
      setError(
        calendarError instanceof Error
          ? calendarError.message
          : "We could not load the economic calendar.",
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  const close = useCallback(() => {
    setIsOpen(false);
  }, []);

  return {
    isOpen,
    isLoading,
    error,
    insight,
    open,
    close,
  };
};
