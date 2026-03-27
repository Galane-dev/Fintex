import type { AuthSession, AuthUser, SignInValues, SignUpValues } from "@/types/auth";
import { getAxiosInstance, setAxiosAuthorizationHeader } from "./axios-instance";
import { unwrapAbpResponse } from "./abp-response";

interface AuthenticateResponse {
  accessToken?: string;
  AccessToken?: string;
  encryptedAccessToken?: string;
  EncryptedAccessToken?: string;
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
  unwrapAbpResponse<CurrentLoginInformationsResponse>(
    getAxiosInstance().get("/api/services/app/Session/GetCurrentLoginInformations", {
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
    encryptedToken:
      authenticateResponse.encryptedAccessToken ?? authenticateResponse.EncryptedAccessToken ?? null,
    userId: authenticateResponse.userId ?? authenticateResponse.UserId ?? null,
    expiresInSeconds:
      authenticateResponse.expireInSeconds ?? authenticateResponse.ExpireInSeconds ?? null,
    user: mapAuthUser(loginInformations.user),
  };
};

export const signInRequest = async (values: SignInValues) => {
  const response = await unwrapAbpResponse<AuthenticateResponse>(
    getAxiosInstance().post("/api/TokenAuth/Authenticate", {
      userNameOrEmailAddress: values.email,
      password: values.password,
      rememberClient: true,
    }),
    "We could not sign you in.",
  );

  setAxiosAuthorizationHeader(response.accessToken ?? response.AccessToken ?? null);

  return buildSession(response);
};

export const signUpRequest = async (values: SignUpValues) => {
  const registerResponse = await unwrapAbpResponse<RegisterResponse>(
    getAxiosInstance().post("/api/services/app/Account/Register", {
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
