import type {
  ConnectExternalBrokerAccountInput,
  ExternalBrokerConnection,
} from "@/types/external-broker";
import { getAxiosInstance } from "./axios-instance";
import { unwrapAbpResponse } from "./abp-response";
import {
  buildConnectExternalBrokerAccountInput,
  normalizeExternalBrokerConnection,
} from "./external-broker";

interface ListResult<T> {
  items?: T[];
}

export const getExternalBrokerConnections = async (): Promise<
  ExternalBrokerConnection[]
> => {
  const result = await unwrapAbpResponse<ListResult<Record<string, unknown>>>(
    getAxiosInstance().get("/api/services/app/ExternalBroker/GetMyConnections"),
    "We could not load your external broker connections.",
  );

  return Array.isArray(result?.items)
    ? result.items.map(normalizeExternalBrokerConnection)
    : [];
};

export const connectExternalBrokerAccount = async (
  input: ConnectExternalBrokerAccountInput,
): Promise<ExternalBrokerConnection> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().post(
      "/api/services/app/ExternalBroker/ConnectAlpacaAccount",
      buildConnectExternalBrokerAccountInput(input),
    ),
    "We could not connect the external broker account.",
  );

  return normalizeExternalBrokerConnection(result);
};

export const disconnectExternalBrokerAccount = async (
  id: number,
): Promise<void> => {
  await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().post("/api/services/app/ExternalBroker/Disconnect", { id }),
    "We could not disconnect the external broker account.",
  );
};
