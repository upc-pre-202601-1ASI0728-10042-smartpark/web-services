namespace SmartPark.Domain.SafetyIncident;

/// <summary>Repositorio del agregado Incident (puerto del dominio).</summary>
public interface IIncidentRepository
{
    Task<IReadOnlyList<Incident>> GetActiveAsync(CancellationToken ct = default);
    Task<Incident?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Incident incident);
}
