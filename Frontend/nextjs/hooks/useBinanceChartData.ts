"use client";

import { useEffect, useMemo, useState } from "react";

export interface BinanceCandle {
  openTime: number;
  closeTime: number;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  isClosed: boolean;
}

export type BinanceInterval = "1m" | "5m" | "15m" | "1h" | "4h";
export type BinanceFeedStatus = "loading" | "live" | "reconnecting" | "offline" | "error";

const REST_LIMIT = 180;
const REST_BASE_URL = "https://api.binance.com";
const WS_BASE_URL = "wss://stream.binance.com:9443/ws";

const mapRestKline = (entry: unknown[]): BinanceCandle => ({
  openTime: Number(entry[0]),
  open: Number(entry[1]),
  high: Number(entry[2]),
  low: Number(entry[3]),
  close: Number(entry[4]),
  volume: Number(entry[5]),
  closeTime: Number(entry[6]),
  isClosed: true,
});

const mapSocketKline = (value: Record<string, unknown>): BinanceCandle => ({
  openTime: Number(value.t),
  open: Number(value.o),
  high: Number(value.h),
  low: Number(value.l),
  close: Number(value.c),
  volume: Number(value.v),
  closeTime: Number(value.T),
  isClosed: Boolean(value.x),
});

const upsertCandle = (candles: BinanceCandle[], nextCandle: BinanceCandle) => {
  const updated = candles.some((item) => item.openTime === nextCandle.openTime)
    ? candles.map((item) => (item.openTime === nextCandle.openTime ? nextCandle : item))
    : [...candles, nextCandle];

  return updated.sort((left, right) => left.openTime - right.openTime).slice(-REST_LIMIT);
};

export const useBinanceChartData = (
  symbol: string,
  interval: BinanceInterval,
) => {
  const [candles, setCandles] = useState<BinanceCandle[]>([]);
  const [status, setStatus] = useState<BinanceFeedStatus>("loading");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isActive = true;
    let socket: WebSocket | null = null;
    let reconnectTimeout: ReturnType<typeof setTimeout> | null = null;

    const fetchHistory = async () => {
      setStatus("loading");
      setError(null);

      try {
        const response = await fetch(
          `${REST_BASE_URL}/api/v3/klines?symbol=${symbol}&interval=${interval}&limit=${REST_LIMIT}`,
          {
            cache: "no-store",
          },
        );

        if (!response.ok) {
          throw new Error("Could not load Binance chart history.");
        }

        const payload = (await response.json()) as unknown[][];

        if (!isActive) {
          return;
        }

        setCandles(payload.map(mapRestKline));
        setStatus("reconnecting");
      } catch (nextError) {
        if (!isActive) {
          return;
        }

        setStatus("error");
        setError(nextError instanceof Error ? nextError.message : "Could not load Binance chart history.");
      }
    };

    const connectSocket = () => {
      if (!isActive) {
        return;
      }

      const streamName = `${symbol.toLowerCase()}@kline_${interval}`;
      socket = new WebSocket(`${WS_BASE_URL}/${streamName}`);

      socket.onopen = () => {
        if (!isActive) {
          return;
        }

        setStatus("live");
      };

      socket.onmessage = (event) => {
        if (!isActive) {
          return;
        }

        try {
          const payload = JSON.parse(event.data) as { k?: Record<string, unknown> };
          if (!payload.k) {
            return;
          }

          setCandles((current) => upsertCandle(current, mapSocketKline(payload.k!)));
        } catch {
          setStatus("error");
          setError("Could not parse Binance live chart data.");
        }
      };

      socket.onerror = () => {
        if (!isActive) {
          return;
        }

        setStatus("error");
        setError("Binance live chart connection hit an error.");
      };

      socket.onclose = () => {
        if (!isActive) {
          return;
        }

        setStatus("offline");
        reconnectTimeout = setTimeout(() => {
          setStatus("reconnecting");
          connectSocket();
        }, 2000);
      };
    };

    void fetchHistory().then(() => {
      if (isActive) {
        connectSocket();
      }
    });

    return () => {
      isActive = false;

      if (reconnectTimeout) {
        clearTimeout(reconnectTimeout);
      }

      if (socket) {
        socket.close();
      }
    };
  }, [interval, symbol]);

  const latestCandle = useMemo(() => candles.at(-1) ?? null, [candles]);

  return {
    candles,
    error,
    latestCandle,
    status,
  };
};
