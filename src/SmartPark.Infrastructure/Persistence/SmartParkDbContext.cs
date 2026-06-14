using Microsoft.EntityFrameworkCore;
using SmartPark.Application.Abstractions;
using SmartPark.Domain.IdentityAccess;
using SmartPark.Domain.Notifications;
using SmartPark.Domain.ParkingSession;
using SmartPark.Domain.SafetyIncident;

namespace SmartPark.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core para los datos transaccionales que NO pertenecen al gemelo
/// digital (usuarios, incidentes, sesiones y device tokens). Actúa como Unit of Work.
/// El estado espacial del estacionamiento vive en Azure Digital Twins.
/// </summary>
public sealed class SmartParkDbContext(DbContextOptions<SmartParkDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartParkDbContext).Assembly);
}
