using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPark.Domain.IdentityAccess;

namespace SmartPark.Infrastructure.Persistence.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Ignore(u => u.DomainEvents);
        b.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasMaxLength(256).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        b.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
    }
}
