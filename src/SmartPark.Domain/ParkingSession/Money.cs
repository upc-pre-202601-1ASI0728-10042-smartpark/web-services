using SmartPark.Domain.Common;

namespace SmartPark.Domain.ParkingSession;

/// <summary>Objeto de valor monetario (monto + moneda). Por defecto soles (PEN).</summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency) { Amount = amount; Currency = currency; }

    public static Money Of(decimal amount, string currency = "PEN")
    {
        if (amount < 0) throw new DomainException("El monto no puede ser negativo.");
        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency);
    }

    public static Money Zero(string currency = "PEN") => new(0, currency);

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Amount; yield return Currency; }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
