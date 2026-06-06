using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPark.Domain.ParkingSession;

namespace SmartPark.Infrastructure.Persistence.Configurations;

public sealed class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> b)
    {
        b.ToTable("parking_sessions");
        b.HasKey(s => s.Id);
        b.Ignore(s => s.DomainEvents);
        b.Ignore(s => s.IsActive);
        b.Property(s => s.VehicleLocation)
            .HasColumnName("vehicle_location")
            .HasMaxLength(64)
            .HasConversion(
                v => v == null ? null : v.SpaceId,
                s => s == null ? null : VehicleLocation.Of(s));
        b.Property(s => s.AccumulatedCost)
            .HasColumnName("accumulated_cost")
            .HasPrecision(10, 2)
            .HasConversion(m => m.Amount, a => Money.Of(a, "PEN"));
    }
}
