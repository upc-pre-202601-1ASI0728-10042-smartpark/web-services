using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPark.Domain.SafetyIncident;

namespace SmartPark.Infrastructure.Persistence.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> b)
    {
        b.ToTable("incidents");
        b.HasKey(i => i.Id);
        b.Ignore(i => i.DomainEvents);
        b.Property(i => i.DetectorId).HasMaxLength(64).IsRequired();
        b.Property(i => i.ZoneId).HasMaxLength(64).IsRequired();
        b.Property(i => i.Reading)
            .HasColumnName("smoke_ppm")
            .HasConversion(r => r.Ppm, p => SmokeReading.Of(p));
        b.Property(i => i.Status).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(i => i.DetectedAt);
    }
}
