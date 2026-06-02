using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;

namespace SmartPark.Application.ParkingOperations;

public record GetSpacesByZoneQuery(string ZoneId);

/// <summary>Caso de uso: detalle de plazas de una zona.</summary>
public sealed class GetSpacesByZoneHandler(IDigitalTwinGateway gw)
{
    public Task<IReadOnlyList<ParkingSpaceDto>> HandleAsync(GetSpacesByZoneQuery q, CancellationToken ct = default)
        => gw.GetSpacesByZoneAsync(q.ZoneId, ct);
}
