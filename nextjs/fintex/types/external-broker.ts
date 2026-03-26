export const EXTERNAL_BROKER_PROVIDER_ALPACA = 2;
export const EXTERNAL_BROKER_PLATFORM_DIRECT_API = 2;

export type ExternalBrokerConnectionStatus =
  | "Pending"
  | "Connected"
  | "Failed"
  | "Disconnected";

export interface ExternalBrokerConnection {
  id: number;
  displayName: string;
  provider: number;
  platform: number;
  accountLogin: string;
  server: string;
  terminalPath: string | null;
  status: ExternalBrokerConnectionStatus;
  isActive: boolean;
  brokerAccountName: string;
  brokerAccountCurrency: string;
  brokerCompany: string;
  brokerLeverage: number | null;
  lastKnownBalance: number | null;
  lastKnownEquity: number | null;
  lastError: string;
  lastValidatedAt: string | null;
  lastSyncedAt: string | null;
}

export interface ConnectExternalBrokerAccountInput {
  displayName: string;
  provider: number;
  platform: number;
  apiKey: string;
  apiSecret: string;
  isPaperEnvironment: boolean;
}

export interface DisconnectExternalBrokerAccountInput {
  id: number;
}

export interface ExternalBrokerState {
  isLoading: boolean;
  isSubmitting: boolean;
  error: string | null;
  connections: ExternalBrokerConnection[];
  lastHydratedAt: string | null;
}

export interface ExternalBrokerProviderActions {
  refreshConnections: () => Promise<void>;
  connectAccount: (
    input: ConnectExternalBrokerAccountInput,
  ) => Promise<ExternalBrokerConnection | null>;
  disconnectAccount: (input: DisconnectExternalBrokerAccountInput) => Promise<void>;
  clearError: () => void;
}
