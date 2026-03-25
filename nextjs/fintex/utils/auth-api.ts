import axios from "axios";
import type { AxiosError } from "axios";
import type { AuthSession, AuthUser, SignInValues, SignUpValues } from "@/types/auth";
import { apiClient } from "./api-client";

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

interface AuthenticateResponse {
  accessToken?: string;
  AccessToken?: string;
  expireInSeconds?: number;
  ExpireInSeconds?: number;
  userId?: number;
  UserId?: number;
}

interface CurrentLoginInformationsResponse {
  user?: {
    name?: string;
    Name?: string;
    surname?: string;
    Surname?: string;
    emailAddress?: string;
    EmailAddress?: string;
  } | null;
}

interface RegisterResponse {
  canLogin?: boolean;
  CanLogin?: boolean;
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

const unwrapResponse = async <T>(request: Promise<{ data: AbpResponse<T> | T }>, fallbackMessage: string) => {
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

const mapAuthUser = (payload: CurrentLoginInformationsResponse["user"]): AuthUser => {
  if (!payload) {
    throw new Error("Your session was created, but no user details were returned.");
  }

  return {
    email: payload.emailAddress ?? payload.EmailAddress ?? "",
    firstName: payload.name ?? payload.Name ?? "",
    lastName: payload.surname ?? payload.Surname ?? "",
  };
};

const getCurrentLoginInformations = async (token: string) =>
  unwrapResponse<CurrentLoginInformationsResponse>(
    apiClient.get("/api/services/app/Session/GetCurrentLoginInformations", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }),
    "We could not fetch your login information.",
  );

const buildSession = async (authenticateResponse: AuthenticateResponse): Promise<AuthSession> => {
  const token = authenticateResponse.accessToken ?? authenticateResponse.AccessToken;

  if (!token) {
    throw new Error("Authentication succeeded, but no access token was returned.");
  }

  const loginInformations = await getCurrentLoginInformations(token);

  return {
    token,
    userId: authenticateResponse.userId ?? authenticateResponse.UserId ?? null,
    expiresInSeconds:
      authenticateResponse.expireInSeconds ?? authenticateResponse.ExpireInSeconds ?? null,
    user: mapAuthUser(loginInformations.user),
  };
};

export const signInRequest = async (values: SignInValues) => {
  const response = await unwrapResponse<AuthenticateResponse>(
    apiClient.post("/api/TokenAuth/Authenticate", {
      userNameOrEmailAddress: values.email,
      password: values.password,
      rememberClient: true,
    }),
    "We could not sign you in.",
  );

  apiClient.defaults.headers.common.Authorization = `Bearer ${response.accessToken ?? response.AccessToken}`;

  return buildSession(response);
};

export const signUpRequest = async (values: SignUpValues) => {
  const registerResponse = await unwrapResponse<RegisterResponse>(
    apiClient.post("/api/services/app/Account/Register", {
      name: values.firstName,
      surname: values.lastName,
      userName: values.email,
      emailAddress: values.email,
      password: values.password,
      captchaResponse: "",
    }),
    "We could not create your account.",
  );

  const canLogin = registerResponse.canLogin ?? registerResponse.CanLogin ?? false;

  if (!canLogin) {
    throw new Error("Your account was created, but automatic login is not available yet.");
  }

  return signInRequest({
    email: values.email,
    password: values.password,
  });
};
