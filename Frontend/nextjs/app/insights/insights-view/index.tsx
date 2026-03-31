"use client";

import { withAuth } from "@/hoc/withAuth";
import { ExternalBrokerProvider } from "@/providers/external-broker-provider";
import { LiveTradingProvider } from "@/providers/live-trading-provider";
import { MarketDataProvider } from "@/providers/market-data-provider";
import { NotificationsProvider } from "@/providers/notifications-provider";
import { PaperTradingProvider } from "@/providers/paper-trading-provider";
import { InsightsContent } from "./insights-content";

function InsightsViewContent() {
  return (
    <MarketDataProvider>
      <ExternalBrokerProvider>
        <NotificationsProvider>
          <PaperTradingProvider>
            <LiveTradingProvider>
              <InsightsContent />
            </LiveTradingProvider>
          </PaperTradingProvider>
        </NotificationsProvider>
      </ExternalBrokerProvider>
    </MarketDataProvider>
  );
}

export const InsightsView = withAuth(InsightsViewContent);
