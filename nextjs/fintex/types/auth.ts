export interface AuthUser {
  email: string;
  firstName: string;
  lastName?: string;
}

export interface AuthSession {
  user: AuthUser;
  token: string;
}

export interface AuthState {
  isReady: boolean;
  isAuthenticated: boolean;
  user: AuthUser | null;
}

export interface SignInValues {
  email: string;
  password: string;
}

export interface SignUpValues extends SignInValues {
  firstName: string;
  lastName: string;
}

export interface AuthContextValue extends AuthState {
  signIn: (values: SignInValues) => Promise<void>;
  signUp: (values: SignUpValues) => Promise<void>;
  signOut: () => void;
}
