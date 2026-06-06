using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPark.Domain.Notifications;

namespace SmartPark.Infrastructure.Persistence.Configurations;

public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> b)
    {
        b.ToTable("device_tokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.Token).HasMaxLength(512).IsRequired();
        b.Property(t => t.Platform).HasMaxLength(32);
        b.HasIndex(t => new { t.UserId, t.Token }).IsUnique();
    }
}
