"use client";

import type { BinanceCandle, BinanceInterval } from "@/hooks/useBinanceChartData";

export const defaultVisibleCount = 90;
export const minimumVisibleCount = 32;
export const maximumVisibleCount = 180;
export const minimumVisualSpreadPercent = 0.0004;

export const clampNumber = (value: number, min: number, max: number) =>
  Math.min(max, Math.max(min, value));

export const formatAxisTimeLabel = (value: number, interval: BinanceInterval) =>
  new Intl.DateTimeFormat(undefined, {
    month: interval === "4h" ? "short" : undefined,
    day: interval === "4h" ? "numeric" : undefined,
    hour: "2-digit",
    minute: interval === "4h" ? undefined : "2-digit",
  }).format(new Date(value));

export const buildAverageSeries = (candles: BinanceCandle[], period: number) =>
  candles.map((_, index) => {
    const start = Math.max(0, index - period + 1);
    const window = candles.slice(start, index + 1);
    const total = window.reduce((sum, item) => sum + item.close, 0);

    return total / window.length;
  });
