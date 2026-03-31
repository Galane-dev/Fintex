import axios, { type AxiosInstance } from "axios";
import { getApiBaseUrl } from "./api-config";
import { readStoredSession } from "./auth-storage";

const TENANT_ID = "1";

let axiosInstance: AxiosInstance | null = null;

const buildAxiosInstance = () => {
  const instance = axios.create({
    baseURL: getApiBaseUrl(),
    headers: {
      "Content-Type": "application/json",
      "Abp-TenantId": TENANT_ID,
    },
  });

  instance.interceptors.request.use((config) => {
    const session = readStoredSession();
    const headers = config.headers;

    headers.set("Abp-TenantId", TENANT_ID);

    if (!headers.has("Authorization") && session?.token) {
      headers.set("Authorization", `Bearer ${session.token}`);
    }

    return config;
  });

  return instance;
};

export const getAxiosInstance = () => {
  if (!axiosInstance) {
    axiosInstance = buildAxiosInstance();
  }

  return axiosInstance;
};

export const setAxiosAuthorizationHeader = (token: string | null) => {
  const instance = getAxiosInstance();

  if (token) {
    instance.defaults.headers.common.Authorization = `Bearer ${token}`;
    return;
  }

  delete instance.defaults.headers.common.Authorization;
};
