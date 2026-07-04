using SmartPark.Domain.Common;

namespace SmartPark.Domain.ParkingSession;

/// <summary>
/// Servicio de dominio que calcula el costo de una sesión de estacionamiento al
/// cerrarla: una tarifa base (por ingreso) más una tarifa por hora, cobrando la
/// fracción de hora como hora completa. El resultado se expresa como <see cref="Money"/>.
/// </summary>
public sealed class ParkingCostCalculator
{
    private readonly decimal _baseFee;
    private readonly decimal _hourlyRate;
    private readonly string _currency;

    public ParkingCostCalculator(decimal baseFee = 5.00m, decimal hourlyRate = 3.50m, string currency = "PEN")
    {
        if (baseFee < 0) throw new DomainException("La tarifa base no puede ser negativa.");
        if (hourlyRate < 0) throw new DomainException("La tarifa por hora no puede ser negativa.");
        _baseFee = baseFee;
        _hourlyRate = hourlyRate;
        _currency = currency;
    }

    /// <summary>Calcula el costo total entre el ingreso y la salida del vehículo.</summary>
    public Money Calculate(DateTimeOffset startedAt, DateTimeOffset endedAt)
    {
        if (endedAt < startedAt) throw new DomainException("La salida no puede ser anterior al ingreso.");
        var hours = (decimal)Math.Ceiling((endedAt - startedAt).TotalHours);
        if (hours < 1) hours = 1; // se cobra un mínimo de una hora
        return Money.Of(_baseFee + _hourlyRate * hours, _currency);
    }
}
