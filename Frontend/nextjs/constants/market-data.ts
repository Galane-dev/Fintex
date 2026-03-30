import type { MarketDataProvider, MarketSelection } from "@/types/market-data";

export const MARKET_PROVIDER_LABELS: Record<MarketDataProvider, string> = {
  1: "Binance",
  2: "Coinbase",
  3: "Oanda",
};

export const MARKET_OPTIONS: MarketSelection[] = [
  {
    key: "binance-btcusdt",
    label: "BTC / USDT",
    symbol: "BTCUSDT",
    provider: 1,
    venue: "Binance",
  },
];

export const DEFAULT_MARKET = MARKET_OPTIONS[0];
