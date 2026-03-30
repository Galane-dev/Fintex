"use client";

import { useCallback, useEffect, useState } from "react";
import type { AcademyStatus } from "@/types/academy";
import { getAcademyStatus } from "@/utils/academy-api";

interface UseAcademyStatusOptions {
  enabled?: boolean;
}

export const useAcademyStatus = ({ enabled = true }: UseAcademyStatusOptions = {}) => {
  const [status, setStatus] = useState<AcademyStatus | null>(null);
  const [isLoading, setIsLoading] = useState(enabled);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!enabled) {
      setIsLoading(false);
      return null;
    }

    setIsLoading(true);
    setError(null);

    try {
      const nextStatus = await getAcademyStatus();
      setStatus(nextStatus);
      return nextStatus;
    } catch (statusError) {
      setError(
        statusError instanceof Error ? statusError.message : "We could not load your academy status.",
      );
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [enabled]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  return {
    status,
    isLoading,
    error,
    refresh,
    setStatus,
  };
};
