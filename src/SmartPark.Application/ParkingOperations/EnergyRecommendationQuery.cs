using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;
using SmartPark.Domain.EnergyEfficiency;

namespace SmartPark.Application.ParkingOperations;

public record GetEnergyRecommendationsQuery(int? Level = null);

/// <summary>
/// Caso de uso del bounded context Energy Efficiency: deriva, por zona/nivel, una
/// recomendación de atenuación de iluminación a partir de la ocupación reportada por
/// el gemelo digital. Las zonas con ocupación BAJA se recomiendan atenuar para ahorrar.
/// </summary>
public sealed class EnergyRecommendationHandler(IDigitalTwinGateway gw)
{
    public async Task<IReadOnlyList<EnergyZoneDto>> HandleAsync(GetEnergyRecommendationsQuery q, CancellationToken ct = default)
    {
        var zones = await gw.GetZonesAsync(q.Level, ct);
        return zones.Select(z =>
        {
            var rec = LightingRecommendation.FromOccupancy(z.OccupancyRate);
            return new EnergyZoneDto($"LIGHT-{z.ZoneId}", rec.CurrentLevel, rec.RecommendedLevel, rec.SavingsPercent, rec.Status);
        }).ToList();
    }
}
