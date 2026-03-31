using Fintex.Investments.Goals;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore
{
    public partial class FintexDbContext
    {
        public DbSet<GoalTarget> GoalTargets { get; set; }
        public DbSet<GoalEvaluationRun> GoalEvaluationRuns { get; set; }
        public DbSet<GoalExecutionPlan> GoalExecutionPlans { get; set; }
        public DbSet<GoalExecutionEvent> GoalExecutionEvents { get; set; }

        private static void ConfigureGoalTarget(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalTarget>(entity =>
            {
                entity.ToTable("AppGoalTargets");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.Status });
                entity.HasIndex(x => new { x.UserId, x.MarketSymbol, x.AccountType, x.DeadlineUtc });

                entity.Property(x => x.Name).IsRequired().HasMaxLength(GoalTarget.MaxNameLength);
                entity.Property(x => x.MarketSymbol).IsRequired().HasMaxLength(GoalTarget.MaxSymbolLength);
                entity.Property(x => x.AllowedSymbols).IsRequired().HasMaxLength(GoalTarget.MaxSymbolsLength);
                entity.Property(x => x.StatusReason).HasMaxLength(GoalTarget.MaxSummaryLength);
                entity.Property(x => x.LatestPlanSummary).HasMaxLength(GoalTarget.MaxSummaryLength);
                entity.Property(x => x.LatestNextAction).HasMaxLength(GoalTarget.MaxSummaryLength);
                entity.Property(x => x.LastError).HasMaxLength(GoalTarget.MaxSummaryLength);

                entity.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.TargetType).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.TradingSession).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

                entity.Property(x => x.StartEquity).HasPrecision(18, 8);
                entity.Property(x => x.CurrentEquity).HasPrecision(18, 8);
                entity.Property(x => x.TargetEquity).HasPrecision(18, 8);
                entity.Property(x => x.TargetPercent).HasPrecision(10, 4);
                entity.Property(x => x.MaxAcceptableRisk).HasPrecision(10, 4);
                entity.Property(x => x.MaxDrawdownPercent).HasPrecision(10, 4);
                entity.Property(x => x.MaxPositionSizePercent).HasPrecision(10, 4);
                entity.Property(x => x.ProgressPercent).HasPrecision(10, 4);
                entity.Property(x => x.RequiredDailyGrowthPercent).HasPrecision(10, 4);
            });
        }

        private static void ConfigureGoalEvaluationRun(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalEvaluationRun>(entity =>
            {
                entity.ToTable("AppGoalEvaluationRuns");
                entity.HasIndex(x => new { x.GoalTargetId, x.OccurredAtUtc });
                entity.HasIndex(x => new { x.UserId, x.GoalStatus, x.OccurredAtUtc });

                entity.Property(x => x.GoalStatus).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Summary).HasMaxLength(GoalEvaluationRun.MaxSummaryLength);
                entity.Property(x => x.CurrentEquity).HasPrecision(18, 8);
                entity.Property(x => x.RequiredGrowthPercent).HasPrecision(10, 4);
                entity.Property(x => x.RequiredDailyGrowthPercent).HasPrecision(10, 4);
                entity.Property(x => x.FeasibilityScore).HasPrecision(10, 4);
                entity.Property(x => x.CounterProposalTargetEquity).HasPrecision(18, 8);
                entity.Property(x => x.CounterProposalTargetPercent).HasPrecision(10, 4);
            });
        }

        private static void ConfigureGoalExecutionPlan(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalExecutionPlan>(entity =>
            {
                entity.ToTable("AppGoalExecutionPlans");
                entity.HasIndex(x => new { x.GoalTargetId, x.GeneratedAtUtc });
                entity.HasIndex(x => new { x.UserId, x.ExecutionSymbol, x.GeneratedAtUtc });

                entity.Property(x => x.ExecutionSymbol).HasMaxLength(GoalExecutionPlan.MaxSymbolLength);
                entity.Property(x => x.SuggestedDirection).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Summary).HasMaxLength(GoalExecutionPlan.MaxSummaryLength);
                entity.Property(x => x.NextAction).HasMaxLength(GoalExecutionPlan.MaxSummaryLength);
                entity.Property(x => x.SuggestedQuantity).HasPrecision(18, 8);
                entity.Property(x => x.SuggestedStopLoss).HasPrecision(18, 8);
                entity.Property(x => x.SuggestedTakeProfit).HasPrecision(18, 8);
                entity.Property(x => x.RiskScore).HasPrecision(10, 4);
            });
        }

        private static void ConfigureGoalExecutionEvent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalExecutionEvent>(entity =>
            {
                entity.ToTable("AppGoalExecutionEvents");
                entity.HasIndex(x => new { x.GoalTargetId, x.OccurredAtUtc });
                entity.HasIndex(x => new { x.UserId, x.EventType, x.OccurredAtUtc });

                entity.Property(x => x.EventType).IsRequired().HasMaxLength(GoalExecutionEvent.MaxTypeLength);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(GoalExecutionEvent.MaxStatusLength);
                entity.Property(x => x.Summary).IsRequired().HasMaxLength(GoalExecutionEvent.MaxSummaryLength);
                entity.Property(x => x.EquityAfterExecution).HasPrecision(18, 8);
            });
        }
    }
}
