"use client";

import { withAuth } from "@/hoc/withAuth";
import { ExternalBrokerProvider } from "@/providers/external-broker-provider";
import { LiveTradingProvider } from "@/providers/live-trading-provider";
import { MarketDataProvider } from "@/providers/market-data-provider";
import { NotificationsProvider } from "@/providers/notifications-provider";
import { PaperTradingProvider } from "@/providers/paper-trading-provider";
import { DashboardContent } from "./dashboard-content";

function DashboardViewContent() {
  return (
    <MarketDataProvider>
      <ExternalBrokerProvider>
        <NotificationsProvider>
          <PaperTradingProvider>
            <LiveTradingProvider>
              <DashboardContent />
            </LiveTradingProvider>
          </PaperTradingProvider>
        </NotificationsProvider>
      </ExternalBrokerProvider>
    </MarketDataProvider>
  );
}

export const DashboardView = withAuth(DashboardViewContent);
