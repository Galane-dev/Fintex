using Abp.Zero.EntityFrameworkCore;
using Fintex.Authorization.Roles;
using Fintex.Authorization.Users;
using Fintex.Investments;
using Fintex.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore
{
    /// <summary>
    /// Main EF Core DbContext for ABP tables and investment domain data.
    /// </summary>
    public class FintexDbContext : AbpZeroDbContext<Tenant, Role, User, FintexDbContext>
    {
        public FintexDbContext(DbContextOptions<FintexDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Stores all persisted market ticks.
        /// </summary>
        public DbSet<MarketDataPoint> MarketDataPoints { get; set; }

        /// <summary>
        /// Stores derived candles for supported indicator timeframes.
        /// </summary>
        public DbSet<MarketDataTimeframeCandle> MarketDataTimeframeCandles { get; set; }

        /// <summary>
        /// Stores user-managed and system-monitored trades.
        /// </summary>
        public DbSet<Trade> Trades { get; set; }

        /// <summary>
        /// Stores user preferences and behavioral insights.
        /// </summary>
        public DbSet<UserProfile> UserProfiles { get; set; }

        /// <summary>
        /// Stores per-trade analytics snapshots.
        /// </summary>
        public DbSet<TradeAnalysisSnapshot> TradeAnalysisSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTrade(modelBuilder);
            ConfigureMarketDataPoint(modelBuilder);
            ConfigureMarketDataTimeframeCandle(modelBuilder);
            ConfigureUserProfile(modelBuilder);
            ConfigureTradeAnalysisSnapshot(modelBuilder);
        }

        private static void ConfigureTrade(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trade>(entity =>
            {
                entity.ToTable("AppTrades");
                entity.HasIndex(x => new { x.TenantId, x.UserId });
                entity.HasIndex(x => new { x.Symbol, x.Status });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(Trade.MaxSymbolLength);
                entity.Property(x => x.ExternalOrderId).HasMaxLength(Trade.MaxExternalOrderIdLength);
                entity.Property(x => x.Notes).HasMaxLength(Trade.MaxNotesLength);
                entity.Property(x => x.CurrentRecommendation).HasMaxLength(64);
                entity.Property(x => x.CurrentAnalysisSummary).HasMaxLength(Trade.MaxNotesLength);

                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.EntryPrice).HasPrecision(18, 8);
                entity.Property(x => x.ExitPrice).HasPrecision(18, 8);
                entity.Property(x => x.StopLoss).HasPrecision(18, 8);
                entity.Property(x => x.TakeProfit).HasPrecision(18, 8);
                entity.Property(x => x.RealizedProfitLoss).HasPrecision(18, 8);
                entity.Property(x => x.UnrealizedProfitLoss).HasPrecision(18, 8);
                entity.Property(x => x.LastMarketPrice).HasPrecision(18, 8);
                entity.Property(x => x.CurrentRiskScore).HasPrecision(10, 4);
            });
        }

        private static void ConfigureMarketDataPoint(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketDataPoint>(entity =>
            {
                entity.ToTable("AppMarketDataPoints");
                entity.HasIndex(x => new { x.Symbol, x.Timestamp });
                entity.HasIndex(x => new { x.Provider, x.Symbol, x.Timestamp });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(MarketDataPoint.MaxSymbolLength);
                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Price).HasPrecision(18, 8);
                entity.Property(x => x.Bid).HasPrecision(18, 8);
                entity.Property(x => x.Ask).HasPrecision(18, 8);
                entity.Property(x => x.Volume).HasPrecision(18, 8);
                entity.Property(x => x.Open24Hours).HasPrecision(18, 8);
                entity.Property(x => x.High24Hours).HasPrecision(18, 8);
                entity.Property(x => x.Low24Hours).HasPrecision(18, 8);
                entity.Property(x => x.Sma).HasPrecision(18, 8);
                entity.Property(x => x.Ema).HasPrecision(18, 8);
                entity.Property(x => x.Rsi).HasPrecision(18, 8);
                entity.Property(x => x.StdDev).HasPrecision(18, 8);
                entity.Property(x => x.Macd).HasPrecision(18, 8);
                entity.Property(x => x.MacdSignal).HasPrecision(18, 8);
                entity.Property(x => x.MacdHistogram).HasPrecision(18, 8);
                entity.Property(x => x.Momentum).HasPrecision(18, 8);
                entity.Property(x => x.RateOfChange).HasPrecision(18, 8);
                entity.Property(x => x.BollingerUpper).HasPrecision(18, 8);
                entity.Property(x => x.BollingerLower).HasPrecision(18, 8);
                entity.Property(x => x.TrendScore).HasPrecision(10, 4);
                entity.Property(x => x.ConfidenceScore).HasPrecision(10, 4);
                entity.Property(x => x.Verdict).HasConversion<string>().HasMaxLength(16);
            });
        }

        private static void ConfigureMarketDataTimeframeCandle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketDataTimeframeCandle>(entity =>
            {
                entity.ToTable("AppMarketDataTimeframeCandles");
                entity.HasIndex(x => new { x.Provider, x.Symbol, x.Timeframe, x.OpenTime }).IsUnique();
                entity.HasIndex(x => new { x.Symbol, x.Timeframe, x.OpenTime });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(MarketDataPoint.MaxSymbolLength);
                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Timeframe).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Open).HasPrecision(18, 8);
                entity.Property(x => x.High).HasPrecision(18, 8);
                entity.Property(x => x.Low).HasPrecision(18, 8);
                entity.Property(x => x.Close).HasPrecision(18, 8);
            });
        }

        private static void ConfigureUserProfile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("AppUserProfiles");
                entity.HasIndex(x => x.UserId).IsUnique();

                entity.Property(x => x.PreferredBaseCurrency).HasMaxLength(UserProfile.MaxCurrencyLength);
                entity.Property(x => x.FavoriteSymbols).HasMaxLength(UserProfile.MaxSymbolsLength);
                entity.Property(x => x.BehavioralSummary).HasMaxLength(UserProfile.MaxSummaryLength);
                entity.Property(x => x.StrategyNotes).HasMaxLength(UserProfile.MaxStrategyLength);
                entity.Property(x => x.LastAiProvider).HasMaxLength(UserProfile.MaxProviderLength);
                entity.Property(x => x.LastAiModel).HasMaxLength(UserProfile.MaxModelLength);

                entity.Property(x => x.RiskTolerance).HasPrecision(10, 4);
                entity.Property(x => x.BehavioralRiskScore).HasPrecision(10, 4);
            });
        }

        private static void ConfigureTradeAnalysisSnapshot(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TradeAnalysisSnapshot>(entity =>
            {
                entity.ToTable("AppTradeAnalysisSnapshots");
                entity.HasIndex(x => new { x.TradeId, x.GeneratedAt });
                entity.HasIndex(x => new { x.UserId, x.GeneratedAt });

                entity.Property(x => x.Recommendation).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Narrative).HasMaxLength(TradeAnalysisSnapshot.MaxSummaryLength);
                entity.Property(x => x.BehavioralSummary).HasMaxLength(TradeAnalysisSnapshot.MaxSummaryLength);
                entity.Property(x => x.ExternalAiProvider).HasMaxLength(TradeAnalysisSnapshot.MaxProviderLength);
                entity.Property(x => x.ExternalAiModel).HasMaxLength(TradeAnalysisSnapshot.MaxModelLength);

                entity.Property(x => x.SmaValue).HasPrecision(18, 8);
                entity.Property(x => x.EmaValue).HasPrecision(18, 8);
                entity.Property(x => x.RsiValue).HasPrecision(18, 8);
                entity.Property(x => x.StdDevValue).HasPrecision(18, 8);
                entity.Property(x => x.CompositeRiskScore).HasPrecision(10, 4);
            });
        }
    }
}
