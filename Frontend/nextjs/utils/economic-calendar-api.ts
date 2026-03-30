import type { EconomicCalendarInsight } from "@/types/economic-calendar";
import { getAxiosInstance } from "./axios-instance";
import { unwrapAbpResponse } from "./abp-response";

const getNumber = (value: unknown, fallback = 0) =>
  typeof value === "number" ? value : value == null ? fallback : Number(value);

const getString = (value: unknown, fallback = "") =>
  typeof value === "string" ? value : fallback;

const normalizeEconomicCalendarInsight = (
  value: Record<string, unknown>,
): EconomicCalendarInsight => ({
  summary: getString(value.summary ?? value.Summary),
  riskScore: getNumber(value.riskScore ?? value.RiskScore),
  nextEventAtUtc:
    value.nextEventAtUtc == null && value.NextEventAtUtc == null
      ? null
      : getString(value.nextEventAtUtc ?? value.NextEventAtUtc),
  upcomingEvents: Array.isArray(value.upcomingEvents ?? value.UpcomingEvents)
    ? (
        (value.upcomingEvents ?? value.UpcomingEvents) as Record<string, unknown>[]
      ).map((item) => ({
        title: getString(item.title ?? item.Title),
        source: getString(item.source ?? item.Source),
        occursAtUtc: getString(item.occursAtUtc ?? item.OccursAtUtc),
        impactScore: getNumber(item.impactScore ?? item.ImpactScore),
      }))
    : [],
});

export const getBitcoinUsdRiskInsight = async (): Promise<EconomicCalendarInsight> => {
  const result = await unwrapAbpResponse<Record<string, unknown>>(
    getAxiosInstance().get("/api/services/app/EconomicCalendar/GetBitcoinUsdRiskInsight"),
    "We could not load the economic calendar right now.",
  );

  return normalizeEconomicCalendarInsight(result);
};
