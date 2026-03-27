"use client";

import { useCallback, useState } from "react";
import type { UserProfile } from "@/types/user-profile";
import { refreshMyBehavioralProfile } from "@/utils/user-profile-api";

export const useDashboardBehaviorAnalysis = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [profile, setProfile] = useState<UserProfile | null>(null);

  const open = useCallback(async () => {
    setIsOpen(true);
    setIsLoading(true);
    setError(null);

    try {
      const nextProfile = await refreshMyBehavioralProfile();
      setProfile(nextProfile);
    } catch (profileError) {
      setError(
        profileError instanceof Error
          ? profileError.message
          : "We could not load your behavior analysis.",
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
    profile,
    open,
    close,
  };
};
