import axios from "axios";
import type { AxiosError } from "axios";

interface AbpValidationError {
  message?: string;
}

interface AbpErrorPayload {
  message?: string;
  details?: string;
  validationErrors?: AbpValidationError[];
}

interface AbpResponse<T> {
  result?: T;
  error?: AbpErrorPayload;
}

const getErrorMessage = (error: unknown, fallback: string) => {
  if (!axios.isAxiosError(error)) {
    return fallback;
  }

  const payload = (error as AxiosError<AbpResponse<unknown>>).response?.data;
  const responseError = payload?.error;
  const validationMessage = responseError?.validationErrors?.find((item) => item.message)?.message;

  return validationMessage ?? responseError?.details ?? responseError?.message ?? error.message ?? fallback;
};

export const unwrapAbpResponse = async <T>(
  request: Promise<{ data: AbpResponse<T> | T }>,
  fallbackMessage: string,
) => {
  try {
    const response = await request;
    const payload = response.data;

    if (payload && typeof payload === "object" && "result" in payload) {
      return payload.result as T;
    }

    return payload as T;
  } catch (error) {
    throw new Error(getErrorMessage(error, fallbackMessage));
  }
};
