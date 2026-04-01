"use client";

import type { BinanceCandle, BinanceInterval } from "@/hooks/useBinanceChartData";

export interface ChartTradeOverlay {
  id: string;
  direction: "Buy" | "Sell";
  entryPrice: number;
  stopLoss: number | null;
  takeProfit: number | null;
}

export interface DashboardChartProps {
  symbol: string;
  venue: string;
  tradeOverlays: ChartTradeOverlay[];
  bid: number | null;
  ask: number | null;
  onOpenAccounts: () => void;
  onOpenRecommendation: () => void;
  onOpenBehaviorAnalysis: () => void;
  onOpenStrategyValidation: () => void;
  onOpenTrade: (direction: "Buy" | "Sell") => void;
}

export interface CrosshairPoint {
  x: number;
  y: number;
}

export interface OverlayLevel {
  key: string;
  label: string;
  price: number;
  color: string;
  dash: number[];
}

export interface DashboardChartController {
  candles: BinanceCandle[];
  canvasRef: React.RefObject<HTMLCanvasElement | null>;
  crosshair: CrosshairPoint | null;
  emaSeries: number[];
  error: string | null;
  handleMouseDown: (event: React.MouseEvent<HTMLCanvasElement>) => void;
  handleMouseMove: (event: React.MouseEvent<HTMLCanvasElement>) => void;
  handleMouseLeave: () => void;
  handleMouseUp: () => void;
  handleWheel: (event: WheelEvent) => void;
  interval: BinanceInterval;
  isPositive: boolean;
  lastVisibleCandle: BinanceCandle | null;
  latestCandle: BinanceCandle | null;
  overlayLevels: OverlayLevel[];
  panOffset: number;
  priceChange: number;
  setInterval: (interval: BinanceInterval) => void;
  smaSeries: number[];
  status: "loading" | "live" | "reconnecting" | "error" | "offline";
  visibleCandles: BinanceCandle[];
  visualSpreadBand: {
    bid: number;
    ask: number;
    width: number;
    isSimulated: boolean;
  } | null;
}

export const intervals: BinanceInterval[] = ["1m", "5m", "15m", "1h", "4h"];
