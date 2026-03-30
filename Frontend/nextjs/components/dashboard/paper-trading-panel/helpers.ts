"use client";

import type { ExternalBrokerConnectionStatus } from "@/types/external-broker";

export const getRiskTone = (riskLevel: "Low" | "Medium" | "High") => {
  if (riskLevel === "High") {
    return "red";
  }

  if (riskLevel === "Medium") {
    return "gold";
  }

  return "green";
};

export const getRecommendationActionTone = (
  action: "Buy" | "Sell" | "Hold" | null,
) => {
  if (action === "Buy") {
    return "green";
  }

  if (action === "Sell") {
    return "red";
  }

  return "default";
};

export const getExternalConnectionStatusTone = (
  status: ExternalBrokerConnectionStatus,
) => {
  if (status === "Connected") {
    return "green" as const;
  }

  if (status === "Failed") {
    return "red" as const;
  }

  if (status === "Pending") {
    return "gold" as const;
  }

  return "default" as const;
};
