import type {
  MarketDataPoint,
  MarketPriceProjection,
} from "@/types/market-data";

const calculateAdaptivePeriod = (count: number, configuredPeriod: number) => {
  if (count < 4) {
    return null;
  }

  const adaptivePeriod = Math.min(configuredPeriod, count - 1);
  return adaptivePeriod >= 3 ? adaptivePeriod : null;
};

const calculateSimpleMovingAverage = (values: number[], period: number) => {
  if (values.length < period) {
    return null;
  }

  const window = values.slice(-period);
  return window.reduce((sum, value) => sum + value, 0) / period;
};

const calculateExponentialMovingAverage = (values: number[], period: number) => {
  if (values.length < period) {
    return null;
  }

  const multiplier = 2 / (period + 1);
  let average = values.slice(0, period).reduce((sum, value) => sum + value, 0) / period;

  for (let index = period; index < values.length; index += 1) {
    average = (values[index] - average) * multiplier + average;
  }

  return average;
};

const calculateSmoothedMovingAverage = (values: number[], period: number) => {
  if (values.length < period) {
    return null;
  }

  let average = values.slice(0, period).reduce((sum, value) => sum + value, 0) / period;

  for (let index = period; index < values.length; index += 1) {
    average = ((average * (period - 1)) + values[index]) / period;
  }

  return average;
};

const projectFromMovingAverage = (
  values: number[],
  configuredPeriod: number,
  stepsAhead: number,
  currentPrice: number,
  calculator: (series: number[], period: number) => number | null,
) => {
  const period = calculateAdaptivePeriod(values.length, configuredPeriod);
  if (period == null) {
    return null;
  }

  const currentAverage = calculator(values, period);
  const previousAverage = calculator(values.slice(0, -1), period);
  if (currentAverage == null || previousAverage == null) {
    return null;
  }

  const slope = currentAverage - previousAverage;
  const driftProjection = currentPrice + slope * stepsAhead;
  const anchorProjection = currentAverage + slope * stepsAhead;

  return driftProjection + ((anchorProjection - currentPrice) * 0.35);
};

export const buildFallbackProjectionFromHistory = (
  history: MarketDataPoint[],
  currentPrice: number | null | undefined,
  minutesAhead: number,
): MarketPriceProjection | null => {
  if (currentPrice == null) {
    return null;
  }

  const closes = history
    .map((point) => point.price)
    .filter((value) => Number.isFinite(value));

  if (closes.length < 4) {
    return null;
  }

  const smaPrice = projectFromMovingAverage(
    closes,
    20,
    minutesAhead,
    currentPrice,
    calculateSimpleMovingAverage,
  );
  const emaPrice = projectFromMovingAverage(
    closes,
    9,
    minutesAhead,
    currentPrice,
    calculateExponentialMovingAverage,
  );
  const smmaPrice = projectFromMovingAverage(
    closes,
    14,
    minutesAhead,
    currentPrice,
    calculateSmoothedMovingAverage,
  );

  const estimates = [smaPrice, emaPrice, smmaPrice].filter(
    (value): value is number => value != null && Number.isFinite(value),
  );

  return {
    horizon: `${minutesAhead}m`,
    minutesAhead,
    targetTimestamp: new Date(Date.now() + minutesAhead * 60_000).toISOString(),
    consensusPrice:
      estimates.length > 0
        ? estimates.reduce((sum, value) => sum + value, 0) / estimates.length
        : null,
    smaPrice,
    emaPrice,
    smmaPrice,
  };
};
