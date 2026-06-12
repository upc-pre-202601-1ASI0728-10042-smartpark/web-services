using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPark.Application.Contracts;
using SmartPark.Application.ParkingOperations;

namespace SmartPark.Api.Controllers;

/// <summary>Endpoints de ocupación (TS-01). Bounded Context: Parking Operations Monitoring.</summary>
[ApiController]
[Authorize(Roles = "Operator")]
[Route("api/v1/occupancy")]
public sealed class OccupancyController(OccupancyQueryHandler occupancy, GetSpacesByZoneHandler spaces) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] string lotId = "LOT-JOCKEY", CancellationToken ct = default)
    {
        var result = await occupancy.GetSummaryAsync(new GetOccupancySummaryQuery(lotId), ct);
        if (result.IsDegraded)
            return StatusCode(503, new { degraded = true, message = "Azure Digital Twins no disponible. Mostrar último estado conocido." });
        return Ok(result.Summary);
    }

    [HttpGet("zones")]
    public async Task<ActionResult<IReadOnlyList<ZoneOccupancyDto>>> Zones([FromQuery] int? level = null, CancellationToken ct = default)
        => Ok(await occupancy.GetZonesAsync(new GetZonesQuery(level), ct));

    [HttpGet("zones/{zoneId}/spaces")]
    public async Task<ActionResult<IReadOnlyList<ParkingSpaceDto>>> Spaces(string zoneId, CancellationToken ct = default)
        => Ok(await spaces.HandleAsync(new GetSpacesByZoneQuery(zoneId), ct));
}
