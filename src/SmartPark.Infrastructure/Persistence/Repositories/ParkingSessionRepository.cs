using Microsoft.EntityFrameworkCore;
using SmartPark.Domain.ParkingSession;

namespace SmartPark.Infrastructure.Persistence.Repositories;

public sealed class ParkingSessionRepository(SmartParkDbContext db) : IParkingSessionRepository
{
    public Task<ParkingSession?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default)
        => db.ParkingSessions.SingleOrDefaultAsync(s => s.DriverId == driverId && s.EndedAt == null, ct);

    public Task<ParkingSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ParkingSessions.SingleOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<ParkingSession>> GetActiveByLocationsAsync(IEnumerable<string> spaceIds, CancellationToken ct = default)
    {
        var ids = spaceIds.Select(s => s.ToUpperInvariant()).ToHashSet();
        return await db.ParkingSessions
            .Where(s => s.EndedAt == null && s.VehicleLocation != null && ids.Contains(s.VehicleLocation.SpaceId))
            .ToListAsync(ct);
    }

    public void Add(ParkingSession session) => db.ParkingSessions.Add(session);
}
