using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartPark.Api.Hubs;
using SmartPark.Application.Contracts;
using SmartPark.Application.SafetyIncident;

namespace SmartPark.Api.Controllers;

/// <summary>Alertas de humo (TS-03). Bounded Context: Safety &amp; Incident.</summary>
[ApiController]
[Route("api/v1/alerts/smoke")]
public sealed class SmokeAlertsController(IngestSmokeAlertHandler ingest, GetActiveAlertsHandler active, IHubContext<AlertsHub> hub) : ControllerBase
{
    /// <summary>Lista de alertas de humo activas (panel de seguridad del operador).</summary>
    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<IReadOnlyList<SmokeAlertDto>>> Active(CancellationToken ct = default)
        => Ok(await active.HandleAsync(new GetActiveAlertsQuery(), ct));

    /// <summary>Ingesta de alerta de humo desde el simulador IoT o un sensor real.</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Ingest([FromBody] SmokeAlertIngestDto alert, CancellationToken ct = default)
    {
        var incidentId = await ingest.HandleAsync(new IngestSmokeAlertCommand(alert), ct);
        await hub.Clients.All.SendAsync("smokeAlert", alert, ct);
        return Accepted(new { received = true, incidentId, alert.DetectorId });
    }
}
