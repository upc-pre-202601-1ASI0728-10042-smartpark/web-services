using SmartPark.Application.Abstractions;
using SmartPark.Domain.SafetyIncident;

namespace SmartPark.Application.SafetyIncident;

public record AcknowledgeSmokeAlertCommand(string DetectorId);
public record ResolveSmokeAlertCommand(string DetectorId);

/// <summary>
/// Caso de uso: el operador toma conocimiento (confirma) del incidente de humo activo
/// más reciente de un detector. Devuelve <c>false</c> si no hay incidente activo.
/// </summary>
public sealed class AcknowledgeSmokeAlertHandler(IIncidentRepository incidents, IUnitOfWork uow)
{
    public async Task<bool> HandleAsync(AcknowledgeSmokeAlertCommand cmd, CancellationToken ct = default)
    {
        var incident = await incidents.GetLatestActiveByDetectorAsync(cmd.DetectorId, ct);
        if (incident is null) return false;

        incident.Confirm();
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>
/// Caso de uso: el operador cierra (resuelve) el incidente de humo activo más reciente
/// de un detector. Devuelve <c>false</c> si no hay incidente activo.
/// </summary>
public sealed class ResolveSmokeAlertHandler(IIncidentRepository incidents, IUnitOfWork uow)
{
    public async Task<bool> HandleAsync(ResolveSmokeAlertCommand cmd, CancellationToken ct = default)
    {
        var incident = await incidents.GetLatestActiveByDetectorAsync(cmd.DetectorId, ct);
        if (incident is null) return false;

        incident.Resolve();
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
