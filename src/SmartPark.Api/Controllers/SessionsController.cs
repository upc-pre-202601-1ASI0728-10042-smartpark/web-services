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
[Authorize]
[Route("api/v1/sessions")]
public sealed class SessionsController(
    StartParkingSessionHandler start,
    RegisterVehicleLocationHandler location,
    FinalizeParkingSessionHandler finalize,
    GetSessionHistoryHandler history,
    GetSessionSummaryHandler summary,
    GetActiveSessionHandler activeSession) : ControllerBase
{
    public record RegisterLocationRequest(string SpaceId);
    public record StartSessionRequest(string? Plate, string? ZoneId, string? SpaceId, string? LotId);

    private Guid DriverId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Resumen de flujo de vehículos del lote para el panel del operador.</summary>
    [HttpGet("summary")]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<SessionSummaryDto>> Summary(CancellationToken ct)
        => Ok(await summary.HandleAsync(new GetSessionSummaryQuery(), ct));

    /// <summary>Sesión de estacionamiento activa del conductor autenticado (204 si no tiene ninguna).</summary>
    [HttpGet("active")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<SessionDto>> Active(CancellationToken ct)
    {
        var session = await activeSession.HandleAsync(new GetActiveSessionQuery(DriverId), ct);
        return session is null ? NoContent() : Ok(session);
    }

    /// <summary>Inicia una sesión de estacionamiento (ingreso del vehículo).</summary>
    [HttpPost]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> Start([FromBody] StartSessionRequest? req, CancellationToken ct)
    {
        try
        {
            var id = await start.HandleAsync(new StartParkingSessionCommand(DriverId, req?.Plate, req?.SpaceId, req?.ZoneId), ct);
            return Created($"/api/v1/sessions/{id}", new { sessionId = id });
        }
        catch (DomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Registra o actualiza la ubicación (plaza) del vehículo.</summary>
    [HttpPatch("{id:guid}/location")]
    [Authorize(Roles = "Driver")]
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
    [Authorize(Roles = "Driver")]
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
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<IReadOnlyList<SessionHistoryItemDto>>> History(CancellationToken ct)
        => Ok(await history.HandleAsync(new GetSessionHistoryQuery(DriverId), ct));
}
