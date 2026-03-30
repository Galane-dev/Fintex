using Abp.Zero.EntityFrameworkCore;
using Fintex.Authorization.Roles;
using Fintex.Authorization.Users;
using Fintex.Investments.Academy;
using Fintex.Investments.Automation;
using Fintex.Investments;
using Fintex.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore
{
    /// <summary>
    /// Main EF Core DbContext for ABP tables and investment domain data.
    /// </summary>
    public partial class FintexDbContext : AbpZeroDbContext<Tenant, Role, User, FintexDbContext>
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
        /// Stores user-linked external broker connections.
        /// </summary>
        public DbSet<ExternalBrokerConnection> ExternalBrokerConnections { get; set; }

        /// <summary>
        /// Stores rich execution-time context for live broker trades.
        /// </summary>
        public DbSet<TradeExecutionContext> TradeExecutionContexts { get; set; }

        /// <summary>
        /// Stores raw broker websocket execution events for later analytics and reconciliation.
        /// </summary>
        public DbSet<ExternalBrokerExecutionEvent> ExternalBrokerExecutionEvents { get; set; }

        /// <summary>
        /// Stores configured upstream financial and economic news feeds.
        /// </summary>
        public DbSet<NewsSource> NewsSources { get; set; }

        /// <summary>
        /// Stores normalized news articles used for recommendation overlays.
        /// </summary>
        public DbSet<NewsArticle> NewsArticles { get; set; }

        /// <summary>
        /// Stores refresh attempts for cached news ingestion.
        /// </summary>
        public DbSet<NewsRefreshRun> NewsRefreshRuns { get; set; }

        /// <summary>
        /// Stores cached AI summaries of recent BTC/USD news.
        /// </summary>
        public DbSet<NewsAnalysisSnapshot> NewsAnalysisSnapshots { get; set; }

        /// <summary>
        /// Stores user paper trading accounts.
        /// </summary>
        public DbSet<PaperTradingAccount> PaperTradingAccounts { get; set; }

        /// <summary>
        /// Stores simulated orders for paper trading.
        /// </summary>
        public DbSet<PaperOrder> PaperOrders { get; set; }

        /// <summary>
        /// Stores open and closed simulated positions.
        /// </summary>
        public DbSet<PaperPosition> PaperPositions { get; set; }

        /// <summary>
        /// Stores simulated trade fills.
        /// </summary>
        public DbSet<PaperTradeFill> PaperTradeFills { get; set; }

        /// <summary>
        /// Stores user preferences and behavioral insights.
        /// </summary>
        public DbSet<UserProfile> UserProfiles { get; set; }

        /// <summary>
        /// Stores intro academy quiz attempts.
        /// </summary>
        public DbSet<AcademyQuizAttempt> AcademyQuizAttempts { get; set; }

        /// <summary>
        /// Stores per-trade analytics snapshots.
        /// </summary>
        public DbSet<TradeAnalysisSnapshot> TradeAnalysisSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTrade(modelBuilder);
            ConfigureTradeExecutionContext(modelBuilder);
            ConfigureExternalBrokerExecutionEvent(modelBuilder);
            ConfigureExternalBrokerConnection(modelBuilder);
            ConfigureNotificationAlertRule(modelBuilder);
            ConfigureNotificationItem(modelBuilder);
            ConfigureTradeAutomationRule(modelBuilder);
            ConfigureNewsSource(modelBuilder);
            ConfigureNewsArticle(modelBuilder);
            ConfigureNewsRefreshRun(modelBuilder);
            ConfigureNewsAnalysisSnapshot(modelBuilder);
            ConfigurePaperTradingAccount(modelBuilder);
            ConfigurePaperOrder(modelBuilder);
            ConfigurePaperPosition(modelBuilder);
            ConfigurePaperTradeFill(modelBuilder);
            ConfigureStrategyValidationRun(modelBuilder);
            ConfigureMarketDataPoint(modelBuilder);
            ConfigureMarketDataTimeframeCandle(modelBuilder);
            ConfigureUserProfile(modelBuilder);
            ConfigureAcademyQuizAttempt(modelBuilder);
            ConfigureTradeAnalysisSnapshot(modelBuilder);
            ConfigureGoalTarget(modelBuilder);
            ConfigureGoalEvaluationRun(modelBuilder);
            ConfigureGoalExecutionPlan(modelBuilder);
            ConfigureGoalExecutionEvent(modelBuilder);
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

        private static void ConfigureTradeExecutionContext(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TradeExecutionContext>(entity =>
            {
                entity.ToTable("AppTradeExecutionContexts");
                entity.HasIndex(x => new { x.TradeId, x.UserId });
                entity.HasIndex(x => new { x.UserId, x.ExternalBrokerConnectionId, x.CreationTime });

                entity.Property(x => x.BrokerEnvironment).IsRequired().HasMaxLength(TradeExecutionContext.MaxEnvironmentLength);
                entity.Property(x => x.BrokerSymbol).IsRequired().HasMaxLength(TradeExecutionContext.MaxBrokerSymbolLength);
                entity.Property(x => x.MarketVerdict).HasMaxLength(TradeExecutionContext.MaxVerdictLength);
                entity.Property(x => x.StructureLabel).HasMaxLength(TradeExecutionContext.MaxStructureLabelLength);
                entity.Property(x => x.BrokerOrderId).HasMaxLength(TradeExecutionContext.MaxOrderIdLength);
                entity.Property(x => x.BrokerClientOrderId).HasMaxLength(TradeExecutionContext.MaxClientOrderIdLength);
                entity.Property(x => x.BrokerOrderStatus).HasMaxLength(TradeExecutionContext.MaxStatusLength);
                entity.Property(x => x.Notes).HasMaxLength(TradeExecutionContext.MaxSummaryLength);
                entity.Property(x => x.DecisionSummary).HasMaxLength(TradeExecutionContext.MaxSummaryLength);
                entity.Property(x => x.RequestPayloadJson).HasMaxLength(TradeExecutionContext.MaxSummaryLength);
                entity.Property(x => x.BrokerResponseJson).HasMaxLength(TradeExecutionContext.MaxSummaryLength);
                entity.Property(x => x.UserBehavioralSummary).HasMaxLength(TradeExecutionContext.MaxSummaryLength);

                entity.Property(x => x.BrokerProvider).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.BrokerPlatform).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.MarketDataProvider).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.ReferencePrice).HasPrecision(18, 8);
                entity.Property(x => x.Bid).HasPrecision(18, 8);
                entity.Property(x => x.Ask).HasPrecision(18, 8);
                entity.Property(x => x.Spread).HasPrecision(18, 8);
                entity.Property(x => x.SpreadPercent).HasPrecision(18, 8);
                entity.Property(x => x.StopLoss).HasPrecision(18, 8);
                entity.Property(x => x.TakeProfit).HasPrecision(18, 8);
                entity.Property(x => x.TrendScore).HasPrecision(10, 4);
                entity.Property(x => x.ConfidenceScore).HasPrecision(10, 4);
                entity.Property(x => x.TimeframeAlignmentScore).HasPrecision(10, 4);
                entity.Property(x => x.StructureScore).HasPrecision(10, 4);
                entity.Property(x => x.Sma).HasPrecision(18, 8);
                entity.Property(x => x.Ema).HasPrecision(18, 8);
                entity.Property(x => x.Rsi).HasPrecision(18, 8);
                entity.Property(x => x.Macd).HasPrecision(18, 8);
                entity.Property(x => x.MacdSignal).HasPrecision(18, 8);
                entity.Property(x => x.MacdHistogram).HasPrecision(18, 8);
                entity.Property(x => x.Momentum).HasPrecision(18, 8);
                entity.Property(x => x.RateOfChange).HasPrecision(18, 8);
                entity.Property(x => x.Atr).HasPrecision(18, 8);
                entity.Property(x => x.AtrPercent).HasPrecision(18, 8);
                entity.Property(x => x.Adx).HasPrecision(18, 8);
                entity.Property(x => x.UserRiskTolerance).HasPrecision(10, 4);
                entity.Property(x => x.UserBehavioralRiskScore).HasPrecision(10, 4);
                entity.Property(x => x.BrokerSubmittedQuantity).HasPrecision(18, 8);
                entity.Property(x => x.BrokerFilledQuantity).HasPrecision(18, 8);
                entity.Property(x => x.BrokerFilledAveragePrice).HasPrecision(18, 8);
            });
        }

        private static void ConfigureExternalBrokerConnection(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalBrokerConnection>(entity =>
            {
                entity.ToTable("AppExternalBrokerConnections");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive });
                entity.HasIndex(x => new { x.UserId, x.Provider, x.AccountLogin, x.Server });

                entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(ExternalBrokerConnection.MaxDisplayNameLength);
                entity.Property(x => x.AccountLogin).IsRequired().HasMaxLength(ExternalBrokerConnection.MaxLoginLength);
                entity.Property(x => x.Server).IsRequired().HasMaxLength(ExternalBrokerConnection.MaxServerLength);
                entity.Property(x => x.EncryptedPassword).IsRequired();
                entity.Property(x => x.TerminalPath).HasMaxLength(ExternalBrokerConnection.MaxTerminalPathLength);
                entity.Property(x => x.LastError).HasMaxLength(ExternalBrokerConnection.MaxErrorLength);
                entity.Property(x => x.BrokerAccountName).HasMaxLength(ExternalBrokerConnection.MaxAccountNameLength);
                entity.Property(x => x.BrokerAccountCurrency).HasMaxLength(ExternalBrokerConnection.MaxCurrencyLength);
                entity.Property(x => x.BrokerCompany).HasMaxLength(ExternalBrokerConnection.MaxCompanyLength);

                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Platform).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

                entity.Property(x => x.LastKnownBalance).HasPrecision(18, 8);
                entity.Property(x => x.LastKnownEquity).HasPrecision(18, 8);
            });
        }

        private static void ConfigureExternalBrokerExecutionEvent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalBrokerExecutionEvent>(entity =>
            {
                entity.ToTable("AppExternalBrokerExecutionEvents");
                entity.HasIndex(x => new { x.UserId, x.ExternalBrokerConnectionId, x.CreationTime });
                entity.HasIndex(x => new { x.ExternalBrokerConnectionId, x.BrokerOrderId, x.EventType });
                entity.HasIndex(x => new { x.ExternalBrokerConnectionId, x.ExecutionId });

                entity.Property(x => x.BrokerEnvironment).IsRequired().HasMaxLength(ExternalBrokerConnection.MaxServerLength);
                entity.Property(x => x.EventType).IsRequired().HasMaxLength(ExternalBrokerExecutionEvent.MaxEventTypeLength);
                entity.Property(x => x.ExecutionId).HasMaxLength(ExternalBrokerExecutionEvent.MaxExecutionIdLength);
                entity.Property(x => x.BrokerOrderId).HasMaxLength(ExternalBrokerExecutionEvent.MaxOrderIdLength);
                entity.Property(x => x.BrokerClientOrderId).HasMaxLength(ExternalBrokerExecutionEvent.MaxClientOrderIdLength);
                entity.Property(x => x.BrokerSymbol).HasMaxLength(ExternalBrokerExecutionEvent.MaxSymbolLength);
                entity.Property(x => x.NormalizedSymbol).HasMaxLength(ExternalBrokerExecutionEvent.MaxSymbolLength);
                entity.Property(x => x.BrokerOrderStatus).HasMaxLength(ExternalBrokerExecutionEvent.MaxStatusLength);
                entity.Property(x => x.RawPayloadJson).HasMaxLength(ExternalBrokerExecutionEvent.MaxPayloadLength);

                entity.Property(x => x.BrokerProvider).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.BrokerPlatform).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.FilledQuantity).HasPrecision(18, 8);
                entity.Property(x => x.EventQuantity).HasPrecision(18, 8);
                entity.Property(x => x.Price).HasPrecision(18, 8);
                entity.Property(x => x.FilledAveragePrice).HasPrecision(18, 8);
                entity.Property(x => x.PositionQuantity).HasPrecision(18, 8);
            });
        }

        private static void ConfigureNewsSource(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsSource>(entity =>
            {
                entity.ToTable("AppNewsSources");
                entity.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
                entity.HasIndex(x => new { x.IsActive, x.Category });

                entity.Property(x => x.Name).IsRequired().HasMaxLength(NewsSource.MaxNameLength);
                entity.Property(x => x.SiteUrl).IsRequired().HasMaxLength(NewsSource.MaxUrlLength);
                entity.Property(x => x.FeedUrl).IsRequired().HasMaxLength(NewsSource.MaxUrlLength);
                entity.Property(x => x.Category).HasMaxLength(NewsSource.MaxCategoryLength);
                entity.Property(x => x.FocusTags).HasMaxLength(NewsSource.MaxFocusTagsLength);
                entity.Property(x => x.LastError).HasMaxLength(NewsSource.MaxErrorLength);

                entity.Property(x => x.SourceKind).HasConversion<string>().HasMaxLength(16);
            });
        }

        private static void ConfigureNewsArticle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsArticle>(entity =>
            {
                entity.ToTable("AppNewsArticles");
                entity.HasIndex(x => new { x.SourceId, x.Url }).IsUnique();
                entity.HasIndex(x => new { x.IsBitcoinRelevant, x.IsUsdRelevant, x.PublishedAt });
                entity.HasIndex(x => new { x.RelevanceScore, x.PublishedAt });

                entity.Property(x => x.Url).IsRequired().HasMaxLength(NewsArticle.MaxUrlLength);
                entity.Property(x => x.Title).IsRequired().HasMaxLength(NewsArticle.MaxTitleLength);
                entity.Property(x => x.Summary).HasMaxLength(NewsArticle.MaxSummaryLength);
                entity.Property(x => x.Author).HasMaxLength(NewsArticle.MaxAuthorLength);
                entity.Property(x => x.Category).HasMaxLength(NewsArticle.MaxCategoryLength);
                entity.Property(x => x.Tags).HasMaxLength(NewsArticle.MaxTagsLength);
                entity.Property(x => x.ContentHash).IsRequired().HasMaxLength(NewsArticle.MaxHashLength);
                entity.Property(x => x.RawPayloadJson).HasMaxLength(NewsArticle.MaxRawPayloadLength);
            });
        }

        private static void ConfigureNewsRefreshRun(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsRefreshRun>(entity =>
            {
                entity.ToTable("AppNewsRefreshRuns");
                entity.HasIndex(x => new { x.FocusKey, x.Status, x.CreationTime });

                entity.Property(x => x.FocusKey).IsRequired().HasMaxLength(NewsRefreshRun.MaxFocusKeyLength);
                entity.Property(x => x.Trigger).IsRequired().HasMaxLength(NewsRefreshRun.MaxTriggerLength);
                entity.Property(x => x.Summary).HasMaxLength(NewsRefreshRun.MaxSummaryLength);

                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            });
        }

        private static void ConfigureNewsAnalysisSnapshot(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsAnalysisSnapshot>(entity =>
            {
                entity.ToTable("AppNewsAnalysisSnapshots");
                entity.HasIndex(x => new { x.FocusKey, x.GeneratedAt });
                entity.HasIndex(x => new { x.FocusKey, x.LatestArticlePublishedAt });

                entity.Property(x => x.FocusKey).IsRequired().HasMaxLength(NewsAnalysisSnapshot.MaxFocusKeyLength);
                entity.Property(x => x.Summary).HasMaxLength(NewsAnalysisSnapshot.MaxSummaryLength);
                entity.Property(x => x.KeyHeadlines).HasMaxLength(NewsAnalysisSnapshot.MaxHeadlinesLength);
                entity.Property(x => x.Provider).HasMaxLength(NewsAnalysisSnapshot.MaxProviderLength);
                entity.Property(x => x.Model).HasMaxLength(NewsAnalysisSnapshot.MaxModelLength);
                entity.Property(x => x.RawPayloadJson).HasMaxLength(NewsAnalysisSnapshot.MaxRawPayloadLength);

                entity.Property(x => x.Sentiment).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.RecommendedAction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.ImpactScore).HasPrecision(10, 4);
            });
        }

        private static void ConfigurePaperTradingAccount(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaperTradingAccount>(entity =>
            {
                entity.ToTable("AppPaperTradingAccounts");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive });

                entity.Property(x => x.Name).IsRequired().HasMaxLength(PaperTradingAccount.MaxNameLength);
                entity.Property(x => x.BaseCurrency).IsRequired().HasMaxLength(PaperTradingAccount.MaxCurrencyLength);

                entity.Property(x => x.StartingBalance).HasPrecision(18, 8);
                entity.Property(x => x.CashBalance).HasPrecision(18, 8);
                entity.Property(x => x.Equity).HasPrecision(18, 8);
                entity.Property(x => x.RealizedProfitLoss).HasPrecision(18, 8);
                entity.Property(x => x.UnrealizedProfitLoss).HasPrecision(18, 8);
            });
        }

        private static void ConfigurePaperOrder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaperOrder>(entity =>
            {
                entity.ToTable("AppPaperOrders");
                entity.HasIndex(x => new { x.AccountId, x.SubmittedAt });
                entity.HasIndex(x => new { x.UserId, x.Status });
                entity.HasIndex(x => new { x.Symbol, x.Provider, x.SubmittedAt });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(PaperOrder.MaxSymbolLength);
                entity.Property(x => x.Notes).HasMaxLength(PaperOrder.MaxNotesLength);

                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.OrderType).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.RequestedPrice).HasPrecision(18, 8);
                entity.Property(x => x.ExecutedPrice).HasPrecision(18, 8);
                entity.Property(x => x.StopLoss).HasPrecision(18, 8);
                entity.Property(x => x.TakeProfit).HasPrecision(18, 8);
            });
        }

        private static void ConfigurePaperPosition(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaperPosition>(entity =>
            {
                entity.ToTable("AppPaperPositions");
                entity.HasIndex(x => new { x.AccountId, x.Status });
                entity.HasIndex(x => new { x.AccountId, x.Symbol, x.Provider, x.Status });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(PaperPosition.MaxSymbolLength);

                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.AverageEntryPrice).HasPrecision(18, 8);
                entity.Property(x => x.CurrentMarketPrice).HasPrecision(18, 8);
                entity.Property(x => x.RealizedProfitLoss).HasPrecision(18, 8);
                entity.Property(x => x.UnrealizedProfitLoss).HasPrecision(18, 8);
                entity.Property(x => x.StopLoss).HasPrecision(18, 8);
                entity.Property(x => x.TakeProfit).HasPrecision(18, 8);
            });
        }

        private static void ConfigurePaperTradeFill(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaperTradeFill>(entity =>
            {
                entity.ToTable("AppPaperTradeFills");
                entity.HasIndex(x => new { x.AccountId, x.ExecutedAt });
                entity.HasIndex(x => new { x.OrderId, x.ExecutedAt });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(PaperTradeFill.MaxSymbolLength);

                entity.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.Price).HasPrecision(18, 8);
                entity.Property(x => x.RealizedProfitLoss).HasPrecision(18, 8);
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
                entity.Property(x => x.CurrentIntroLessonKey).HasMaxLength(UserProfile.MaxAcademyLessonKeysLength);
                entity.Property(x => x.CompletedIntroLessonKeys).HasMaxLength(UserProfile.MaxAcademyLessonKeysLength);
                entity.Property(x => x.AcademyStage).HasConversion<string>().HasMaxLength(32);

                entity.Property(x => x.RiskTolerance).HasPrecision(10, 4);
                entity.Property(x => x.BehavioralRiskScore).HasPrecision(10, 4);
                entity.Property(x => x.BestIntroQuizScore).HasPrecision(10, 2);
            });
        }

        private static void ConfigureAcademyQuizAttempt(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcademyQuizAttempt>(entity =>
            {
                entity.ToTable("AppAcademyQuizAttempts");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.CreationTime });
                entity.Property(x => x.CourseKey).IsRequired().HasMaxLength(AcademyQuizAttempt.MaxCourseKeyLength);
                entity.Property(x => x.ScorePercent).HasPrecision(10, 2);
                entity.Property(x => x.RequiredScorePercent).HasPrecision(10, 2);
                entity.Property(x => x.AnswersJson).HasMaxLength(AcademyQuizAttempt.MaxAnswersJsonLength);
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
