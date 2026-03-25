"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Alert, Segmented, Tag, Typography } from "antd";
import type { BinanceCandle, BinanceInterval } from "@/hooks/useBinanceChartData";
import { useBinanceChartData } from "@/hooks/useBinanceChartData";
import { formatCompact, formatPrice, formatSigned, formatTime } from "@/utils/market-data";
import { useStyles } from "./style";

interface DashboardChartProps {
  symbol: string;
  venue: string;
}

const intervals: BinanceInterval[] = ["1m", "5m", "15m", "1h", "4h"];

const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value));

const buildAverageSeries = (candles: BinanceCandle[], period: number) =>
  candles.map((_, index) => {
    const start = Math.max(0, index - period + 1);
    const window = candles.slice(start, index + 1);
    const total = window.reduce((sum, item) => sum + item.close, 0);

    return total / window.length;
  });

export function DashboardChart({ symbol, venue }: DashboardChartProps) {
  const { styles, cx } = useStyles();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [interval, setInterval] = useState<BinanceInterval>("5m");
  const { candles, error, latestCandle, status } = useBinanceChartData(symbol, interval);

  const visibleCandles = useMemo(() => candles.slice(-90), [candles]);
  const emaSeries = useMemo(() => buildAverageSeries(visibleCandles, 9), [visibleCandles]);
  const smaSeries = useMemo(() => buildAverageSeries(visibleCandles, 20), [visibleCandles]);

  const firstCandle = visibleCandles[0] ?? null;
  const priceChange =
    firstCandle && latestCandle
      ? ((latestCandle.close - firstCandle.open) / firstCandle.open) * 100
      : 0;
  const isPositive = priceChange >= 0;

  const statTiles = useMemo(() => {
    if (visibleCandles.length === 0) {
      return [
        { label: "Open", value: "-", tone: "neutral" },
        { label: "High", value: "-", tone: "neutral" },
        { label: "Low", value: "-", tone: "neutral" },
        { label: "Volume", value: "-", tone: "neutral" },
      ];
    }

    const highs = visibleCandles.map((item) => item.high);
    const lows = visibleCandles.map((item) => item.low);
    const totalVolume = visibleCandles.reduce((sum, item) => sum + item.volume, 0);

    return [
      { label: "Open", value: formatPrice(visibleCandles[0]?.open), tone: "neutral" },
      { label: "High", value: formatPrice(Math.max(...highs)), tone: "positive" },
      { label: "Low", value: formatPrice(Math.min(...lows)), tone: "negative" },
      { label: "Volume", value: formatCompact(totalVolume), tone: "neutral" },
    ];
  }, [visibleCandles]);

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
    const rightPadding = 24;
    const leftPadding = 24;
    const topPadding = 26;
    const bottomPadding = 108;
    const volumeHeight = 74;

    const resizeCanvas = () => {
      canvas.width = Math.floor(canvas.offsetWidth * dpr);
      canvas.height = Math.floor(canvas.offsetHeight * dpr);
      context.setTransform(1, 0, 0, 1, 0, 0);
      context.scale(dpr, dpr);
    };

    const drawLineSeries = (
      points: number[],
      color: string,
      priceToY: (value: number) => number,
      width: number,
      innerWidth: number,
    ) => {
      if (points.length < 2) {
        return;
      }

      context.strokeStyle = color;
      context.lineWidth = 1.2;
      context.beginPath();

      points.forEach((value, index) => {
        const x = leftPadding + index * (innerWidth / Math.max(points.length - 1, 1));
        const y = priceToY(value);

        if (index === 0) {
          context.moveTo(x, y);
          return;
        }

        context.lineTo(x, y);
      });

      context.stroke();
    };

    const drawChart = () => {
      const width = canvas.width / dpr;
      const height = canvas.height / dpr;
      const chartHeight = height - topPadding - bottomPadding;
      const volumeTop = height - volumeHeight - 18;
      const innerWidth = width - leftPadding - rightPadding;

      context.clearRect(0, 0, width, height);

      if (visibleCandles.length === 0) {
        return;
      }

      const highs = visibleCandles.map((item) => item.high);
      const lows = visibleCandles.map((item) => item.low);
      const emaHighs = emaSeries.length > 0 ? emaSeries : highs;
      const smaHighs = smaSeries.length > 0 ? smaSeries : highs;
      const minPrice = Math.min(...lows, ...emaHighs, ...smaHighs);
      const maxPrice = Math.max(...highs, ...emaHighs, ...smaHighs);
      const maxVolume = Math.max(...visibleCandles.map((item) => item.volume), 1);
      const verticalPadding = (maxPrice - minPrice || 1) * 0.14;
      const paddedMin = minPrice - verticalPadding;
      const paddedMax = maxPrice + verticalPadding;
      const range = Math.max(paddedMax - paddedMin, 1);
      const candleSlotWidth = innerWidth / Math.max(visibleCandles.length, 1);
      const candleWidth = clamp(candleSlotWidth * 0.62, 4, 12);

      const priceToY = (price: number) =>
        topPadding + ((paddedMax - price) / range) * chartHeight;
      const volumeToY = (volume: number) =>
        volumeTop + volumeHeight * (1 - volume / maxVolume);

      context.strokeStyle = "rgba(255,255,255,0.05)";
      context.lineWidth = 1;

      Array.from({ length: 7 }).forEach((_, index) => {
        const y = topPadding + (chartHeight / 6) * index;
        context.beginPath();
        context.moveTo(leftPadding, y);
        context.lineTo(width - rightPadding, y);
        context.stroke();
      });

      Array.from({ length: 6 }).forEach((_, index) => {
        const x = leftPadding + (innerWidth / 5) * index;
        context.beginPath();
        context.moveTo(x, topPadding);
        context.lineTo(x, height - bottomPadding);
        context.stroke();
      });

      visibleCandles.forEach((candle, index) => {
        const x = leftPadding + index * candleSlotWidth + (candleSlotWidth - candleWidth) / 2;
        const wickX = x + candleWidth / 2;
        const openY = priceToY(candle.open);
        const closeY = priceToY(candle.close);
        const highY = priceToY(candle.high);
        const lowY = priceToY(candle.low);
        const bodyY = Math.min(openY, closeY);
        const bodyHeight = Math.max(Math.abs(closeY - openY), 1.8);
        const isBullish = candle.close >= candle.open;

        context.strokeStyle = isBullish ? "#4be16b" : "#ff7875";
        context.lineWidth = 1;
        context.beginPath();
        context.moveTo(wickX, highY);
        context.lineTo(wickX, lowY);
        context.stroke();

        context.fillStyle = isBullish ? "rgba(75, 225, 107, 0.88)" : "rgba(255, 120, 117, 0.82)";
        context.fillRect(x, bodyY, candleWidth, bodyHeight);

        context.strokeStyle = isBullish ? "rgba(155, 242, 177, 0.95)" : "rgba(255, 181, 177, 0.92)";
        context.strokeRect(x, bodyY, candleWidth, bodyHeight);

        const volumeY = volumeToY(candle.volume);
        context.fillStyle = isBullish ? "rgba(75, 225, 107, 0.18)" : "rgba(255, 120, 117, 0.16)";
        context.fillRect(x, volumeY, candleWidth, height - volumeY - 18);
      });

      drawLineSeries(emaSeries, "rgba(214, 244, 158, 0.88)", priceToY, width, innerWidth);
      drawLineSeries(smaSeries, "rgba(96, 165, 250, 0.78)", priceToY, width, innerWidth);

      if (latestCandle) {
        const currentPriceY = priceToY(latestCandle.close);
        context.strokeStyle = "rgba(75, 225, 107, 0.24)";
        context.setLineDash([5, 5]);
        context.beginPath();
        context.moveTo(leftPadding, currentPriceY);
        context.lineTo(width - rightPadding, currentPriceY);
        context.stroke();
        context.setLineDash([]);
      }

      const fadeWidth = 56;
      const leftFade = context.createLinearGradient(0, 0, fadeWidth, 0);
      leftFade.addColorStop(0, "rgba(5, 6, 7, 1)");
      leftFade.addColorStop(1, "rgba(5, 6, 7, 0)");
      context.fillStyle = leftFade;
      context.fillRect(0, topPadding, fadeWidth, chartHeight);
    };

    const handleResize = () => {
      resizeCanvas();
      drawChart();
    };

    handleResize();
    window.addEventListener("resize", handleResize);

    return () => {
      window.removeEventListener("resize", handleResize);
    };
  }, [emaSeries, latestCandle, smaSeries, visibleCandles]);

  return (
    <div className={styles.terminal}>
      <div className={styles.header}>
        <div className={styles.symbolRow}>
          <div className={styles.symbolWrap}>
            <Typography.Text className={styles.symbol}>{symbol}</Typography.Text>
            <Typography.Text className={styles.price}>{formatPrice(latestCandle?.close)}</Typography.Text>
            <Tag color={isPositive ? "green" : "red"}>{formatSigned(priceChange)}%</Tag>
            <Tag>{venue}</Tag>
            <Tag
              color={
                status === "live"
                  ? "green"
                  : status === "reconnecting" || status === "loading"
                    ? "gold"
                    : "red"
              }
            >
              {status}
            </Tag>
          </div>
          <Segmented
            options={intervals}
            value={interval}
            onChange={(value) => setInterval(value as BinanceInterval)}
          />
        </div>

        <div className={styles.metaRow}>
          {statTiles.map((item) => (
            <div key={item.label} className={styles.statTile}>
              <div className={styles.statLabel}>{item.label}</div>
              <div
                className={cx(
                  styles.statValue,
                  item.tone === "positive" ? styles.positive : undefined,
                  item.tone === "negative" ? styles.negative : undefined,
                )}
              >
                {item.value}
              </div>
            </div>
          ))}
        </div>

        {latestCandle ? (
          <div className={styles.liveMetaRow}>
            <Typography.Text type="secondary">
              Last candle close: {formatTime(new Date(latestCandle.closeTime).toISOString())}
            </Typography.Text>
            <Typography.Text type="secondary">
              Interval: {interval}
            </Typography.Text>
          </div>
        ) : null}
      </div>

      {error ? (
        <div className={styles.errorWrap}>
          <Alert type="warning" showIcon message={error} />
        </div>
      ) : null}

      <div className={styles.canvasWrap}>
        <canvas ref={canvasRef} className={styles.chartCanvas} aria-hidden="true" />
      </div>

      <div className={styles.footerBar}>
        <div className={styles.legend}>
          <span className={styles.legendItem}>
            <span className={styles.legendDotBull} />
            Bull candles
          </span>
          <span className={styles.legendItem}>
            <span className={styles.legendDotBear} />
            EMA 9
          </span>
          <span className={styles.legendItem}>
            <span className={styles.legendDotSignal} />
            SMA 20
          </span>
        </div>
        <Typography.Text type="secondary">
          Direct Binance klines for the chart, backend analytics for the signal stack.
        </Typography.Text>
      </div>
    </div>
  );
}
