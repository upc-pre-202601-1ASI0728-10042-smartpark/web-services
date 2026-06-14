using SmartPark.Domain.Common;

namespace SmartPark.Domain.IdentityAccess.Events;

/// <summary>Evento de dominio: una nueva cuenta de usuario fue registrada.</summary>
public sealed record UserRegistered(Guid UserId, string Email, UserRole Role) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
