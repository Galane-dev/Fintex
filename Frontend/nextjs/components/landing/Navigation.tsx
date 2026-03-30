"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { LoginOutlined, MenuOutlined, RocketOutlined } from "@ant-design/icons";
import { Button, Drawer, Space, Typography } from "antd";
import { navLinks } from "@/constants/landing";
import { ROUTES } from "@/constants/routes";
import { useAuth } from "@/hooks/useAuth";
import { useStyles } from "./style";

export function Navigation() {
  const { styles } = useStyles();
  const { isAuthenticated, signOut } = useAuth();
  const [isOpen, setIsOpen] = useState(false);

  const primaryAction = useMemo(
    () => (isAuthenticated ? ROUTES.dashboard : ROUTES.signUp),
    [isAuthenticated],
  );

  return (
    <div className={styles.navWrap}>
      <div className={styles.shell}>
        <div className={styles.navInner}>
          <Link href={ROUTES.home} className={styles.brand}>
            <span className={styles.brandBadge}>F</span>
            <Typography.Text className={styles.brandText}>FinteX</Typography.Text>
          </Link>

          <div className={styles.navLinks}>
            {navLinks.map((link) => (
              <a key={link.label} href={link.href} className={styles.navLink}>
                {link.label}
              </a>
            ))}
          </div>

          <div className={styles.navActions}>
            {isAuthenticated ? (
              <>
                <Link href={ROUTES.dashboard}>
                  <Button type="text" icon={<RocketOutlined />}>
                    Dashboard
                  </Button>
                </Link>
                <Button onClick={signOut}>Sign out</Button>
              </>
            ) : (
              <>
                <Link href={ROUTES.signIn}>
                  <Button type="text" icon={<LoginOutlined />}>
                    Sign in
                  </Button>
                </Link>
                <Link href={primaryAction}>
                  <Button type="primary">Get started</Button>
                </Link>
              </>
            )}
          </div>

          <Button
            className={styles.mobileMenu}
            icon={<MenuOutlined />}
            onClick={() => setIsOpen(true)}
            aria-label="Open navigation"
          />
        </div>
      </div>

      <Drawer
        placement="right"
        open={isOpen}
        onClose={() => setIsOpen(false)}
        title="FinteX"
        styles={{
          body: { background: "#05070b" },
          header: { background: "#05070b", color: "#f5fbff", borderBottom: "1px solid rgba(118, 154, 198, 0.16)" },
          section: { background: "#05070b" },
        }}
      >
        <Space orientation="vertical" size="large" style={{ width: "100%" }}>
          {navLinks.map((link) => (
            <a key={link.label} href={link.href} onClick={() => setIsOpen(false)}>
              {link.label}
            </a>
          ))}
          <Space orientation="vertical" size="middle" style={{ width: "100%" }}>
            <Link href={isAuthenticated ? ROUTES.dashboard : ROUTES.signIn} onClick={() => setIsOpen(false)}>
              <Button block>{isAuthenticated ? "Dashboard" : "Sign in"}</Button>
            </Link>
            {isAuthenticated ? (
              <Button
                block
                type="primary"
                onClick={() => {
                  signOut();
                  setIsOpen(false);
                }}
              >
                Sign out
              </Button>
            ) : (
              <Link href={ROUTES.signUp} onClick={() => setIsOpen(false)}>
                <Button block type="primary">
                  Create account
                </Button>
              </Link>
            )}
          </Space>
        </Space>
      </Drawer>
    </div>
  );
}
