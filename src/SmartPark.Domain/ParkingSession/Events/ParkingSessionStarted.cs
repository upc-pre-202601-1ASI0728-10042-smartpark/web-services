using SmartPark.Domain.Common;

namespace SmartPark.Domain.ParkingSession.Events;

public sealed record ParkingSessionStarted(Guid SessionId, Guid DriverId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
