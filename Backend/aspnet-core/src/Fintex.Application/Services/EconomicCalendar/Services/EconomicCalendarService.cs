using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.EconomicCalendar
{
    /// <summary>
    /// Pulls upcoming CPI, NFP, and FOMC events from official sources and turns them into macro risk shading.
    /// </summary>
    public class EconomicCalendarService : IEconomicCalendarService, ITransientDependency
    {
        private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);
        private static DateTime _cachedAtUtc = DateTime.MinValue;
        private static List<EconomicCalendarEvent> _cachedEvents = new List<EconomicCalendarEvent>();

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EconomicCalendarService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<EconomicCalendarInsight> GetBitcoinUsdRiskInsightAsync(CancellationToken cancellationToken)
        {
            var upcomingEvents = await GetUpcomingEventsAsync(cancellationToken);
            var relevantEvents = upcomingEvents
                .Where(item => item.OccursAtUtc >= DateTime.UtcNow)
                .OrderBy(item => item.OccursAtUtc)
                .Take(5)
                .ToList();

            if (relevantEvents.Count == 0)
            {
                return new EconomicCalendarInsight
                {
                    Summary = "No high-impact CPI, NFP, or FOMC event is currently close enough to materially shade this BTC/USD recommendation.",
                    RiskScore = 0m
                };
            }

            var nextEvent = relevantEvents[0];
            var hoursUntilNextEvent = (nextEvent.OccursAtUtc - DateTime.UtcNow).TotalHours;
            var riskScore = CalculateRiskScore(nextEvent, hoursUntilNextEvent);
            var summary = BuildSummary(nextEvent, hoursUntilNextEvent, relevantEvents.Count);

            return new EconomicCalendarInsight
            {
                Summary = summary,
                RiskScore = riskScore,
                NextEventAtUtc = nextEvent.OccursAtUtc,
                UpcomingEvents = relevantEvents
            };
        }

        private async Task<IReadOnlyList<EconomicCalendarEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken)
        {
            var refreshWindowMinutes = Math.Max(10, _configuration.GetValue<int?>("EconomicCalendar:RefreshWindowMinutes") ?? 30);
            if (_cachedAtUtc >= DateTime.UtcNow.AddMinutes(-refreshWindowMinutes) && _cachedEvents.Count > 0)
            {
                return _cachedEvents;
            }

            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                if (_cachedAtUtc >= DateTime.UtcNow.AddMinutes(-refreshWindowMinutes) && _cachedEvents.Count > 0)
                {
                    return _cachedEvents;
                }

                var events = new List<EconomicCalendarEvent>();
                events.AddRange(await TryFetchSourceEventsAsync(() => FetchBlsEventsAsync(cancellationToken)));
                events.AddRange(await TryFetchSourceEventsAsync(() => FetchFomcEventsAsync(cancellationToken)));

                if (events.Count == 0 && _cachedEvents.Count > 0)
                {
                    return _cachedEvents;
                }

                _cachedEvents = events
                    .Where(item => item.OccursAtUtc >= DateTime.UtcNow.AddDays(-1))
                    .OrderBy(item => item.OccursAtUtc)
                    .ToList();
                _cachedAtUtc = DateTime.UtcNow;
                return _cachedEvents;
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        private async Task<IEnumerable<EconomicCalendarEvent>> FetchBlsEventsAsync(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["EconomicCalendar:Sources:BlsIcsUrl"] ?? "https://www.bls.gov/schedule/news_release/bls.ics");
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("FintexMacroCalendar", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/calendar"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseBlsIcs(payload);
        }

        private async Task<IEnumerable<EconomicCalendarEvent>> FetchFomcEventsAsync(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration["EconomicCalendar:Sources:FomcUrl"] ?? "https://www.federalreserve.gov/monetarypolicy/fomccalendars.htm");
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("FintexMacroCalendar", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseFomcHtml(payload);
        }

        private static async Task<IEnumerable<EconomicCalendarEvent>> TryFetchSourceEventsAsync(Func<Task<IEnumerable<EconomicCalendarEvent>>> fetch)
        {
            try
            {
                return await fetch();
            }
            catch (HttpRequestException)
            {
                return Enumerable.Empty<EconomicCalendarEvent>();
            }
        }

        private static IEnumerable<EconomicCalendarEvent> ParseBlsIcs(string payload)
        {
            var rawEvents = Regex.Split(payload ?? string.Empty, "BEGIN:VEVENT", RegexOptions.IgnoreCase);
            foreach (var rawEvent in rawEvents)
            {
                var summary = MatchIcsValue(rawEvent, "SUMMARY");
                var startsAt = MatchIcsValue(rawEvent, "DTSTART");
                if (string.IsNullOrWhiteSpace(summary) || string.IsNullOrWhiteSpace(startsAt))
                {
                    continue;
                }

                if (!TryParseIcsDate(startsAt, out var occursAtUtc))
                {
                    continue;
                }

                if (summary.Contains("Consumer Price Index", StringComparison.OrdinalIgnoreCase))
                {
                    yield return BuildEvent(EconomicCalendarEventType.ConsumerPriceIndex, "CPI", "BLS", occursAtUtc, 82m);
                }

                if (summary.Contains("Employment Situation", StringComparison.OrdinalIgnoreCase))
                {
                    yield return BuildEvent(EconomicCalendarEventType.NonFarmPayrolls, "NFP / Employment Situation", "BLS", occursAtUtc, 80m);
                }
            }
        }

        private static IEnumerable<EconomicCalendarEvent> ParseFomcHtml(string payload)
        {
            var plainText = Regex.Replace(payload ?? string.Empty, "<[^>]+>", " ");
            plainText = System.Net.WebUtility.HtmlDecode(plainText);
            plainText = Regex.Replace(plainText, "\\s+", " ").Trim();

            foreach (var yearMatch in Regex.Matches(plainText, @"(?<year>20\d{2})\s+FOMC Meetings(?<body>.*?)(?=20\d{2}\s+FOMC Meetings|Last Update:|$)"))
            {
                if (!int.TryParse(((Match)yearMatch).Groups["year"].Value, out var year))
                {
                    continue;
                }

                var body = ((Match)yearMatch).Groups["body"].Value;
                foreach (Match eventMatch in Regex.Matches(body, @"(?<month>January|February|March|April|May|June|July|August|September|October|November|December)\s+(?<start>\d{1,2})(?:-(?<end>\d{1,2}))?"))
                {
                    if (!TryBuildFomcDate(year, eventMatch.Groups["month"].Value, eventMatch.Groups["start"].Value, eventMatch.Groups["end"].Value, out var occursAtUtc))
                    {
                        continue;
                    }

                    yield return BuildEvent(EconomicCalendarEventType.Fomc, "FOMC decision", "Federal Reserve", occursAtUtc, 88m);
                }
            }
        }

        private static EconomicCalendarEvent BuildEvent(EconomicCalendarEventType type, string title, string source, DateTime occursAtUtc, decimal impactScore)
        {
            return new EconomicCalendarEvent
            {
                Type = type,
                Title = title,
                Source = source,
                OccursAtUtc = occursAtUtc,
                ImpactScore = impactScore
            };
        }

        private static string MatchIcsValue(string content, string key)
        {
            var match = Regex.Match(content ?? string.Empty, $@"{key}(;[^:]+)?:([^\r\n]+)");
            return match.Success ? match.Groups[2].Value.Trim() : null;
        }

        private static bool TryParseIcsDate(string rawValue, out DateTime occursAtUtc)
        {
            if (DateTime.TryParseExact(rawValue, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out occursAtUtc))
            {
                occursAtUtc = occursAtUtc.ToUniversalTime();
                return true;
            }

            if (DateTime.TryParseExact(rawValue, "yyyyMMdd'T'HHmm'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out occursAtUtc))
            {
                occursAtUtc = occursAtUtc.ToUniversalTime();
                return true;
            }

            if (DateTime.TryParseExact(rawValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out occursAtUtc))
            {
                occursAtUtc = DateTime.SpecifyKind(occursAtUtc, DateTimeKind.Utc).AddHours(12);
                return true;
            }

            occursAtUtc = default;
            return false;
        }

        private static bool TryBuildFomcDate(int year, string monthText, string startText, string endText, out DateTime occursAtUtc)
        {
            occursAtUtc = default;
            if (!int.TryParse(string.IsNullOrWhiteSpace(endText) ? startText : endText, out var day))
            {
                return false;
            }

            if (!DateTime.TryParseExact(monthText, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
            {
                return false;
            }

            var eastern = GetEasternTimeZone();
            var localReleaseTime = new DateTime(year, month.Month, day, 14, 0, 0, DateTimeKind.Unspecified);
            occursAtUtc = TimeZoneInfo.ConvertTimeToUtc(localReleaseTime, eastern);
            return true;
        }

        private static TimeZoneInfo GetEasternTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            }
        }

        private static decimal CalculateRiskScore(EconomicCalendarEvent nextEvent, double hoursUntilNextEvent)
        {
            var baseRisk = nextEvent.ImpactScore;
            if (hoursUntilNextEvent <= 1)
            {
                return baseRisk;
            }

            if (hoursUntilNextEvent <= 4)
            {
                return Math.Max(60m, baseRisk - 10m);
            }

            if (hoursUntilNextEvent <= 12)
            {
                return Math.Max(42m, baseRisk - 24m);
            }

            if (hoursUntilNextEvent <= 24)
            {
                return Math.Max(26m, baseRisk - 38m);
            }

            return 0m;
        }

        private static string BuildSummary(EconomicCalendarEvent nextEvent, double hoursUntilNextEvent, int upcomingCount)
        {
            if (hoursUntilNextEvent <= 1)
            {
                return $"{nextEvent.Title} is due within the next hour, so BTC/USD event risk is elevated and pre-release trades deserve extra caution.";
            }

            if (hoursUntilNextEvent <= 12)
            {
                return $"{nextEvent.Title} is approaching within {Math.Ceiling(hoursUntilNextEvent)} hours. Macro-event risk should still shade position size and timing.";
            }

            return upcomingCount > 1
                ? $"{nextEvent.Title} is the next major macro event on the tape, with more high-impact releases queued after it."
                : $"{nextEvent.Title} is the next major macro event that could disturb BTC/USD conditions.";
        }
    }
}
