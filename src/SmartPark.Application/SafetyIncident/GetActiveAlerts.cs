using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;

namespace SmartPark.Application.SafetyIncident;

public record GetActiveAlertsQuery;

/// <summary>Caso de uso: lista de alertas de humo activas (panel de seguridad).</summary>
public sealed class GetActiveAlertsHandler(IDigitalTwinGateway gw)
{
    public Task<IReadOnlyList<SmokeAlertDto>> HandleAsync(GetActiveAlertsQuery query, CancellationToken ct = default)
        => gw.GetActiveSmokeAlertsAsync(ct);
}
