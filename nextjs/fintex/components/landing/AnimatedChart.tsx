"use client";

import { useEffect, useRef } from "react";
import { useStyles } from "./style";

interface Candle {
  open: number;
  high: number;
  low: number;
  close: number;
}

export function AnimatedChart() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const { styles } = useStyles();

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      return;
    }

    const context = canvas.getContext("2d");
    if (!context) {
      return;
    }

    const dpr = window.devicePixelRatio || 1;
    const candleWidth = 12;
    const candleGap = 8;
    const totalWidth = candleWidth + candleGap;
    const rightPadding = 28;
    const candleDuration = 2600;
    const basePrice = 1.0845;
    const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value));

    let historicalCandles: Candle[] = [];
    let activeCandle: Candle = { open: basePrice, high: basePrice, low: basePrice, close: basePrice };
    let candleStartedAt = performance.now();
    let lastFrame = performance.now();
    let drift = 0;
    let displayMin = basePrice - 0.0015;
    let displayMax = basePrice + 0.0015;

    const resizeCanvas = () => {
      canvas.width = Math.floor(canvas.offsetWidth * dpr);
      canvas.height = Math.floor(canvas.offsetHeight * dpr);
      context.setTransform(1, 0, 0, 1, 0, 0);
      context.scale(dpr, dpr);
    };

    const createHistoricalCandle = (currentPrice: number, phase: number): Candle => {
      const volatility = 0.0003;
      const cycle = Math.sin(phase * 0.58) * 0.00014;
      const bias = Math.cos(phase * 0.16 + 0.8) * 0.00008;
      const change = clamp(cycle + bias + (Math.random() - 0.5) * volatility, -0.00065, 0.00065);
      const close = currentPrice + change;
      const high = Math.max(currentPrice, close) + Math.random() * 0.0002;
      const low = Math.min(currentPrice, close) - Math.random() * 0.0002;

      return { open: currentPrice, high, low, close };
    };

    const createActiveCandle = (price: number): Candle => ({
      open: price,
      high: price,
      low: price,
      close: price,
    });

    const initializeCandles = () => {
      const width = canvas.width / dpr;
      const candleCount = Math.ceil(width / totalWidth) + 14;
      let currentPrice = basePrice;
      const seededCandles = Array.from({ length: candleCount }, (_, index) => {
        const candle = createHistoricalCandle(currentPrice, index + 1);
        currentPrice = candle.close;
        return candle;
      });

      historicalCandles = seededCandles.reverse();
      activeCandle = createActiveCandle(currentPrice);
      candleStartedAt = performance.now();
      lastFrame = candleStartedAt;
      displayMin = currentPrice - 0.0015;
      displayMax = currentPrice + 0.0015;
    };

    const priceToY = (price: number) => {
      const height = canvas.height / dpr;
      const topPadding = height * 0.12;
      const usableHeight = height - topPadding * 2;
      const range = Math.max(displayMax - displayMin, 0.0001);

      return height - topPadding - ((price - displayMin) / range) * usableHeight;
    };

    const updateViewport = () => {
      const visibleCandles = [activeCandle, ...historicalCandles.slice(0, 30)];
      const targetMin = Math.min(...visibleCandles.map((candle) => candle.low)) - 0.0005;
      const targetMax = Math.max(...visibleCandles.map((candle) => candle.high)) + 0.0005;
      displayMin += (targetMin - displayMin) * 0.08;
      displayMax += (targetMax - displayMax) * 0.08;
    };

    const updateActiveCandle = (deltaMs: number, elapsedMs: number) => {
      const macroTarget =
        basePrice +
        Math.sin((timestampSeed + elapsedMs) * 0.00014) * 0.0012 +
        Math.cos((timestampSeed + elapsedMs) * 0.00041) * 0.00052;
      const directionalPull = (macroTarget - activeCandle.close) * 0.085;
      const candleReversion = (activeCandle.open - activeCandle.close) * 0.05;
      const microWave = Math.sin((timestampSeed + elapsedMs) * 0.0018) * 0.000028;
      const noise = (Math.random() - 0.5) * 0.00003;
      drift = clamp(
        drift * 0.78 + directionalPull + candleReversion + microWave + noise,
        -0.00022,
        0.00022,
      );

      const nextClose = activeCandle.close + drift * (deltaMs / 16);
      const close = clamp(nextClose, activeCandle.open - 0.0012, activeCandle.open + 0.0012);

      activeCandle = {
        ...activeCandle,
        close,
        high: Math.max(activeCandle.high, close),
        low: Math.min(activeCandle.low, close),
      };
    };

    const drawCandle = (candle: Candle, x: number, isActive: boolean) => {
      const bullish = candle.close >= candle.open;
      const highY = priceToY(candle.high);
      const lowY = priceToY(candle.low);
      const openY = priceToY(candle.open);
      const closeY = priceToY(candle.close);
      const bodyTop = Math.min(openY, closeY);
      const bodyHeight = Math.max(Math.abs(closeY - openY), 2);
      const stroke = bullish ? "rgba(0, 214, 53, 0.92)" : "rgba(251, 31, 49, 0.84)";
      const fill = bullish ? "rgba(78, 172, 106, 0.18)" : "rgba(176, 74, 85, 0.16)";

      context.strokeStyle = stroke;
      context.lineWidth = isActive ? 1.7 : 1.25;
      context.beginPath();
      context.moveTo(x + candleWidth / 2, highY);
      context.lineTo(x + candleWidth / 2, lowY);
      context.stroke();

      context.fillStyle = fill;
      context.fillRect(x, bodyTop, candleWidth, bodyHeight);
      context.strokeRect(x, bodyTop, candleWidth, bodyHeight);

      if (isActive) {
        context.shadowBlur = 14;
        context.shadowColor = bullish ? "rgba(116, 203, 138, 0.26)" : "rgba(208, 94, 104, 0.24)";
        context.strokeRect(x, bodyTop, candleWidth, bodyHeight);
        context.shadowBlur = 0;
      }
    };

    const handleResize = () => {
      resizeCanvas();
      initializeCandles();
    };

    const timestampSeed = performance.now();

    handleResize();
    window.addEventListener("resize", handleResize);

    let animationFrame = 0;

    const draw = (timestamp: number) => {
      const width = canvas.width / dpr;
      const height = canvas.height / dpr;
      const deltaMs = Math.min(timestamp - lastFrame, 32);
      const elapsedMs = timestamp - candleStartedAt;
      const progress = clamp(elapsedMs / candleDuration, 0, 1);

      lastFrame = timestamp;

      updateActiveCandle(deltaMs, elapsedMs);
      updateViewport();

      if (progress >= 1) {
        historicalCandles.unshift({ ...activeCandle });
        historicalCandles = historicalCandles.slice(0, Math.ceil(width / totalWidth) + 16);
        activeCandle = createActiveCandle(activeCandle.close);
        candleStartedAt = timestamp;
      }

      context.clearRect(0, 0, width, height);

      context.strokeStyle = "rgba(126, 144, 130, 0.08)";
      context.lineWidth = 1;

      Array.from({ length: 7 }).forEach((_, index) => {
        const y = (height / 7) * index;
        context.beginPath();
        context.moveTo(0, y);
        context.lineTo(width, y);
        context.stroke();
      });

      Array.from({ length: 9 }).forEach((_, index) => {
        const x = (width / 9) * index;
        context.beginPath();
        context.moveTo(x, 0);
        context.lineTo(x, height);
        context.stroke();
      });

      const visibleHistoricalCandles = historicalCandles.slice(0, Math.ceil(width / totalWidth) + 8);

      visibleHistoricalCandles.forEach((candle, index) => {
        const slot = index + 1 + progress;
        const x = width - rightPadding - candleWidth - slot * totalWidth;
        drawCandle(candle, x, false);
      });

      const activeX = width - rightPadding - candleWidth - progress * totalWidth;
      drawCandle(activeCandle, activeX, true);

      const trendLinePoints = [activeCandle, ...visibleHistoricalCandles].slice(0, 18);
      context.strokeStyle = "rgba(168, 244, 187, 0.5)";
      context.lineWidth = 1.6;
      context.beginPath();

      trendLinePoints.forEach((candle, index) => {
        const windowCandles = trendLinePoints.slice(index, index + 4);
        const average =
          windowCandles.reduce((sum, item) => sum + (item.open + item.close) / 2, 0) /
          windowCandles.length;
        const slot = index === 0 ? progress : index + progress;
        const x = width - rightPadding - candleWidth / 2 - slot * totalWidth;
        const y = priceToY(average);

        if (index === 0) {
          context.moveTo(x, y);
        } else {
          context.lineTo(x, y);
        }
      });

      context.stroke();

      const priceY = priceToY(activeCandle.close);
      context.strokeStyle = "rgba(168, 244, 187, 0.2)";
      context.setLineDash([6, 6]);
      context.beginPath();
      context.moveTo(width * 0.38, priceY);
      context.lineTo(width, priceY);
      context.stroke();
      context.setLineDash([]);

      animationFrame = requestAnimationFrame(draw);
    };

    animationFrame = requestAnimationFrame(draw);

    return () => {
      cancelAnimationFrame(animationFrame);
      window.removeEventListener("resize", handleResize);
    };
  }, []);

  return <canvas ref={canvasRef} className={styles.chartCanvas} aria-hidden="true" />;
}
