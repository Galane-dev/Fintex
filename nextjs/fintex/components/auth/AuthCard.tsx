"use client";

import Link from "next/link";
import { useSearchParams, useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, Form, Input, Space, Typography } from "antd";
import { LockOutlined, MailOutlined, UserOutlined } from "@ant-design/icons";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import type { SignInValues, SignUpValues } from "@/types/auth";
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

  const redirectPath = searchParams.get("redirect") || ROUTES.dashboard;
  const isSignIn = mode === "sign-in";

  useEffect(() => {
    if (isAuthenticated) {
      router.replace(redirectPath);
    }
  }, [isAuthenticated, redirectPath, router]);

  const copy = useMemo(
    () =>
      isSignIn
        ? {
            title: "Welcome back",
            subtitle: "Sign in to continue into your protected trading workspace.",
            button: "Sign in",
            alternateLabel: "Need an account?",
            alternateRoute: ROUTES.signUp,
            alternateAction: "Create one",
          }
        : {
            title: "Create your account",
            subtitle: "Open your FinteX workspace and start with a secure trading setup.",
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

      router.replace(redirectPath);
    } catch {
      setErrorMessage("We could not complete that request. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className={styles.content}>
      <Card className={styles.card}>
        <Space orientation="vertical" size="large" style={{ width: "100%" }}>
          <div>
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
              <Space size="middle" style={{ display: "flex" }}>
                <Form.Item
                  name="firstName"
                  label="First name"
                  rules={[{ required: true, message: "Enter your first name." }]}
                  style={{ flex: 1 }}
                >
                  <Input prefix={<UserOutlined />} placeholder="Ada" />
                </Form.Item>
                <Form.Item
                  name="lastName"
                  label="Last name"
                  rules={[{ required: true, message: "Enter your last name." }]}
                  style={{ flex: 1 }}
                >
                  <Input prefix={<UserOutlined />} placeholder="Lovelace" />
                </Form.Item>
              </Space>
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
              <Input.Password prefix={<LockOutlined />} placeholder="••••••••" />
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
              <Button htmlType="submit" type="primary" block loading={isSubmitting}>
                {copy.button}
              </Button>
            </Form.Item>
          </Form>

          <div className={styles.footerRow}>
            <Typography.Text type="secondary">{copy.alternateLabel}</Typography.Text>
            <Link href={copy.alternateRoute}>{copy.alternateAction}</Link>
          </div>
        </Space>
      </Card>
    </div>
  );
}
