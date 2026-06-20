namespace SmartPark.Application.Abstractions;

/// <summary>
/// Capacidad opcional de simulación de ocupación, expuesta por el gemelo de
/// demostración para reflejar entradas/salidas de vehículos sin un ADT real.
/// </summary>
public interface IOccupancySimulator
{
    /// <summary>Aplica un "tick": algunos vehículos entran y otros salen.</summary>
    void SimulateTick();
}
