"use client";

import Link from "next/link";
import { useSearchParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, Form, Input, Space, Typography } from "antd";
import { LockOutlined, MailOutlined, UserOutlined } from "@ant-design/icons";
import { FintexLoader, getFintexButtonLoading } from "@/components/fintex-loader";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import type { SignInValues, SignUpValues } from "@/types/auth";
import { getAcademyStatus } from "@/utils/academy-api";
import { useStyles } from "./style";

interface AuthCardProps {
  mode: "sign-in" | "sign-up";
}

export function AuthCard({ mode }: AuthCardProps) {
  const { styles } = useStyles();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isAuthenticated, signIn, signUp } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const requestedRedirectPath = searchParams.get("redirect");
  const isSignIn = mode === "sign-in";

  const resolvePostAuthRoute = useCallback(async () => {
    if (requestedRedirectPath) {
      return requestedRedirectPath;
    }

    try {
      const academyStatus = await getAcademyStatus();
      return academyStatus.hasTradeAcademyAccess ? ROUTES.dashboard : ROUTES.academy;
    } catch {
      return ROUTES.academy;
    }
  }, [requestedRedirectPath]);

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    let isCancelled = false;

    const navigate = async () => {
      const destination = await resolvePostAuthRoute();

      if (!isCancelled) {
        router.replace(destination);
      }
    };

    void navigate();

    return () => {
      isCancelled = true;
    };
  }, [isAuthenticated, resolvePostAuthRoute, router]);

  const copy = useMemo(
    () =>
      isSignIn
        ? {
            badge: "Secure access",
            title: "Welcome back",
            subtitle: "Sign in to continue to your protected Fintex trading workspace.",
            button: "Sign in",
            alternateLabel: "Need an account?",
            alternateRoute: ROUTES.signUp,
            alternateAction: "Create one",
          }
        : {
            badge: "Account setup",
            title: "Create your account",
            subtitle: "Open your Fintex workspace and start with a secure trading setup.",
            button: "Create account",
            alternateLabel: "Already have an account?",
            alternateRoute: ROUTES.signIn,
            alternateAction: "Sign in",
          },
    [isSignIn],
  );

  const handleSubmit = async (values: SignInValues | SignUpValues) => {
    try {
      setIsSubmitting(true);
      setErrorMessage(null);

      if (isSignIn) {
        await signIn(values as SignInValues);
      } else {
        await signUp(values as SignUpValues);
      }
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "We could not complete that request. Please try again.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isAuthenticated) {
    return <FintexLoader variant="fullscreen" label="Loading" />;
  }

  return (
    <div className={styles.content}>
      <Card className={styles.card}>
        <Space orientation="vertical" size="large" style={{ width: "100%" }}>
          <div>
            <Typography.Text className={styles.overline}>{copy.badge}</Typography.Text>
            <Typography.Title level={2} className={styles.heading}>
              {copy.title}
            </Typography.Title>
            <Typography.Paragraph className={styles.helper}>
              {copy.subtitle}
            </Typography.Paragraph>
          </div>

          {errorMessage ? <Alert type="error" message={errorMessage} showIcon /> : null}

          <Form layout="vertical" requiredMark={false} onFinish={handleSubmit}>
            {!isSignIn ? (
              <div className={styles.nameGrid}>
                <Form.Item
                  name="firstName"
                  label="First name"
                  rules={[{ required: true, message: "Enter your first name." }]}
                >
                  <Input prefix={<UserOutlined />} placeholder="Ada" />
                </Form.Item>
                <Form.Item
                  name="lastName"
                  label="Last name"
                  rules={[{ required: true, message: "Enter your last name." }]}
                >
                  <Input prefix={<UserOutlined />} placeholder="Lovelace" />
                </Form.Item>
              </div>
            ) : null}

            <Form.Item
              name="email"
              label="Email"
              rules={[
                { required: true, message: "Enter your email address." },
                { type: "email", message: "Enter a valid email address." },
              ]}
            >
              <Input prefix={<MailOutlined />} placeholder="you@fintex.com" />
            </Form.Item>

            <Form.Item
              name="password"
              label="Password"
              rules={[{ required: true, message: "Enter your password." }]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="********" />
            </Form.Item>

            {!isSignIn ? (
              <Form.Item
                name="terms"
                valuePropName="checked"
                rules={[
                  {
                    validator: async (_, value) => {
                      if (value) {
                        return;
                      }

                      throw new Error("You need to accept the terms to continue.");
                    },
                  },
                ]}
              >
                <Checkbox>I agree to the terms and acknowledge the risk disclosure.</Checkbox>
              </Form.Item>
            ) : null}

            <Form.Item style={{ marginBottom: 12 }}>
              <Button
                htmlType="submit"
                type="primary"
                block
                loading={getFintexButtonLoading(isSubmitting)}
                className={styles.submitButton}
              >
                {copy.button}
              </Button>
            </Form.Item>
          </Form>

          <div className={styles.footerRow}>
            <Typography.Text type="secondary">{copy.alternateLabel}</Typography.Text>
            <Link href={copy.alternateRoute} className={styles.link}>
              {copy.alternateAction}
            </Link>
          </div>
        </Space>
      </Card>
    </div>
  );
}
