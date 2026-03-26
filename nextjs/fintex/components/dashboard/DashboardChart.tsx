"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import {
  BulbOutlined,
  RadarChartOutlined,
  WalletOutlined,
} from "@ant-design/icons";
import { Alert, Button, Segmented, Space, Tag, Typography } from "antd";
import type { BinanceCandle, BinanceInterval } from "@/hooks/useBinanceChartData";
import { useBinanceChartData } from "@/hooks/useBinanceChartData";
import { formatPrice, formatSigned, formatTime } from "@/utils/market-data";
import { useStyles } from "./style";

export interface ChartTradeOverlay {
  id: string;
  direction: "Buy" | "Sell";
  entryPrice: number;
  stopLoss: number | null;
  takeProfit: number | null;
}

interface DashboardChartProps {
  symbol: string;
  venue: string;
  tradeOverlays: ChartTradeOverlay[];
  bid: number | null;
  ask: number | null;
  onOpenAccounts: () => void;
  onOpenRecommendation: () => void;
  onOpenBehaviorAnalysis: () => void;
  onOpenTrade: (direction: "Buy" | "Sell") => void;
}

interface CrosshairPoint {
  x: number;
  y: number;
}

interface OverlayLevel {
  key: string;
  label: string;
  price: number;
  color: string;
  dash: number[];
}

const intervals: BinanceInterval[] = ["1m", "5m", "15m", "1h", "4h"];
const defaultVisibleCount = 90;
const minimumVisibleCount = 32;
const maximumVisibleCount = 180;
const minimumVisualSpreadPercent = 0.0004;

const clampNumber = (value: number, min: number, max: number) =>
  Math.min(max, Math.max(min, value));

const formatAxisTimeLabel = (value: number, interval: BinanceInterval) =>
  new Intl.DateTimeFormat(undefined, {
    month: interval === "4h" ? "short" : undefined,
    day: interval === "4h" ? "numeric" : undefined,
    hour: "2-digit",
    minute: interval === "4h" ? undefined : "2-digit",
  }).format(new Date(value));

const buildAverageSeries = (candles: BinanceCandle[], period: number) =>
  candles.map((_, index) => {
    const start = Math.max(0, index - period + 1);
    const window = candles.slice(start, index + 1);
    const total = window.reduce((sum, item) => sum + item.close, 0);

    return total / window.length;
  });

export function DashboardChart({
  symbol,
  venue,
  tradeOverlays,
  bid,
  ask,
  onOpenAccounts,
  onOpenRecommendation,
  onOpenBehaviorAnalysis,
  onOpenTrade,
}: DashboardChartProps) {
  const { styles, cx } = useStyles();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const slotWidthRef = useRef(10);
  const dragStateRef = useRef({ isDragging: false, lastX: 0 });

  const [interval, setInterval] = useState<BinanceInterval>("5m");
  const [visibleCount, setVisibleCount] = useState(defaultVisibleCount);
  const [panOffset, setPanOffset] = useState(0);
  const [crosshair, setCrosshair] = useState<CrosshairPoint | null>(null);
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
  const isPositive = priceChange >= 0;

  const overlayLevels = useMemo<OverlayLevel[]>(
    () =>
      tradeOverlays.flatMap((position) => {
        const baseLevels: OverlayLevel[] = [
          {
            key: `${position.id}-entry`,
            label: `${position.direction} entry`,
            price: position.entryPrice,
            color: "#60a5fa",
            dash: [],
          },
        ];

        const stopLossLevel =
          position.stopLoss != null
            ? [
                {
                  key: `${position.id}-sl`,
                  label: "Stop loss",
                  price: position.stopLoss,
                  color: "#ff7875",
                  dash: [6, 4],
                },
              ]
            : [];

        const takeProfitLevel =
          position.takeProfit != null
            ? [
                {
                  key: `${position.id}-tp`,
                  label: "Take profit",
                  price: position.takeProfit,
                  color: "#7cf0a1",
                  dash: [6, 4],
                },
              ]
            : [];

        return [...baseLevels, ...stopLossLevel, ...takeProfitLevel];
      }),
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
    const leftPadding = 24;
    const rightPadding = 88;
    const topPadding = 28;
    const bottomPadding = 110;
    const volumeHeight = 46;

    const resizeCanvas = () => {
      canvas.width = Math.floor(canvas.offsetWidth * dpr);
      canvas.height = Math.floor(canvas.offsetHeight * dpr);
      context.setTransform(1, 0, 0, 1, 0, 0);
      context.scale(dpr, dpr);
      context.textBaseline = "middle";
      context.font = "12px sans-serif";
    };

    const drawLineSeries = (
      points: number[],
      color: string,
      priceToY: (value: number) => number,
      innerWidth: number,
    ) => {
      if (points.length < 2) {
        return;
      }

      context.strokeStyle = color;
      context.lineWidth = 1.2;
      context.beginPath();

      points.forEach((value, index) => {
        const x =
          leftPadding + index * (innerWidth / Math.max(points.length - 1, 1));
        const y = priceToY(value);

        if (index === 0) {
          context.moveTo(x, y);
          return;
        }

        context.lineTo(x, y);
      });

      context.stroke();
    };

    const drawAxisLabel = (text: string, x: number, y: number, color: string) => {
      const width = Math.max(context.measureText(text).width + 12, 64);
      context.fillStyle = "rgba(5, 6, 7, 0.96)";
      context.fillRect(x, y - 10, width, 20);
      context.strokeStyle = color;
      context.strokeRect(x, y - 10, width, 20);
      context.fillStyle = color;
      context.fillText(text, x + 6, y);
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
      const overlayPrices = [
        ...overlayLevels.map((item) => item.price),
        ...(visualSpreadBand ? [visualSpreadBand.bid, visualSpreadBand.ask] : []),
      ];

      const minPrice = Math.min(...lows, ...emaHighs, ...smaHighs, ...overlayPrices);
      const maxPrice = Math.max(...highs, ...emaHighs, ...smaHighs, ...overlayPrices);
      const maxVolume = Math.max(...visibleCandles.map((item) => item.volume), 1);
      const verticalPadding = (maxPrice - minPrice || 1) * 0.14;
      const paddedMin = minPrice - verticalPadding;
      const paddedMax = maxPrice + verticalPadding;
      const range = Math.max(paddedMax - paddedMin, 1);
      const candleSlotWidth = innerWidth / Math.max(visibleCandles.length, 1);
      const candleWidth = clampNumber(candleSlotWidth * 0.62, 4, 12);
      slotWidthRef.current = candleSlotWidth;

      const priceToY = (price: number) =>
        topPadding + ((paddedMax - price) / range) * chartHeight;
      const volumeToY = (volume: number) =>
        volumeTop + volumeHeight * (1 - volume / maxVolume);
      const yToPrice = (y: number) =>
        paddedMax - ((y - topPadding) / chartHeight) * range;

      context.strokeStyle = "rgba(255,255,255,0.05)";
      context.lineWidth = 1;

      Array.from({ length: 7 }).forEach((_, index) => {
        const y = topPadding + (chartHeight / 6) * index;
        const price = paddedMax - (range / 6) * index;

        context.beginPath();
        context.moveTo(leftPadding, y);
        context.lineTo(width - rightPadding, y);
        context.stroke();

        context.fillStyle = "rgba(255,255,255,0.45)";
        context.fillText(formatPrice(price), width - rightPadding + 8, y);
      });

      Array.from({ length: 6 }).forEach((_, index) => {
        const x = leftPadding + (innerWidth / 5) * index;
        context.beginPath();
        context.moveTo(x, topPadding);
        context.lineTo(x, height - bottomPadding);
        context.stroke();
      });

      const axisTickCount = Math.min(6, visibleCandles.length);
      let previousAxisRightEdge = -Infinity;

      Array.from({ length: axisTickCount }).forEach((_, index) => {
        const candleIndex = Math.round(
          ((visibleCandles.length - 1) * index) / Math.max(axisTickCount - 1, 1),
        );
        const candle = visibleCandles[candleIndex];
        if (!candle) {
          return;
        }

        const x = leftPadding + candleIndex * candleSlotWidth + candleSlotWidth / 2;
        const label = formatAxisTimeLabel(candle.closeTime, interval);
        const labelWidth = context.measureText(label).width;
        const labelX = clampNumber(
          x - labelWidth / 2,
          leftPadding,
          width - rightPadding - labelWidth,
        );

        if (labelX <= previousAxisRightEdge + 14) {
          return;
        }

        previousAxisRightEdge = labelX + labelWidth;

        context.strokeStyle = "rgba(255,255,255,0.16)";
        context.beginPath();
        context.moveTo(x, height - bottomPadding);
        context.lineTo(x, height - bottomPadding + 8);
        context.stroke();

        context.fillStyle = "rgba(255,255,255,0.52)";
        context.fillText(label, labelX, height - bottomPadding + 26);
      });

      visibleCandles.forEach((candle, index) => {
        const x =
          leftPadding + index * candleSlotWidth + (candleSlotWidth - candleWidth) / 2;
        const wickX = x + candleWidth / 2;
        const openY = priceToY(candle.open);
        const closeY = priceToY(candle.close);
        const highY = priceToY(candle.high);
        const lowY = priceToY(candle.low);
        const bodyY = Math.min(openY, closeY);
        const bodyHeight = Math.max(Math.abs(closeY - openY), 1.8);
        const isBullish = candle.close >= candle.open;

        context.strokeStyle = isBullish ? "#0bfc3f" : "#f40400";
        context.lineWidth = 1;
        context.beginPath();
        context.moveTo(wickX, highY);
        context.lineTo(wickX, lowY);
        context.stroke();

        context.fillStyle = isBullish
          ? "rgba(25, 255, 75, 0.88)"
          : "rgba(255, 23, 19, 0.82)";
        context.fillRect(x, bodyY, candleWidth, bodyHeight);

        context.strokeStyle = isBullish
          ? "rgba(19, 249, 76, 0.95)"
          : "rgba(246, 38, 27, 0.92)";
        context.strokeRect(x, bodyY, candleWidth, bodyHeight);

        const volumeY = volumeToY(candle.volume);
        context.fillStyle = isBullish
          ? "rgba(75, 225, 107, 0.18)"
          : "rgba(255, 120, 117, 0.16)";
        context.fillRect(x, volumeY, candleWidth, height - volumeY - 18);
      });

      drawLineSeries(emaSeries, "rgba(214, 244, 158, 0.88)", priceToY, innerWidth);
      drawLineSeries(smaSeries, "rgba(96, 165, 250, 0.78)", priceToY, innerWidth);

      if (visualSpreadBand) {
        const bandTop = priceToY(visualSpreadBand.ask);
        const bandBottom = priceToY(visualSpreadBand.bid);
        context.fillStyle = "rgba(168, 85, 247, 0.1)";
        context.fillRect(
          leftPadding,
          Math.min(bandTop, bandBottom),
          innerWidth,
          Math.max(Math.abs(bandBottom - bandTop), 1.2),
        );

        context.strokeStyle = "rgba(168, 85, 247, 0.48)";
        context.setLineDash([3, 4]);
        context.beginPath();
        context.moveTo(leftPadding, bandTop);
        context.lineTo(width - rightPadding, bandTop);
        context.moveTo(leftPadding, bandBottom);
        context.lineTo(width - rightPadding, bandBottom);
        context.stroke();
        context.setLineDash([]);
      }

      overlayLevels.forEach((level, index) => {
        const y = priceToY(level.price);
        const label = `${level.label}: ${formatPrice(level.price)}`;
        const textWidth = Math.max(context.measureText(label).width + 12, 110);
        const labelX = width - rightPadding - textWidth - 6;
        const labelY = y - 10 - index * 2;

        context.strokeStyle = level.color;
        context.lineWidth = 1.2;
        context.setLineDash(level.dash);
        context.beginPath();
        context.moveTo(leftPadding, y);
        context.lineTo(width - rightPadding, y);
        context.stroke();
        context.setLineDash([]);

        context.fillStyle = "rgba(5, 6, 7, 0.94)";
        context.fillRect(labelX, labelY, textWidth, 20);
        context.strokeStyle = level.color;
        context.strokeRect(labelX, labelY, textWidth, 20);
        context.fillStyle = level.color;
        context.fillText(label, labelX + 6, labelY + 10);
      });

      if (lastVisibleCandle) {
        const currentPriceY = priceToY(lastVisibleCandle.close);
        context.strokeStyle = "rgba(46, 249, 0, 0.68)";
        context.setLineDash([5, 5]);
        context.beginPath();
        context.moveTo(leftPadding, currentPriceY);
        context.lineTo(width - rightPadding, currentPriceY);
        context.stroke();
        context.setLineDash([]);

        drawAxisLabel(
          formatPrice(lastVisibleCandle.close),
          width - rightPadding + 6,
          currentPriceY,
          "#7cf0a1",
        );
      }

      if (
        crosshair &&
        crosshair.x >= leftPadding &&
        crosshair.x <= width - rightPadding &&
        crosshair.y >= topPadding &&
        crosshair.y <= height - bottomPadding
      ) {
        const hoverIndex = clampNumber(
          Math.floor((crosshair.x - leftPadding) / candleSlotWidth),
          0,
          visibleCandles.length - 1,
        );
        const hoveredCandle = visibleCandles[hoverIndex];
        const candleCenterX =
          leftPadding + hoverIndex * candleSlotWidth + candleSlotWidth / 2;
        const hoverPrice = yToPrice(crosshair.y);

        context.strokeStyle = "rgba(255,255,255,0.25)";
        context.setLineDash([4, 4]);
        context.beginPath();
        context.moveTo(candleCenterX, topPadding);
        context.lineTo(candleCenterX, height - bottomPadding);
        context.moveTo(leftPadding, crosshair.y);
        context.lineTo(width - rightPadding, crosshair.y);
        context.stroke();
        context.setLineDash([]);

        drawAxisLabel(
          formatPrice(hoverPrice),
          width - rightPadding + 6,
          crosshair.y,
          "#ffffff",
        );

        const timeLabel = formatTime(new Date(hoveredCandle.closeTime).toISOString());
        const timeWidth = Math.max(context.measureText(timeLabel).width + 12, 96);
        const timeX = clampNumber(
          candleCenterX - timeWidth / 2,
          leftPadding,
          width - rightPadding - timeWidth,
        );

        context.fillStyle = "rgba(5, 6, 7, 0.96)";
        context.fillRect(timeX, height - bottomPadding + 12, timeWidth, 20);
        context.strokeStyle = "rgba(255,255,255,0.18)";
        context.strokeRect(timeX, height - bottomPadding + 12, timeWidth, 20);
        context.fillStyle = "#ffffff";
        context.fillText(timeLabel, timeX + 6, height - bottomPadding + 22);

        const infoLines = [
          `O ${formatPrice(hoveredCandle.open)}`,
          `H ${formatPrice(hoveredCandle.high)}`,
          `L ${formatPrice(hoveredCandle.low)}`,
          `C ${formatPrice(hoveredCandle.close)}`,
        ];

        const infoWidth =
          Math.max(...infoLines.map((line) => context.measureText(line).width)) + 16;
        const infoHeight = 18 + infoLines.length * 16;

        context.fillStyle = "rgba(5, 6, 7, 0.94)";
        context.fillRect(leftPadding + 8, topPadding + 8, infoWidth, infoHeight);
        context.strokeStyle = "rgba(255,255,255,0.14)";
        context.strokeRect(leftPadding + 8, topPadding + 8, infoWidth, infoHeight);

        infoLines.forEach((line, index) => {
          context.fillStyle =
            index === 3
              ? hoveredCandle.close >= hoveredCandle.open
                ? "#7cf0a1"
                : "#ff7875"
              : "rgba(255,255,255,0.82)";
          context.fillText(line, leftPadding + 16, topPadding + 22 + index * 16);
        });
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
  }, [
    crosshair,
    emaSeries,
    interval,
    lastVisibleCandle,
    overlayLevels,
    smaSeries,
    visibleCandles,
    visualSpreadBand,
  ]);

  const handleWheel = (event: React.WheelEvent<HTMLCanvasElement>) => {
    event.preventDefault();

    setVisibleCount((current) =>
      clampNumber(
        current + (event.deltaY > 0 ? 8 : -8),
        minimumVisibleCount,
        Math.max(minimumVisibleCount, Math.min(maximumVisibleCount, candles.length || maximumVisibleCount)),
      ),
    );
  };

  const handleMouseMove = (event: React.MouseEvent<HTMLCanvasElement>) => {
    const rect = event.currentTarget.getBoundingClientRect();
    const nextPoint = {
      x: event.clientX - rect.left,
      y: event.clientY - rect.top,
    };

    if (dragStateRef.current.isDragging) {
      const deltaX = nextPoint.x - dragStateRef.current.lastX;
      const movedSteps = Math.round(deltaX / Math.max(slotWidthRef.current, 1));

      if (movedSteps !== 0) {
        setPanOffset((current) => clampNumber(current - movedSteps, 0, maxPanOffset));
        dragStateRef.current.lastX = nextPoint.x;
      }
    }

    setCrosshair(nextPoint);
  };

  const handleMouseDown = (event: React.MouseEvent<HTMLCanvasElement>) => {
    const rect = event.currentTarget.getBoundingClientRect();
    dragStateRef.current = {
      isDragging: true,
      lastX: event.clientX - rect.left,
    };
  };

  const releaseDrag = () => {
    dragStateRef.current.isDragging = false;
  };

  return (
    <div className={styles.terminal}>
      <div className={styles.header}>
        <div className={styles.symbolRow}>
          <div className={styles.symbolWrap}>
            <Typography.Text className={styles.symbol}>{symbol}</Typography.Text>
            <Typography.Text className={styles.price}>
              {formatPrice(lastVisibleCandle?.close)}
            </Typography.Text>
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
            {visualSpreadBand ? (
              <Tag color="purple">
                Spread {formatPrice(visualSpreadBand.width)}
                {visualSpreadBand.isSimulated ? " visual" : ""}
              </Tag>
            ) : null}
          </div>
          <Segmented
            options={intervals}
            value={interval}
            onChange={(value) => {
              setInterval(value as BinanceInterval);
              setVisibleCount(defaultVisibleCount);
              setPanOffset(0);
              setCrosshair(null);
            }}
          />
        </div>

        <div className={styles.actionBar}>
          <Space wrap size={10}>
            <Button
              type="primary"
              className={cx(styles.actionButton, styles.buyButton)}
              onClick={() => onOpenTrade("Buy")}
            >
              Buy
            </Button>
            <Button
              danger
              className={cx(styles.actionButton, styles.sellButton)}
              onClick={() => onOpenTrade("Sell")}
            >
              Sell
            </Button>
            <Button
              icon={<WalletOutlined />}
              className={styles.actionButton}
              onClick={onOpenAccounts}
            >
              Accounts
            </Button>
            <Button
              icon={<BulbOutlined />}
              className={styles.actionButton}
              onClick={onOpenRecommendation}
            >
              Get recommendation
            </Button>
            <Button
              icon={<RadarChartOutlined />}
              className={styles.actionButton}
              onClick={onOpenBehaviorAnalysis}
            >
              My behavior analysis
            </Button>
          </Space>

        </div>

        {lastVisibleCandle ? (
          <div className={styles.liveMetaRow}>
            <Typography.Text type="secondary">
              Last candle close: {formatTime(new Date(lastVisibleCandle.closeTime).toISOString())}
            </Typography.Text>
            <Typography.Text type="secondary">Interval: {interval}</Typography.Text>
          </div>
        ) : null}
      </div>

      {error ? (
        <div className={styles.errorWrap}>
          <Alert type="warning" showIcon title={error} />
        </div>
      ) : null}

      <div className={styles.canvasWrap}>
        <canvas
          ref={canvasRef}
          className={styles.chartCanvas}
          aria-label="Interactive BTCUSDT chart"
          onWheel={handleWheel}
          onMouseMove={handleMouseMove}
          onMouseDown={handleMouseDown}
          onMouseUp={releaseDrag}
          onMouseLeave={() => {
            releaseDrag();
            setCrosshair(null);
          }}
        />
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
          <span className={styles.legendItem}>
            <span className={styles.legendDotEntry} />
            Entry / SL / TP
          </span>
          <span className={styles.legendItem}>
            <span className={styles.legendDotSpread} />
            Spread
          </span>
        </div>
        <Typography.Text type="secondary">
          Wheel to zoom, drag to pan, move to inspect price and candle detail.
        </Typography.Text>
      </div>
    </div>
  );
}
