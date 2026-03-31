"use client";

import { useMemo, useRef, useState } from "react";
import type { BinanceInterval } from "@/hooks/useBinanceChartData";
import { useBinanceChartData } from "@/hooks/useBinanceChartData";
import {
  buildAverageSeries,
  clampNumber,
  defaultVisibleCount,
  maximumVisibleCount,
  minimumVisualSpreadPercent,
  minimumVisibleCount,
} from "./chart-helpers";
import type {
  ChartTradeOverlay,
  DashboardChartController,
  DashboardChartProps,
  OverlayLevel,
} from "./types";

export const useDashboardChartController = ({
  ask,
  bid,
  symbol,
  tradeOverlays,
}: Pick<DashboardChartProps, "ask" | "bid" | "symbol" | "tradeOverlays">): DashboardChartController => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const slotWidthRef = useRef(10);
  const dragStateRef = useRef({ isDragging: false, lastX: 0 });
  const [interval, setChartInterval] = useState<BinanceInterval>("5m");
  const [visibleCount, setVisibleCount] = useState(defaultVisibleCount);
  const [panOffset, setPanOffset] = useState(0);
  const [crosshair, setCrosshair] = useState<{ x: number; y: number } | null>(null);
  const { candles, error, latestCandle, status } = useBinanceChartData(symbol, interval);
  const boundedVisibleCount = Math.min(
    Math.max(visibleCount, minimumVisibleCount),
    Math.max(minimumVisibleCount, Math.min(maximumVisibleCount, candles.length || defaultVisibleCount)),
  );
  const maxPanOffset = Math.max(0, candles.length - boundedVisibleCount);
  const effectivePanOffset = Math.min(panOffset, maxPanOffset);

  const visibleCandles = useMemo(() => {
    if (candles.length === 0) {
      return [];
    }

    const endIndex = Math.max(candles.length - effectivePanOffset, 0);
    const startIndex = Math.max(0, endIndex - boundedVisibleCount);
    return candles.slice(startIndex, endIndex);
  }, [boundedVisibleCount, candles, effectivePanOffset]);

  const emaSeries = useMemo(() => buildAverageSeries(visibleCandles, 9), [visibleCandles]);
  const smaSeries = useMemo(() => buildAverageSeries(visibleCandles, 20), [visibleCandles]);
  const firstCandle = visibleCandles[0] ?? null;
  const lastVisibleCandle = visibleCandles[visibleCandles.length - 1] ?? latestCandle ?? null;
  const priceChange =
    firstCandle && lastVisibleCandle
      ? ((lastVisibleCandle.close - firstCandle.open) / firstCandle.open) * 100
      : 0;

  const overlayLevels = useMemo<OverlayLevel[]>(
    () =>
      tradeOverlays.flatMap((position: ChartTradeOverlay) => [
        { key: `${position.id}-entry`, label: `${position.direction} entry`, price: position.entryPrice, color: "#60a5fa", dash: [] },
        ...(position.stopLoss != null ? [{ key: `${position.id}-sl`, label: "Stop loss", price: position.stopLoss, color: "#ff7875", dash: [6, 4] }] : []),
        ...(position.takeProfit != null ? [{ key: `${position.id}-tp`, label: "Take profit", price: position.takeProfit, color: "#7cf0a1", dash: [6, 4] }] : []),
      ]),
    [tradeOverlays],
  );

  const visualSpreadBand = useMemo(() => {
    const anchorPrice = lastVisibleCandle?.close ?? latestCandle?.close ?? null;
    if (anchorPrice == null) {
      return null;
    }

    const actualSpread = bid != null && ask != null && ask >= bid ? ask - bid : 0;
    const fallbackSpread = anchorPrice * minimumVisualSpreadPercent;
    const spreadWidth = Math.max(actualSpread, fallbackSpread);

    return {
      bid: anchorPrice - spreadWidth / 2,
      ask: anchorPrice + spreadWidth / 2,
      width: spreadWidth,
      isSimulated: actualSpread < fallbackSpread,
    };
  }, [ask, bid, lastVisibleCandle, latestCandle]);

  return {
    candles,
    canvasRef,
    crosshair,
    emaSeries,
    error,
    handleMouseDown: (event) => {
      const rect = event.currentTarget.getBoundingClientRect();
      dragStateRef.current = { isDragging: true, lastX: event.clientX - rect.left };
    },
    handleMouseMove: (event) => {
      const rect = event.currentTarget.getBoundingClientRect();
      const nextPoint = { x: event.clientX - rect.left, y: event.clientY - rect.top };

      if (dragStateRef.current.isDragging) {
        const deltaX = nextPoint.x - dragStateRef.current.lastX;
        const movedSteps = Math.round(deltaX / Math.max(slotWidthRef.current, 1));
        if (movedSteps !== 0) {
          setPanOffset((current) => clampNumber(current - movedSteps, 0, maxPanOffset));
          dragStateRef.current.lastX = nextPoint.x;
        }
      }

      setCrosshair(nextPoint);
    },
    handleMouseLeave: () => {
      dragStateRef.current.isDragging = false;
      setCrosshair(null);
    },
    handleMouseUp: () => {
      dragStateRef.current.isDragging = false;
    },
    handleWheel: (event) => {
      event.preventDefault();
      setVisibleCount((current) =>
        clampNumber(
          current + (event.deltaY > 0 ? 8 : -8),
          minimumVisibleCount,
          Math.max(minimumVisibleCount, Math.min(maximumVisibleCount, candles.length || maximumVisibleCount)),
        ),
      );
    },
    interval,
    isPositive: priceChange >= 0,
    lastVisibleCandle,
    latestCandle,
    overlayLevels,
    panOffset: effectivePanOffset,
    priceChange,
    setInterval: (nextInterval) => {
      setChartInterval(nextInterval);
      setVisibleCount(defaultVisibleCount);
      setPanOffset(0);
      setCrosshair(null);
    },
    smaSeries,
    status,
    visibleCandles,
    visualSpreadBand,
  };
};
