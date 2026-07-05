namespace SmartPark.Domain.ParkingSession;

/// <summary>Repositorio del agregado ParkingSession (puerto del dominio).</summary>
public interface IParkingSessionRepository
{
    Task<ParkingSession?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default);
    Task<ParkingSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ParkingSession>> GetByDriverAsync(Guid driverId, CancellationToken ct = default);
    Task<IReadOnlyList<ParkingSession>> GetActiveByLocationsAsync(IEnumerable<string> spaceIds, CancellationToken ct = default);
    Task<IReadOnlyList<ParkingSession>> GetAllAsync(CancellationToken ct = default);
    void Add(ParkingSession session);
}
