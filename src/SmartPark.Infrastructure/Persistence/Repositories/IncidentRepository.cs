using Microsoft.EntityFrameworkCore;
using SmartPark.Domain.SafetyIncident;

namespace SmartPark.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository(SmartParkDbContext db) : IIncidentRepository
{
    public async Task<IReadOnlyList<Incident>> GetActiveAsync(CancellationToken ct = default)
        => await db.Incidents.Where(i => i.Status != IncidentStatus.Resolved)
                             .OrderByDescending(i => i.DetectedAt).ToListAsync(ct);

    public Task<Incident?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Incidents.SingleOrDefaultAsync(i => i.Id == id, ct);

    public void Add(Incident incident) => db.Incidents.Add(incident);
}
