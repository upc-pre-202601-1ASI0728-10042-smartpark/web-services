using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;

namespace SmartPark.Infrastructure.DigitalTwins;

/// <summary>
/// Implementación de demostración del puerto del gemelo digital. Devuelve un
/// grafo de ocupación sembrado (un lote, zonas y espacios) para entornos donde
/// no hay una instancia de Azure Digital Twins accesible (p. ej. el despliegue
/// gratuito en la nube). Se activa con la configuración <c>Adt:Mode = Demo</c>.
/// </summary>
public sealed class DemoDigitalTwinGateway : IDigitalTwinGateway
{
    private const string LotId = "LOT-JOCKEY";

    private static readonly (string Zone, string Code, int Level, int Total, int Occupied, string Congestion)[] Zones =
    {
        ("Z-A", "Zona A", 1, 60, 33, "Low"),
        ("Z-B", "Zona B", 1, 60, 50, "High"),
        ("Z-C", "Zona C", 2, 60, 57, "Full"),
        ("Z-D", "Zona D", 2, 60, 23, "Low"),
    };

    public Task<OccupancySummaryDto> GetLotOccupancyAsync(string lotId, CancellationToken ct = default)
    {
        var total = Zones.Sum(z => z.Total);
        var occ = Zones.Sum(z => z.Occupied);
        return Task.FromResult(new OccupancySummaryDto(
            lotId, total, occ, Math.Round((double)occ / total, 3), DateTimeOffset.UtcNow));
    }

    public Task<IReadOnlyList<ZoneOccupancyDto>> GetZonesAsync(int? levelNumber = null, CancellationToken ct = default)
    {
        var list = Zones
            .Where(z => levelNumber == null || z.Level == levelNumber)
            .Select(z => new ZoneOccupancyDto(
                z.Zone, z.Code, z.Level, z.Total, z.Occupied,
                Math.Round((double)z.Occupied / z.Total, 3), z.Congestion))
            .ToList();
        return Task.FromResult<IReadOnlyList<ZoneOccupancyDto>>(list);
    }

    public Task<IReadOnlyList<ParkingSpaceDto>> GetSpacesByZoneAsync(string zoneId, CancellationToken ct = default)
    {
        var z = Zones.FirstOrDefault(x => string.Equals(x.Zone, zoneId, StringComparison.OrdinalIgnoreCase));
        if (z.Zone is null) return Task.FromResult<IReadOnlyList<ParkingSpaceDto>>(Array.Empty<ParkingSpaceDto>());

        var letter = z.Zone.Replace("Z-", "");
        var reserved = Math.Min(4, z.Total - z.Occupied);
        var spaces = new List<ParkingSpaceDto>(z.Total);
        for (var i = 1; i <= z.Total; i++)
        {
            string state = i <= z.Occupied ? "Occupied"
                : i <= z.Occupied + reserved ? "Reserved"
                : "Free";
            spaces.Add(new ParkingSpaceDto(
                $"{z.Zone}-{i:D2}", $"{letter}-{i:D2}", z.Zone, z.Level,
                state, i % 12 == 0 ? "Disabled" : "Regular", DateTimeOffset.UtcNow));
        }
        return Task.FromResult<IReadOnlyList<ParkingSpaceDto>>(spaces);
    }

    public Task<IReadOnlyList<SmokeAlertDto>> GetActiveSmokeAlertsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SmokeAlertDto>>(Array.Empty<SmokeAlertDto>());

    public Task<IReadOnlyList<EnergyZoneDto>> GetEnergyRecommendationsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<EnergyZoneDto>>(new[]
        {
            new EnergyZoneDto("LZ-A", 80, 55, 31.2, "Optimizable"),
            new EnergyZoneDto("LZ-C", 100, 100, 0, "Optimal"),
        });

    public Task UpdateSmokeStateAsync(string detectorId, double smokeLevel, DateTimeOffset at, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult(true);
}
