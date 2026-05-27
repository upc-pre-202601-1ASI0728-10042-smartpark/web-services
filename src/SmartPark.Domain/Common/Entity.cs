namespace SmartPark.Domain.Common;

/// <summary>Entidad con identidad propia. La igualdad se define por el Id, no por sus atributos.</summary>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id) => Id = id;
    protected Entity() { }

    public override bool Equals(object? obj)
        => obj is Entity<TId> other && GetType() == other.GetType() && Equals(Id, other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
