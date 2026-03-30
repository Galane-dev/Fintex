using Fintex.Investments.Strategies;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore
{
    /// <summary>
    /// Strategy-validation DbSet registrations and EF model configuration.
    /// </summary>
    public partial class FintexDbContext
    {
        public DbSet<StrategyValidationRun> StrategyValidationRuns { get; set; }

        private static void ConfigureStrategyValidationRun(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StrategyValidationRun>(entity =>
            {
                entity.ToTable("AppStrategyValidationRuns");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.CreationTime });
                entity.HasIndex(x => new { x.UserId, x.Symbol, x.Provider, x.Outcome });

                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(StrategyValidationRun.MaxSymbolLength);
                entity.Property(x => x.StrategyName).HasMaxLength(StrategyValidationRun.MaxNameLength);
                entity.Property(x => x.Timeframe).HasMaxLength(StrategyValidationRun.MaxTimeframeLength);
                entity.Property(x => x.DirectionPreference).HasMaxLength(StrategyValidationRun.MaxDirectionLength);
                entity.Property(x => x.StrategyText).IsRequired().HasMaxLength(StrategyValidationRun.MaxStrategyLength);
                entity.Property(x => x.MarketVerdict).HasMaxLength(StrategyValidationRun.MaxDirectionLength);
                entity.Property(x => x.NewsSummary).HasMaxLength(StrategyValidationRun.MaxSummaryLength);
                entity.Property(x => x.Summary).HasMaxLength(StrategyValidationRun.MaxSummaryLength);
                entity.Property(x => x.StrengthsJson).HasMaxLength(StrategyValidationRun.MaxListJsonLength);
                entity.Property(x => x.RisksJson).HasMaxLength(StrategyValidationRun.MaxListJsonLength);
                entity.Property(x => x.ImprovementsJson).HasMaxLength(StrategyValidationRun.MaxListJsonLength);
                entity.Property(x => x.SuggestedAction).HasMaxLength(StrategyValidationRun.MaxDirectionLength);
                entity.Property(x => x.AiProvider).HasMaxLength(StrategyValidationRun.MaxProviderLength);
                entity.Property(x => x.AiModel).HasMaxLength(StrategyValidationRun.MaxModelLength);

                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.MarketPrice).HasPrecision(18, 8);
                entity.Property(x => x.MarketTrendScore).HasPrecision(10, 4);
                entity.Property(x => x.MarketConfidenceScore).HasPrecision(10, 4);
                entity.Property(x => x.ValidationScore).HasPrecision(10, 4);
                entity.Property(x => x.SuggestedEntryPrice).HasPrecision(18, 8);
                entity.Property(x => x.SuggestedStopLoss).HasPrecision(18, 8);
                entity.Property(x => x.SuggestedTakeProfit).HasPrecision(18, 8);
            });
        }
    }
}
