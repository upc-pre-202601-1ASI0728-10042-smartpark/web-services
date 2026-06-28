using Microsoft.EntityFrameworkCore;
using SmartPark.Domain.ParkingSession;

namespace SmartPark.Infrastructure.Persistence.Repositories;

public sealed class ParkingSessionRepository(SmartParkDbContext db) : IParkingSessionRepository
{
    public Task<ParkingSession?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default)
        => db.ParkingSessions.SingleOrDefaultAsync(s => s.DriverId == driverId && s.EndedAt == null, ct);

    public Task<ParkingSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ParkingSessions.SingleOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<ParkingSession>> GetByDriverAsync(Guid driverId, CancellationToken ct = default)
        => await db.ParkingSessions
            .Where(s => s.DriverId == driverId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ParkingSession>> GetActiveByLocationsAsync(IEnumerable<string> spaceIds, CancellationToken ct = default)
    {
        var ids = spaceIds.Select(s => s.ToUpperInvariant()).ToHashSet();
        // VehicleLocation es un value object convertido; el filtro por su SpaceId
        // se evalúa en memoria sobre las sesiones activas (EF no lo traduce a SQL).
        var active = await db.ParkingSessions
            .Where(s => s.EndedAt == null)
            .ToListAsync(ct);
        return active
            .Where(s => s.VehicleLocation != null && ids.Contains(s.VehicleLocation.SpaceId.ToUpperInvariant()))
            .ToList();
    }

    public void Add(ParkingSession session) => db.ParkingSessions.Add(session);
}
