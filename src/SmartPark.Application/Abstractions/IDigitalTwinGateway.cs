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
    Task UpdateSmokeStateAsync(string detectorId, double smokeLevel, DateTimeOffset at, CancellationToken ct = default);
    /// <summary>Marca una plaza como ocupada/libre (refleja el ingreso o salida del
    /// vehículo en la ocupación del gemelo). Si <paramref name="zoneId"/> es nulo se
    /// busca la plaza por código en cualquier zona.</summary>
    Task SetSpaceOccupancyAsync(string? zoneId, string spaceCode, bool occupied, CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
