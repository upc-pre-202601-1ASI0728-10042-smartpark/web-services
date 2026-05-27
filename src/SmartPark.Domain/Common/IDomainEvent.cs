namespace SmartPark.Domain.Common;

/// <summary>Marcador de un evento de dominio. Se publica cuando un agregado cambia de estado relevante.</summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
