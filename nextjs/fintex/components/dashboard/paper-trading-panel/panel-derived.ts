"use client";

import { EXTERNAL_BROKER_PROVIDER_ALPACA } from "@/types/external-broker";
import type {
  ExternalBrokerConnection,
} from "@/types/external-broker";
import type { PaperTradingAccount } from "@/types/paper-trading";
import { getExternalBrokerEnvironmentLabel } from "@/utils/external-broker";
import { formatPrice } from "@/utils/market-data";
import { PAPER_EXECUTION_TARGET } from "./constants";
import type { AccountMetric, ExecutionTargetOption } from "./types";

export const sortConnections = (connections: ExternalBrokerConnection[]) =>
  [...connections].sort((left, right) => {
    if (left.isActive === right.isActive) {
      return right.id - left.id;
    }

    return left.isActive ? -1 : 1;
  });

export const getConnectedExternalConnections = (
  connections: ExternalBrokerConnection[],
) =>
  connections.filter(
    (item) =>
      item.isActive &&
      item.provider === EXTERNAL_BROKER_PROVIDER_ALPACA &&
      item.status !== "Failed" &&
      item.status !== "Disconnected",
  );

export const buildExecutionTargets = (
  account: PaperTradingAccount | null,
  connections: ExternalBrokerConnection[],
): ExecutionTargetOption[] => {
  const targets = account
    ? [{ label: "Paper academy", value: PAPER_EXECUTION_TARGET }]
    : [];

  connections.forEach((connection) => {
    targets.push({
      label: `${connection.displayName} (${getExternalBrokerEnvironmentLabel(connection)})`,
      value: `alpaca:${connection.id}`,
    });
  });

  return targets;
};

export const buildAccountMetrics = (
  account: PaperTradingAccount | null,
): AccountMetric[] =>
  !account
    ? []
    : [
        { label: "Cash balance", value: formatPrice(account.cashBalance), tone: "neutral" },
        { label: "Equity", value: formatPrice(account.equity), tone: "positive" },
        {
          label: "Realized P/L",
          value: formatPrice(account.realizedProfitLoss),
          tone: account.realizedProfitLoss >= 0 ? "positive" : "negative",
        },
        {
          label: "Unrealized P/L",
          value: formatPrice(account.unrealizedProfitLoss),
          tone: account.unrealizedProfitLoss >= 0 ? "positive" : "negative",
        },
      ];
