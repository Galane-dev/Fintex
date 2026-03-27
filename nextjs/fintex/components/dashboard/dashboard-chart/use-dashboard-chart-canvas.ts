"use client";

import { useEffect, useRef } from "react";
import { formatPrice, formatTime } from "@/utils/market-data";
import { clampNumber, formatAxisTimeLabel } from "./chart-helpers";
import type { DashboardChartController } from "./types";

export const useDashboardChartCanvas = (
  controller: Pick<
    DashboardChartController,
    "canvasRef" | "crosshair" | "emaSeries" | "interval" | "lastVisibleCandle" | "overlayLevels" | "smaSeries" | "visibleCandles" | "visualSpreadBand"
  >,
) => {
  const slotWidthRef = useRef(10);

  useEffect(() => {
    const canvas = controller.canvasRef.current;
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

    const drawLineSeries = (points: number[], color: string, priceToY: (value: number) => number, innerWidth: number) => {
      if (points.length < 2) return;
      context.strokeStyle = color;
      context.lineWidth = 1.2;
      context.beginPath();
      points.forEach((value, index) => {
        const x = leftPadding + index * (innerWidth / Math.max(points.length - 1, 1));
        const y = priceToY(value);
        if (index === 0) context.moveTo(x, y);
        else context.lineTo(x, y);
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
      if (controller.visibleCandles.length === 0) return;

      const highs = controller.visibleCandles.map((item) => item.high);
      const lows = controller.visibleCandles.map((item) => item.low);
      const emaHighs = controller.emaSeries.length > 0 ? controller.emaSeries : highs;
      const smaHighs = controller.smaSeries.length > 0 ? controller.smaSeries : highs;
      const overlayPrices = [...controller.overlayLevels.map((item) => item.price), ...(controller.visualSpreadBand ? [controller.visualSpreadBand.bid, controller.visualSpreadBand.ask] : [])];
      const minPrice = Math.min(...lows, ...emaHighs, ...smaHighs, ...overlayPrices);
      const maxPrice = Math.max(...highs, ...emaHighs, ...smaHighs, ...overlayPrices);
      const maxVolume = Math.max(...controller.visibleCandles.map((item) => item.volume), 1);
      const verticalPadding = (maxPrice - minPrice || 1) * 0.14;
      const paddedMin = minPrice - verticalPadding;
      const paddedMax = maxPrice + verticalPadding;
      const range = Math.max(paddedMax - paddedMin, 1);
      const candleSlotWidth = innerWidth / Math.max(controller.visibleCandles.length, 1);
      const candleWidth = clampNumber(candleSlotWidth * 0.62, 4, 12);
      slotWidthRef.current = candleSlotWidth;
      const priceToY = (price: number) => topPadding + ((paddedMax - price) / range) * chartHeight;
      const volumeToY = (volume: number) => volumeTop + volumeHeight * (1 - volume / maxVolume);
      const yToPrice = (y: number) => paddedMax - ((y - topPadding) / chartHeight) * range;

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

      const axisTickCount = Math.min(6, controller.visibleCandles.length);
      let previousAxisRightEdge = -Infinity;
      Array.from({ length: axisTickCount }).forEach((_, index) => {
        const candleIndex = Math.round(((controller.visibleCandles.length - 1) * index) / Math.max(axisTickCount - 1, 1));
        const candle = controller.visibleCandles[candleIndex];
        if (!candle) return;
        const x = leftPadding + candleIndex * candleSlotWidth + candleSlotWidth / 2;
        const label = formatAxisTimeLabel(candle.closeTime, controller.interval);
        const labelWidth = context.measureText(label).width;
        const labelX = clampNumber(x - labelWidth / 2, leftPadding, width - rightPadding - labelWidth);
        if (labelX <= previousAxisRightEdge + 14) return;
        previousAxisRightEdge = labelX + labelWidth;
        context.strokeStyle = "rgba(255,255,255,0.16)";
        context.beginPath();
        context.moveTo(x, height - bottomPadding);
        context.lineTo(x, height - bottomPadding + 8);
        context.stroke();
        context.fillStyle = "rgba(255,255,255,0.52)";
        context.fillText(label, labelX, height - bottomPadding + 26);
      });

      controller.visibleCandles.forEach((candle, index) => {
        const x = leftPadding + index * candleSlotWidth + (candleSlotWidth - candleWidth) / 2;
        const wickX = x + candleWidth / 2;
        const openY = priceToY(candle.open);
        const closeY = priceToY(candle.close);
        const highY = priceToY(candle.high);
        const lowY = priceToY(candle.low);
        const bodyY = Math.min(openY, closeY);
        const bodyHeight = Math.max(Math.abs(closeY - openY), 1.8);
        const isBullish = candle.close >= candle.open;
        context.strokeStyle = isBullish ? "#0bfc3f" : "#f40400";
        context.beginPath();
        context.moveTo(wickX, highY);
        context.lineTo(wickX, lowY);
        context.stroke();
        context.fillStyle = isBullish ? "rgba(25, 255, 75, 0.88)" : "rgba(255, 23, 19, 0.82)";
        context.fillRect(x, bodyY, candleWidth, bodyHeight);
        context.strokeStyle = isBullish ? "rgba(19, 249, 76, 0.95)" : "rgba(246, 38, 27, 0.92)";
        context.strokeRect(x, bodyY, candleWidth, bodyHeight);
        const volumeY = volumeToY(candle.volume);
        context.fillStyle = isBullish ? "rgba(75, 225, 107, 0.18)" : "rgba(255, 120, 117, 0.16)";
        context.fillRect(x, volumeY, candleWidth, height - volumeY - 18);
      });

      drawLineSeries(controller.emaSeries, "rgba(214, 244, 158, 0.88)", priceToY, innerWidth);
      drawLineSeries(controller.smaSeries, "rgba(96, 165, 250, 0.78)", priceToY, innerWidth);

      if (controller.visualSpreadBand) {
        const bandTop = priceToY(controller.visualSpreadBand.ask);
        const bandBottom = priceToY(controller.visualSpreadBand.bid);
        context.fillStyle = "rgba(168, 85, 247, 0.1)";
        context.fillRect(leftPadding, Math.min(bandTop, bandBottom), innerWidth, Math.max(Math.abs(bandBottom - bandTop), 1.2));
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

      controller.overlayLevels.forEach((level, index) => {
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

      if (controller.lastVisibleCandle) {
        const currentPriceY = priceToY(controller.lastVisibleCandle.close);
        context.strokeStyle = "rgba(46, 249, 0, 0.68)";
        context.setLineDash([5, 5]);
        context.beginPath();
        context.moveTo(leftPadding, currentPriceY);
        context.lineTo(width - rightPadding, currentPriceY);
        context.stroke();
        context.setLineDash([]);
        drawAxisLabel(formatPrice(controller.lastVisibleCandle.close), width - rightPadding + 6, currentPriceY, "#7cf0a1");
      }

      if (controller.crosshair && controller.crosshair.x >= leftPadding && controller.crosshair.x <= width - rightPadding && controller.crosshair.y >= topPadding && controller.crosshair.y <= height - bottomPadding) {
        const hoverIndex = clampNumber(Math.floor((controller.crosshair.x - leftPadding) / candleSlotWidth), 0, controller.visibleCandles.length - 1);
        const hoveredCandle = controller.visibleCandles[hoverIndex];
        const candleCenterX = leftPadding + hoverIndex * candleSlotWidth + candleSlotWidth / 2;
        const hoverPrice = yToPrice(controller.crosshair.y);
        context.strokeStyle = "rgba(255,255,255,0.25)";
        context.setLineDash([4, 4]);
        context.beginPath();
        context.moveTo(candleCenterX, topPadding);
        context.lineTo(candleCenterX, height - bottomPadding);
        context.moveTo(leftPadding, controller.crosshair.y);
        context.lineTo(width - rightPadding, controller.crosshair.y);
        context.stroke();
        context.setLineDash([]);
        drawAxisLabel(formatPrice(hoverPrice), width - rightPadding + 6, controller.crosshair.y, "#ffffff");
        const timeLabel = formatTime(new Date(hoveredCandle.closeTime).toISOString());
        const timeWidth = Math.max(context.measureText(timeLabel).width + 12, 96);
        const timeX = clampNumber(candleCenterX - timeWidth / 2, leftPadding, width - rightPadding - timeWidth);
        context.fillStyle = "rgba(5, 6, 7, 0.96)";
        context.fillRect(timeX, height - bottomPadding + 12, timeWidth, 20);
        context.strokeStyle = "rgba(255,255,255,0.18)";
        context.strokeRect(timeX, height - bottomPadding + 12, timeWidth, 20);
        context.fillStyle = "#ffffff";
        context.fillText(timeLabel, timeX + 6, height - bottomPadding + 22);
        const infoLines = [`O ${formatPrice(hoveredCandle.open)}`, `H ${formatPrice(hoveredCandle.high)}`, `L ${formatPrice(hoveredCandle.low)}`, `C ${formatPrice(hoveredCandle.close)}`];
        const infoWidth = Math.max(...infoLines.map((line) => context.measureText(line).width)) + 16;
        const infoHeight = 18 + infoLines.length * 16;
        context.fillStyle = "rgba(5, 6, 7, 0.94)";
        context.fillRect(leftPadding + 8, topPadding + 8, infoWidth, infoHeight);
        context.strokeStyle = "rgba(255,255,255,0.14)";
        context.strokeRect(leftPadding + 8, topPadding + 8, infoWidth, infoHeight);
        infoLines.forEach((line, index) => {
          context.fillStyle = index === 3 ? (hoveredCandle.close >= hoveredCandle.open ? "#7cf0a1" : "#ff7875") : "rgba(255,255,255,0.82)";
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
  }, [controller]);
};
