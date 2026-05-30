using SmartPark.Domain.Common;

namespace SmartPark.Domain.ParkingSession;

/// <summary>Objeto de valor: ubicación del vehículo (dtId de la plaza, ej. SPACE-L1-A03).</summary>
public sealed class VehicleLocation : ValueObject
{
    public string SpaceId { get; }

    private VehicleLocation(string spaceId) => SpaceId = spaceId;

    public static VehicleLocation Of(string spaceId)
    {
        if (string.IsNullOrWhiteSpace(spaceId)) throw new DomainException("La ubicación del vehículo es obligatoria.");
        return new VehicleLocation(spaceId.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return SpaceId; }

    public override string ToString() => SpaceId;
}
