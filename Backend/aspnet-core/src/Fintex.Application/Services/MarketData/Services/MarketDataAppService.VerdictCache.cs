using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Concurrent;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
        private static readonly ConcurrentDictionary<string, CachedVerdictSnapshot> VerdictCache = new();
        private static readonly TimeSpan VerdictCacheTtl = TimeSpan.FromSeconds(20);

        private static string BuildVerdictCacheKey(string symbol, MarketDataProvider provider, long marketDataPointId)
        {
            return $"{symbol?.Trim()?.ToUpperInvariant()}::{provider}::{marketDataPointId}";
        }

        private static MarketVerdictDto TryGetCachedVerdict(string cacheKey)
        {
            if (!VerdictCache.TryGetValue(cacheKey, out var cached))
            {
                return null;
            }

            if (cached.ExpiresAtUtc < DateTime.UtcNow)
            {
                VerdictCache.TryRemove(cacheKey, out _);
                return null;
            }

            return cached.Verdict;
        }

        private static void CacheVerdict(string cacheKey, MarketVerdictDto verdict)
        {
            VerdictCache[cacheKey] = new CachedVerdictSnapshot(verdict, DateTime.UtcNow.Add(VerdictCacheTtl));
        }

        private readonly struct CachedVerdictSnapshot
        {
            public CachedVerdictSnapshot(MarketVerdictDto verdict, DateTime expiresAtUtc)
            {
                Verdict = verdict;
                ExpiresAtUtc = expiresAtUtc;
            }

            public MarketVerdictDto Verdict { get; }

            public DateTime ExpiresAtUtc { get; }
        }
    }
}
