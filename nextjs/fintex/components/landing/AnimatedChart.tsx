"use client";

import { useEffect, useRef } from "react";
import { useStyles } from "./style";

interface Candle {
  x: number;
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

    const resizeCanvas = () => {
      canvas.width = canvas.offsetWidth;
      canvas.height = canvas.offsetHeight;
    };

    resizeCanvas();
    window.addEventListener("resize", resizeCanvas);

    const candleWidth = 18;
    const candleGap = 10;
    const totalWidth = candleWidth + candleGap;
    const basePrice = 1.0845;
    let frameTime = 0;
    let candles: Candle[] = [];

    const createCandle = (x: number, currentPrice: number) => {
      const volatility = 0.0014;
      const trend = Math.sin(frameTime * 0.02) * 0.0006;
      const open = currentPrice;
      const change = (Math.random() - 0.46) * volatility + trend;
      const close = open + change;
      const high = Math.max(open, close) + Math.random() * volatility * 0.7;
      const low = Math.min(open, close) - Math.random() * volatility * 0.7;

      return { x, open, high, low, close };
    };

    const initializeCandles = () => {
      candles = [];
      const candleCount = Math.ceil(canvas.width / totalWidth) + 4;
      let currentPrice = basePrice;

      Array.from({ length: candleCount }).forEach((_, index) => {
        const candle = createCandle(canvas.width - index * totalWidth, currentPrice);
        candles.push(candle);
        currentPrice = candle.close;
      });
    };

    const priceToY = (price: number) => {
      const min = Math.min(...candles.map((candle) => candle.low)) - 0.002;
      const max = Math.max(...candles.map((candle) => candle.high)) + 0.002;
      const range = max - min;
      const topPadding = canvas.height * 0.12;
      const usableHeight = canvas.height - topPadding * 2;

      return canvas.height - topPadding - ((price - min) / range) * usableHeight;
    };

    initializeCandles();

    let animationFrame = 0;
    let lastCandleTimestamp = Date.now();

    const draw = () => {
      frameTime += 1;
      context.clearRect(0, 0, canvas.width, canvas.height);

      context.strokeStyle = "rgba(84, 255, 214, 0.08)";
      context.lineWidth = 1;

      Array.from({ length: 8 }).forEach((_, index) => {
        const y = (canvas.height / 8) * index;
        context.beginPath();
        context.moveTo(0, y);
        context.lineTo(canvas.width, y);
        context.stroke();
      });

      Array.from({ length: 10 }).forEach((_, index) => {
        const x = (canvas.width / 10) * index;
        context.beginPath();
        context.moveTo(x, 0);
        context.lineTo(x, canvas.height);
        context.stroke();
      });

      candles = candles.map((candle) => ({ ...candle, x: candle.x - 0.45 }));

      if (Date.now() - lastCandleTimestamp > 2200 || candles[0]?.x < -totalWidth) {
        lastCandleTimestamp = Date.now();
        candles = candles.filter((candle) => candle.x > -totalWidth);
        const latestClose = candles[0]?.close ?? basePrice;
        candles.unshift(createCandle(canvas.width + totalWidth, latestClose));
      }

      candles.forEach((candle, index) => {
        const bullish = candle.close >= candle.open;
        const highY = priceToY(candle.high);
        const lowY = priceToY(candle.low);
        const openY = priceToY(candle.open);
        const closeY = priceToY(candle.close);
        const bodyTop = Math.min(openY, closeY);
        const bodyHeight = Math.max(Math.abs(closeY - openY), 2);

        context.strokeStyle = bullish ? "rgba(52, 245, 197, 0.9)" : "rgba(255, 96, 128, 0.88)";
        context.lineWidth = 1.5;
        context.beginPath();
        context.moveTo(candle.x + candleWidth / 2, highY);
        context.lineTo(candle.x + candleWidth / 2, lowY);
        context.stroke();

        context.fillStyle = bullish ? "rgba(52, 245, 197, 0.18)" : "rgba(255, 96, 128, 0.18)";
        context.fillRect(candle.x, bodyTop, candleWidth, bodyHeight);
        context.strokeRect(candle.x, bodyTop, candleWidth, bodyHeight);

        if (index === 0) {
          context.shadowBlur = 18;
          context.shadowColor = bullish ? "rgba(52, 245, 197, 0.6)" : "rgba(255, 96, 128, 0.5)";
          context.strokeRect(candle.x, bodyTop, candleWidth, bodyHeight);
          context.shadowBlur = 0;
        }
      });

      context.strokeStyle = "rgba(121, 199, 255, 0.45)";
      context.lineWidth = 2;
      context.beginPath();

      candles.slice(0, 24).forEach((candle, index) => {
        const windowCandles = candles.slice(index, index + 5);
        const average = windowCandles.reduce((sum, item) => sum + (item.open + item.close) / 2, 0) / windowCandles.length;
        const x = candle.x + candleWidth / 2;
        const y = priceToY(average);

        if (index === 0) {
          context.moveTo(x, y);
        } else {
          context.lineTo(x, y);
        }
      });

      context.stroke();
      animationFrame = requestAnimationFrame(draw);
    };

    draw();

    return () => {
      cancelAnimationFrame(animationFrame);
      window.removeEventListener("resize", resizeCanvas);
    };
  }, []);

  return <canvas ref={canvasRef} className={styles.chartCanvas} aria-hidden="true" />;
}
