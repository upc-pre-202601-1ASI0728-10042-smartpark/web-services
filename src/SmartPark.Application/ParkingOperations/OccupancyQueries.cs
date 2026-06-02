using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;

namespace SmartPark.Application.ParkingOperations;

public record GetOccupancySummaryQuery(string LotId = "LOT-JOCKEY");
public record GetZonesQuery(int? Level = null);

/// <summary>
/// Casos de uso de lectura de ocupación (TS-01), bounded context Parking Operations
/// Monitoring. Delegan en el ACL del gemelo digital. El modo degradado (ADT caído)
/// se reporta con <see cref="OccupancyReadResult"/> para que el cliente muestre el
/// último estado conocido.
/// </summary>
public sealed class OccupancyQueryHandler(IDigitalTwinGateway gw)
{
    public async Task<OccupancyReadResult> GetSummaryAsync(GetOccupancySummaryQuery q, CancellationToken ct = default)
    {
        if (!await gw.IsHealthyAsync(ct))
            return OccupancyReadResult.Degraded();
        return OccupancyReadResult.Ok(await gw.GetLotOccupancyAsync(q.LotId, ct));
    }

    public Task<IReadOnlyList<ZoneOccupancyDto>> GetZonesAsync(GetZonesQuery q, CancellationToken ct = default)
        => gw.GetZonesAsync(q.Level, ct);
}

public sealed record OccupancyReadResult(bool IsDegraded, OccupancySummaryDto? Summary)
{
    public static OccupancyReadResult Ok(OccupancySummaryDto s) => new(false, s);
    public static OccupancyReadResult Degraded() => new(true, null);
}
