using SmartPark.Domain.Common;

namespace SmartPark.Domain.SafetyIncident;

/// <summary>Objeto de valor: lectura de humo en partes por millón (ppm).</summary>
public sealed class SmokeReading : ValueObject
{
    /// <summary>Umbral (ppm) a partir del cual se considera alerta.</summary>
    public const double AlertThresholdPpm = 200;

    public double Ppm { get; }

    private SmokeReading(double ppm) => Ppm = ppm;

    public static SmokeReading Of(double ppm)
    {
        if (ppm < 0) throw new DomainException("La lectura de humo no puede ser negativa.");
        return new SmokeReading(ppm);
    }

    public bool IsAlert => Ppm >= AlertThresholdPpm;

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Ppm; }
}
