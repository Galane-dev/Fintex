export interface EconomicCalendarEvent {
  title: string;
  source: string;
  occursAtUtc: string;
  impactScore: number;
}

export interface EconomicCalendarInsight {
  summary: string;
  riskScore: number;
  nextEventAtUtc: string | null;
  upcomingEvents: EconomicCalendarEvent[];
}
