using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;
using SmartPark.Domain.Notifications;
using SmartPark.Domain.ParkingSession;
using SmartPark.Domain.SafetyIncident;

namespace SmartPark.Application.SafetyIncident;

public record IngestSmokeAlertCommand(SmokeAlertIngestDto Alert);

/// <summary>
/// Caso de uso: ingesta de una alerta de humo (TS-03). Crea el incidente (agregado
/// Safety &amp; Incident), lo persiste, actualiza el twin del detector vía el ACL y
/// despacha notificaciones push a los conductores con vehículo en la zona afectada.
/// </summary>
public sealed class IngestSmokeAlertHandler(
    IIncidentRepository incidents,
    IParkingSessionRepository sessions,
    IDeviceTokenRepository deviceTokens,
    IDigitalTwinGateway gateway,
    INotificationService notifier,
    IUnitOfWork uow)
{
    public async Task<Guid> HandleAsync(IngestSmokeAlertCommand cmd, CancellationToken ct = default)
    {
        var a = cmd.Alert;

        var incident = Incident.Raise(a.DetectorId, a.ZoneId, a.LevelNumber, SmokeReading.Of(a.SmokeLevel));
        incidents.Add(incident);
        await uow.SaveChangesAsync(ct);

        await gateway.UpdateSmokeStateAsync(a.DetectorId, a.SmokeLevel, a.DetectedAt, ct);

        // Notificar a los conductores afectados (vehículo en una plaza de la zona del incidente).
        if (a.AffectedOccupiedSpaces.Length > 0)
        {
            var affected = await sessions.GetActiveByLocationsAsync(a.AffectedOccupiedSpaces, ct);
            var driverIds = affected.Select(s => s.DriverId).Distinct().ToList();
            if (driverIds.Count > 0)
            {
                var tokens = await deviceTokens.GetTokensForUsersAsync(driverIds, ct);
                await notifier.SendToTokensAsync(
                    tokens,
                    "Alerta de seguridad",
                    $"Se detectó humo en la zona {a.ZoneId}. Toma precauciones.",
                    new Dictionary<string, string> { ["zoneId"] = a.ZoneId, ["detectorId"] = a.DetectorId },
                    ct);
            }
        }

        return incident.Id;
    }
}
