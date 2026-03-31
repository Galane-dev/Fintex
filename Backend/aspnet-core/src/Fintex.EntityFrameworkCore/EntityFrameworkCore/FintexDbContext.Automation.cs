using Fintex.Investments.Automation;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore
{
    /// <summary>
    /// Trade-automation DbSet registrations and EF model configuration.
    /// </summary>
    public partial class FintexDbContext
    {
        public DbSet<TradeAutomationRule> TradeAutomationRules { get; set; }

        private static void ConfigureTradeAutomationRule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TradeAutomationRule>(entity =>
            {
                entity.ToTable("AppTradeAutomationRules");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive });
                entity.HasIndex(x => new { x.UserId, x.Symbol, x.Provider, x.TriggerType, x.TargetMetricValue });

                entity.Property(x => x.Name).IsRequired().HasMaxLength(TradeAutomationRule.MaxNameLength);
                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(TradeAutomationRule.MaxSymbolLength);
                entity.Property(x => x.Notes).HasMaxLength(TradeAutomationRule.MaxNotesLength);

                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.TriggerType).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.TargetVerdict).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Destination).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.TradeDirection).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.CreatedMetricValue).HasPrecision(18, 8);
                entity.Property(x => x.LastObservedMetricValue).HasPrecision(18, 8);
                entity.Property(x => x.TargetMetricValue).HasPrecision(18, 8);
                entity.Property(x => x.MinimumConfidenceScore).HasPrecision(10, 4);
                entity.Property(x => x.Quantity).HasPrecision(18, 8);
                entity.Property(x => x.StopLoss).HasPrecision(18, 8);
                entity.Property(x => x.TakeProfit).HasPrecision(18, 8);
            });
        }
    }
}
