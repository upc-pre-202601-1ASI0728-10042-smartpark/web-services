namespace SmartPark.Domain.ParkingSession;

/// <summary>Repositorio del agregado ParkingSession (puerto del dominio).</summary>
public interface IParkingSessionRepository
{
    Task<ParkingSession?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default);
    Task<ParkingSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(ParkingSession session);
}
