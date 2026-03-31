using Fintex.Investments.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore
{
    /// <summary>
    /// Notification-specific DbSet registrations and EF model configuration.
    /// </summary>
    public partial class FintexDbContext
    {
        public DbSet<NotificationAlertRule> NotificationAlertRules { get; set; }

        public DbSet<NotificationItem> NotificationItems { get; set; }

        private static void ConfigureNotificationAlertRule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationAlertRule>(entity =>
            {
                entity.ToTable("AppNotificationAlertRules");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive });
                entity.HasIndex(x => new { x.UserId, x.Symbol, x.Provider, x.TargetPrice, x.CreatedPrice });

                entity.Property(x => x.Name).IsRequired().HasMaxLength(NotificationAlertRule.MaxNameLength);
                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(NotificationAlertRule.MaxSymbolLength);
                entity.Property(x => x.Notes).HasMaxLength(NotificationAlertRule.MaxNotesLength);

                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.AlertType).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.CreatedPrice).HasPrecision(18, 8);
                entity.Property(x => x.LastObservedPrice).HasPrecision(18, 8);
                entity.Property(x => x.TargetPrice).HasPrecision(18, 8);
            });
        }

        private static void ConfigureNotificationItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationItem>(entity =>
            {
                entity.ToTable("AppNotificationItems");
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead, x.OccurredAt });
                entity.HasIndex(x => new { x.UserId, x.TriggerKey, x.OccurredAt });

                entity.Property(x => x.Title).IsRequired().HasMaxLength(NotificationItem.MaxTitleLength);
                entity.Property(x => x.Message).IsRequired().HasMaxLength(NotificationItem.MaxMessageLength);
                entity.Property(x => x.Symbol).IsRequired().HasMaxLength(NotificationItem.MaxSymbolLength);
                entity.Property(x => x.TriggerKey).IsRequired().HasMaxLength(NotificationItem.MaxTriggerKeyLength);
                entity.Property(x => x.EmailError).HasMaxLength(NotificationItem.MaxErrorLength);
                entity.Property(x => x.ContextJson).HasMaxLength(NotificationItem.MaxContextJsonLength);

                entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
                entity.Property(x => x.Severity).HasConversion<string>().HasMaxLength(16);
                entity.Property(x => x.Verdict).HasConversion<string>().HasMaxLength(16);

                entity.Property(x => x.ReferencePrice).HasPrecision(18, 8);
                entity.Property(x => x.TargetPrice).HasPrecision(18, 8);
                entity.Property(x => x.ConfidenceScore).HasPrecision(10, 4);
            });
        }
    }
}
