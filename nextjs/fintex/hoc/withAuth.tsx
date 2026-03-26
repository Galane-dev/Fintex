"use client";

import { ComponentType, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { Flex, Spin } from "antd";
import { useAuth } from "@/hooks/useAuth";
import { ROUTES } from "@/constants/routes";

export const withAuth = <P extends object>(WrappedComponent: ComponentType<P>) => {
  const ProtectedComponent = (props: P) => {
    const router = useRouter();
    const pathname = usePathname();
    const { isReady, isAuthenticated } = useAuth();

    useEffect(() => {
      if (isReady && !isAuthenticated) {
        const redirect = pathname ? `?redirect=${encodeURIComponent(pathname)}` : "";
        router.replace(`${ROUTES.signIn}${redirect}`);
      }
    }, [isAuthenticated, isReady, pathname, router]);

    if (!isReady || !isAuthenticated) {
      return (
        <Flex align="center" justify="center" style={{ minHeight: "100vh" }}>
          <Spin size="large" />
        </Flex>
      );
    }

    return <WrappedComponent {...props} />;
  };

  ProtectedComponent.displayName = `withAuth(${WrappedComponent.displayName ?? WrappedComponent.name ?? "Component"})`;

  return ProtectedComponent;
};
