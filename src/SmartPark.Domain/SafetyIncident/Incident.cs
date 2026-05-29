using SmartPark.Domain.Common;
using SmartPark.Domain.SafetyIncident.Events;

namespace SmartPark.Domain.SafetyIncident;

/// <summary>
/// Agregado raíz del bounded context Safety &amp; Incident. Representa un incidente de
/// humo detectado en una zona, con su ciclo de vida (Alert → Confirmed → Resolved).
/// </summary>
public sealed class Incident : AggregateRoot<Guid>
{
    public string DetectorId { get; private set; } = default!;
    public string ZoneId { get; private set; } = default!;
    public int LevelNumber { get; private set; }
    public SmokeReading Reading { get; private set; } = default!;
    public IncidentStatus Status { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    private Incident() { } // EF Core

    private Incident(Guid id, string detectorId, string zoneId, int level, SmokeReading reading) : base(id)
    {
        DetectorId = detectorId;
        ZoneId = zoneId;
        LevelNumber = level;
        Reading = reading;
        Status = IncidentStatus.Alert;
        DetectedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Factory: registra una alerta de humo y emite el evento de dominio.</summary>
    public static Incident Raise(string detectorId, string zoneId, int level, SmokeReading reading)
    {
        if (string.IsNullOrWhiteSpace(detectorId)) throw new DomainException("DetectorId es obligatorio.");
        if (string.IsNullOrWhiteSpace(zoneId)) throw new DomainException("ZoneId es obligatorio.");

        var incident = new Incident(Guid.NewGuid(), detectorId, zoneId, level, reading);
        incident.Raise(new SmokeAlertRaised(incident.Id, detectorId, zoneId, level, reading.Ppm));
        return incident;
    }

    /// <summary>El operador toma conocimiento del incidente.</summary>
    public void Confirm()
    {
        if (Status == IncidentStatus.Resolved) throw new DomainException("Un incidente resuelto no puede confirmarse.");
        Status = IncidentStatus.Confirmed;
    }

    /// <summary>Cierra el incidente.</summary>
    public void Resolve()
    {
        if (Status == IncidentStatus.Resolved) return;
        Status = IncidentStatus.Resolved;
        ResolvedAt = DateTimeOffset.UtcNow;
        Raise(new IncidentResolved(Id, DetectorId));
    }
}
