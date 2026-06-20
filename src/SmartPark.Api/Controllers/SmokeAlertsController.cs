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
public sealed class SmokeAlertsController(
    IngestSmokeAlertHandler ingest,
    GetActiveAlertsHandler active,
    IHubContext<AlertsHub> hub,
    IConfiguration config) : ControllerBase
{
    private const string ApiKeyHeader = "X-Api-Key";

    /// <summary>Lista de alertas de humo activas (panel de seguridad del operador).</summary>
    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<IReadOnlyList<SmokeAlertDto>>> Active(CancellationToken ct = default)
        => Ok(await active.HandleAsync(new GetActiveAlertsQuery(), ct));

    /// <summary>
    /// Ingesta de alerta de humo desde el simulador IoT o un sensor real.
    /// Se autentica con una API key compartida (header <c>X-Api-Key</c>); si no
    /// hay clave configurada (entorno local) la ingesta queda abierta.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Ingest([FromBody] SmokeAlertIngestDto alert, CancellationToken ct = default)
    {
        var expectedKey = config["Ingest:ApiKey"];
        if (!string.IsNullOrWhiteSpace(expectedKey))
        {
            var provided = Request.Headers[ApiKeyHeader].ToString();
            if (!CryptographicEquals(provided, expectedKey))
                return Unauthorized(new { message = "API key inválida o ausente." });
        }

        var incidentId = await ingest.HandleAsync(new IngestSmokeAlertCommand(alert), ct);
        await hub.Clients.All.SendAsync("smokeAlert", alert, ct);
        return Accepted(new { received = true, incidentId, alert.DetectorId });
    }

    /// <summary>Comparación en tiempo constante para no filtrar la clave por timing.</summary>
    private static bool CryptographicEquals(string a, string b)
    {
        var ba = System.Text.Encoding.UTF8.GetBytes(a);
        var bb = System.Text.Encoding.UTF8.GetBytes(b);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
