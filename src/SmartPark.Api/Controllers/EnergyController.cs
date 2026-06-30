using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPark.Application.Contracts;
using SmartPark.Application.ParkingOperations;

namespace SmartPark.Api.Controllers;

/// <summary>Eficiencia energética (TS-08): recomendaciones de iluminación. Bounded Context: Energy Efficiency.</summary>
[ApiController]
[Authorize(Roles = "Operator")]
[Route("api/v1/energy")]
public sealed class EnergyController(EnergyRecommendationHandler recommendations) : ControllerBase
{
    /// <summary>
    /// Recomendación de atenuación de iluminación por zona/nivel, derivada de la
    /// ocupación: las zonas con ocupación baja se recomiendan atenuar para ahorrar.
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<IReadOnlyList<EnergyZoneDto>>> Recommendations([FromQuery] int? level = null, CancellationToken ct = default)
        => Ok(await recommendations.HandleAsync(new GetEnergyRecommendationsQuery(level), ct));
}
