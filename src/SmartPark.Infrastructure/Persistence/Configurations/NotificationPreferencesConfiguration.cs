using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPark.Domain.Notifications;

namespace SmartPark.Infrastructure.Persistence.Configurations;

public sealed class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> b)
    {
        b.ToTable("notification_preferences");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("user_id").ValueGeneratedNever();
        b.Property(p => p.SmokeAlerts).IsRequired();
        b.Property(p => p.AvailabilityAlerts).IsRequired();
        b.Property(p => p.SessionReminders).IsRequired();
        b.Property(p => p.Promotions).IsRequired();
    }
}
