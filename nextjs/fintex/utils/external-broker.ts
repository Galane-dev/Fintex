import type {
  ConnectExternalBrokerAccountInput,
  ExternalBrokerConnection,
  ExternalBrokerConnectionStatus,
} from "@/types/external-broker";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getNullableNumber = (value: unknown) =>
  value == null ? null : typeof value === "number" ? value : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

const getNullableString = (value: unknown) =>
  value == null ? null : getString(value);

export const normalizeExternalBrokerConnection = (
  value: Record<string, unknown>,
): ExternalBrokerConnection => ({
  id: getNumber(value.id ?? value.Id),
  displayName: getString(value.displayName ?? value.DisplayName),
  provider: getNumber(value.provider ?? value.Provider),
  platform: getNumber(value.platform ?? value.Platform),
  accountLogin: getString(value.accountLogin ?? value.AccountLogin),
  server: getString(value.server ?? value.Server),
  terminalPath: getNullableString(value.terminalPath ?? value.TerminalPath),
  status: getString(
    value.status ?? value.Status,
  ) as ExternalBrokerConnectionStatus,
  isActive: Boolean(value.isActive ?? value.IsActive),
  brokerAccountName: getString(value.brokerAccountName ?? value.BrokerAccountName),
  brokerAccountCurrency: getString(
    value.brokerAccountCurrency ?? value.BrokerAccountCurrency,
  ),
  brokerCompany: getString(value.brokerCompany ?? value.BrokerCompany),
  brokerLeverage: getNullableNumber(value.brokerLeverage ?? value.BrokerLeverage),
  lastKnownBalance: getNullableNumber(value.lastKnownBalance ?? value.LastKnownBalance),
  lastKnownEquity: getNullableNumber(value.lastKnownEquity ?? value.LastKnownEquity),
  lastError: getString(value.lastError ?? value.LastError),
  lastValidatedAt: getNullableString(value.lastValidatedAt ?? value.LastValidatedAt),
  lastSyncedAt: getNullableString(value.lastSyncedAt ?? value.LastSyncedAt),
});

export const buildConnectExternalBrokerAccountInput = (
  input: ConnectExternalBrokerAccountInput,
): Record<string, unknown> => ({
  displayName: input.displayName,
  apiKey: input.apiKey,
  apiSecret: input.apiSecret,
  isPaperEnvironment: input.isPaperEnvironment,
});

export const getExternalBrokerStatusTone = (
  status: ExternalBrokerConnectionStatus,
): "green" | "gold" | "red" | "default" => {
  switch (status) {
    case "Connected":
      return "green";
    case "Pending":
      return "gold";
    case "Failed":
      return "red";
    default:
      return "default";
  }
};

export const getExternalBrokerEnvironmentLabel = (
  connection: ExternalBrokerConnection,
): "Paper" | "Live" | "Unknown" => {
  const server = connection.server.toLowerCase();

  if (server.includes("paper-api.alpaca.markets")) {
    return "Paper";
  }

  if (server.includes("api.alpaca.markets")) {
    return "Live";
  }

  return "Unknown";
};

export const maskExternalBrokerKey = (value: string): string => {
  if (!value) {
    return "-";
  }

  const trimmed = value.trim();
  if (trimmed.length <= 8) {
    return trimmed;
  }

  return `${trimmed.slice(0, 4)}...${trimmed.slice(-4)}`;
};
