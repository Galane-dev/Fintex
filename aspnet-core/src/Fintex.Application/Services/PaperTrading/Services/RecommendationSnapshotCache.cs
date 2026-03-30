using Abp.Dependency;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    public class RecommendationSnapshotCache : IRecommendationSnapshotCache, ITransientDependency
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(20);

        public async Task<PaperTradeRecommendationDto> GetOrCreateAsync(string symbol, MarketDataProvider provider, Func<Task<PaperTradeRecommendationDto>> factory)
        {
            var cacheKey = $"{symbol?.Trim()?.ToUpperInvariant()}::{provider}";
            if (Cache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAtUtc >= DateTime.UtcNow)
            {
                return cached.Value;
            }

            var recommendation = await factory();
            Cache[cacheKey] = new CacheEntry(recommendation, DateTime.UtcNow.Add(CacheTtl));
            return recommendation;
        }

        private readonly struct CacheEntry
        {
            public CacheEntry(PaperTradeRecommendationDto value, DateTime expiresAtUtc)
            {
                Value = value;
                ExpiresAtUtc = expiresAtUtc;
            }

            public PaperTradeRecommendationDto Value { get; }

            public DateTime ExpiresAtUtc { get; }
        }
    }
}
