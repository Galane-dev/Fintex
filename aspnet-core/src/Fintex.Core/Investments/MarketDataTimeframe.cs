using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Supported chart and indicator timeframes.
    /// </summary>
    public enum MarketDataTimeframe
    {
        OneMinute = 1,
        FiveMinutes = 5,
        FifteenMinutes = 15,
        OneHour = 60,
        FourHours = 240
    }

    public static class MarketDataTimeframeExtensions
    {
        public static TimeSpan ToTimeSpan(this MarketDataTimeframe timeframe)
        {
            switch (timeframe)
            {
                case MarketDataTimeframe.OneMinute:
                    return TimeSpan.FromMinutes(1);
                case MarketDataTimeframe.FiveMinutes:
                    return TimeSpan.FromMinutes(5);
                case MarketDataTimeframe.FifteenMinutes:
                    return TimeSpan.FromMinutes(15);
                case MarketDataTimeframe.OneHour:
                    return TimeSpan.FromHours(1);
                case MarketDataTimeframe.FourHours:
                    return TimeSpan.FromHours(4);
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, "Unsupported timeframe.");
            }
        }

        public static string ToCode(this MarketDataTimeframe timeframe)
        {
            switch (timeframe)
            {
                case MarketDataTimeframe.OneMinute:
                    return "1m";
                case MarketDataTimeframe.FiveMinutes:
                    return "5m";
                case MarketDataTimeframe.FifteenMinutes:
                    return "15m";
                case MarketDataTimeframe.OneHour:
                    return "1h";
                case MarketDataTimeframe.FourHours:
                    return "4h";
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, "Unsupported timeframe.");
            }
        }

        public static DateTime FloorTimestamp(this MarketDataTimeframe timeframe, DateTime timestamp)
        {
            var normalized = timestamp.Kind == DateTimeKind.Utc
                ? timestamp
                : timestamp.ToUniversalTime();

            var span = timeframe.ToTimeSpan();
            var flooredTicks = normalized.Ticks - (normalized.Ticks % span.Ticks);
            return new DateTime(flooredTicks, DateTimeKind.Utc);
        }
    }
}
