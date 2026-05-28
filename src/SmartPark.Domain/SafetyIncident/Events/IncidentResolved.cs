using SmartPark.Domain.Common;

namespace SmartPark.Domain.SafetyIncident.Events;

/// <summary>Evento de dominio: un incidente fue resuelto.</summary>
public sealed record IncidentResolved(Guid IncidentId, string DetectorId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
