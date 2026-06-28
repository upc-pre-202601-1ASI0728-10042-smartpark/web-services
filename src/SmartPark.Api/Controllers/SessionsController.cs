using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPark.Application.Contracts;
using SmartPark.Application.ParkingOperations;
using SmartPark.Domain.Common;

namespace SmartPark.Api.Controllers;

/// <summary>
/// Gestión de sesiones de estacionamiento del conductor (TS-02): ingreso, ubicación
/// del vehículo, salida (con costo) e historial. Bounded Context: Parking Session.
/// </summary>
[ApiController]
[Authorize(Roles = "Driver")]
[Route("api/v1/sessions")]
public sealed class SessionsController(
    StartParkingSessionHandler start,
    RegisterVehicleLocationHandler location,
    FinalizeParkingSessionHandler finalize,
    GetSessionHistoryHandler history) : ControllerBase
{
    public record RegisterLocationRequest(string SpaceId);

    private Guid DriverId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Inicia una sesión de estacionamiento (ingreso del vehículo).</summary>
    [HttpPost]
    public async Task<IActionResult> Start(CancellationToken ct)
    {
        try
        {
            var id = await start.HandleAsync(new StartParkingSessionCommand(DriverId), ct);
            return Created($"/api/v1/sessions/{id}", new { sessionId = id });
        }
        catch (DomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Registra o actualiza la ubicación (plaza) del vehículo.</summary>
    [HttpPatch("{id:guid}/location")]
    public async Task<IActionResult> Location(Guid id, [FromBody] RegisterLocationRequest req, CancellationToken ct)
    {
        try
        {
            await location.HandleAsync(new RegisterVehicleLocationCommand(id, DriverId, req.SpaceId), ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Finaliza la sesión (salida del vehículo) y devuelve el comprobante con el costo.</summary>
    [HttpPatch("{id:guid}/finalize")]
    public async Task<ActionResult<SessionReceiptDto>> Finalize(Guid id, CancellationToken ct)
    {
        try
        {
            return Ok(await finalize.HandleAsync(new FinalizeParkingSessionCommand(id, DriverId), ct));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Historial de sesiones del conductor con costo y duración.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<SessionHistoryItemDto>>> History(CancellationToken ct)
        => Ok(await history.HandleAsync(new GetSessionHistoryQuery(DriverId), ct));
}
