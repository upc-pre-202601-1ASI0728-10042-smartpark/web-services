using SmartPark.Domain.Common;

namespace SmartPark.Domain.SafetyIncident.Events;

/// <summary>Evento de dominio: se registró una alerta de humo en una zona.</summary>
public sealed record SmokeAlertRaised(Guid IncidentId, string DetectorId, string ZoneId, int LevelNumber, double SmokeLevel) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
