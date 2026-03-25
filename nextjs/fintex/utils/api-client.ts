import axios from "axios";
import { getApiBaseUrl } from "./api-config";
import { readStoredSession } from "./auth-storage";

const TENANT_ID = "1";

export const apiClient = axios.create({
  baseURL: getApiBaseUrl(),
  headers: {
    "Content-Type": "application/json",
    "Abp-TenantId": TENANT_ID,
  },
});

apiClient.interceptors.request.use((config) => {
  const session = readStoredSession();
  const headers = config.headers;

  headers.set("Abp-TenantId", TENANT_ID);

  if (headers.has("Authorization")) {
    return config;
  }

  if (session?.token) {
    headers.set("Authorization", `Bearer ${session.token}`);
  }

  return config;
});
