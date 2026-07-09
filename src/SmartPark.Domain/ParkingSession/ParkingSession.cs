using SmartPark.Domain.Common;
using SmartPark.Domain.ParkingSession.Events;

namespace SmartPark.Domain.ParkingSession;

/// <summary>
/// Agregado raíz del bounded context Parking Session. Modela el ciclo de vida de la
/// sesión de estacionamiento del conductor (inicio → registro de ubicación → cierre).
/// </summary>
public sealed class ParkingSession : AggregateRoot<Guid>
{
    public Guid DriverId { get; private set; }
    public string? Plate { get; private set; }
    public VehicleLocation? VehicleLocation { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public Money AccumulatedCost { get; private set; } = Money.Zero();

    public bool IsActive => EndedAt is null;

    private ParkingSession() { } // EF Core

    private ParkingSession(Guid id, Guid driverId) : base(id)
    {
        DriverId = driverId;
        StartedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Factory: inicia una sesión de estacionamiento para un conductor. Opcionalmente
    /// registra la placa y la plaza elegida (ingreso directo desde la app móvil).
    /// </summary>
    public static ParkingSession Start(Guid driverId, string? plate = null, string? spaceId = null)
    {
        if (driverId == Guid.Empty) throw new DomainException("DriverId es obligatorio.");
        var session = new ParkingSession(Guid.NewGuid(), driverId);
        session.Plate = string.IsNullOrWhiteSpace(plate) ? null : plate.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(spaceId))
            session.VehicleLocation = VehicleLocation.Of(spaceId);
        session.Raise(new ParkingSessionStarted(session.Id, driverId));
        return session;
    }

    /// <summary>Registra (o actualiza) la ubicación del vehículo.</summary>
    public void RegisterVehicleLocation(VehicleLocation location)
    {
        if (!IsActive) throw new DomainException("No se puede registrar ubicación en una sesión finalizada.");
        VehicleLocation = location;
        Raise(new VehicleLocationRegistered(Id, location.SpaceId));
    }

    /// <summary>Finaliza la sesión y fija el costo total.</summary>
    public void Finalize(Money totalCost)
    {
        if (!IsActive) return;
        AccumulatedCost = totalCost;
        EndedAt = DateTimeOffset.UtcNow;
    }
}
