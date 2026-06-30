using SmartPark.Domain.Common;

namespace SmartPark.Domain.EnergyEfficiency;

/// <summary>
/// Objeto de valor del bounded context Energy Efficiency: recomendación de atenuación
/// de iluminación para una zona, derivada de su nivel de ocupación. Una zona con
/// ocupación BAJA puede atenuar sus luminarias para ahorrar energía sin afectar el
/// confort de los conductores.
/// </summary>
public sealed class LightingRecommendation : ValueObject
{
    /// <summary>Umbral de ocupación por debajo del cual se recomienda atenuar (30 %).</summary>
    public const double LowOccupancyThreshold = 0.30;

    public double CurrentLevel { get; }
    public double RecommendedLevel { get; }
    public double SavingsPercent { get; }
    public string Status { get; }

    private LightingRecommendation(double currentLevel, double recommendedLevel, string status)
    {
        CurrentLevel = currentLevel;
        RecommendedLevel = recommendedLevel;
        SavingsPercent = Math.Round(currentLevel - recommendedLevel, 2);
        Status = status;
    }

    /// <summary>
    /// Deriva la recomendación a partir de la tasa de ocupación (0..1) de la zona.
    /// Zona vacía → reducir a mínimo de seguridad; ocupación baja → atenuar; en otro
    /// caso → mantener al 100 %.
    /// </summary>
    public static LightingRecommendation FromOccupancy(double occupancyRate, double lowOccupancyThreshold = LowOccupancyThreshold)
    {
        if (occupancyRate < 0) occupancyRate = 0;
        if (occupancyRate <= 0.0001)
            return new LightingRecommendation(100, 20, "ReduceToStandby");
        if (occupancyRate < lowOccupancyThreshold)
            return new LightingRecommendation(100, 50, "Dim");
        return new LightingRecommendation(100, 100, "Optimal");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CurrentLevel;
        yield return RecommendedLevel;
        yield return Status;
    }
}
