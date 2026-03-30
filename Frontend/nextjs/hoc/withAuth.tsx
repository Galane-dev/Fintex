"use client";

import { ComponentType, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { Alert, Flex, Spin } from "antd";
import { useAuth } from "@/hooks/useAuth";
import { useAcademyStatus } from "@/hooks/use-academy-status";
import { ROUTES } from "@/constants/routes";

interface WithAuthOptions {
  requireAcademyAccess?: boolean;
}

export const withAuth = <P extends object>(
  WrappedComponent: ComponentType<P>,
  options?: WithAuthOptions,
) => {
  const requireAcademyAccess = options?.requireAcademyAccess ?? true;
  const ProtectedComponent = (props: P) => {
    const router = useRouter();
    const pathname = usePathname();
    const { isReady, isAuthenticated } = useAuth();
    const academy = useAcademyStatus({
      enabled: isReady && isAuthenticated && requireAcademyAccess,
    });

    useEffect(() => {
      if (isReady && !isAuthenticated) {
        const redirect = pathname ? `?redirect=${encodeURIComponent(pathname)}` : "";
        router.replace(`${ROUTES.signIn}${redirect}`);
      }
    }, [isAuthenticated, isReady, pathname, router]);

    useEffect(() => {
      if (!requireAcademyAccess || !isReady || !isAuthenticated || academy.isLoading) {
        return;
      }

      if (academy.status && !academy.status.hasTradeAcademyAccess) {
        router.replace(ROUTES.academy);
      }
    }, [
      academy.isLoading,
      academy.status,
      isAuthenticated,
      isReady,
      router,
    ]);

    if (!isReady || !isAuthenticated || (requireAcademyAccess && academy.isLoading)) {
      return (
        <Flex align="center" justify="center" style={{ minHeight: "100vh" }}>
          <Spin size="large" />
        </Flex>
      );
    }

    if (requireAcademyAccess && academy.error && !academy.status) {
      return (
        <Flex align="center" justify="center" style={{ minHeight: "100vh", padding: 24 }}>
          <Alert type="error" showIcon message={academy.error} />
        </Flex>
      );
    }

    if (requireAcademyAccess && academy.status && !academy.status.hasTradeAcademyAccess) {
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
