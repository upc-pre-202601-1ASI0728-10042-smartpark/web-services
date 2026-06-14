using SmartPark.Application.Contracts;

namespace SmartPark.Application.Abstractions;

/// <summary>
/// Puerto del Anti-Corruption Layer del contexto Digital Twin Synchronization.
/// La capa de aplicación depende SOLO de esta interfaz, nunca del SDK de Azure.
/// </summary>
public interface IDigitalTwinGateway
{
    Task<OccupancySummaryDto> GetLotOccupancyAsync(string lotId, CancellationToken ct = default);
    Task<IReadOnlyList<ZoneOccupancyDto>> GetZonesAsync(int? levelNumber = null, CancellationToken ct = default);
    Task<IReadOnlyList<ParkingSpaceDto>> GetSpacesByZoneAsync(string zoneId, CancellationToken ct = default);
    Task<IReadOnlyList<SmokeAlertDto>> GetActiveSmokeAlertsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EnergyZoneDto>> GetEnergyRecommendationsAsync(CancellationToken ct = default);
    Task UpdateSmokeStateAsync(string detectorId, double smokeLevel, DateTimeOffset at, CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
