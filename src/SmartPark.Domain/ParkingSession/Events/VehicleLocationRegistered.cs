using SmartPark.Domain.Common;

namespace SmartPark.Domain.ParkingSession.Events;

public sealed record VehicleLocationRegistered(Guid SessionId, string SpaceId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
