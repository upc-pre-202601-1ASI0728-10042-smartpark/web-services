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
    /// <summary>Umbral de ocupación por debajo del cual se recomienda atenuar (35 %).</summary>
    public const double LowOccupancyThreshold = 0.35;

    /// <summary>Nivel de iluminación de referencia de una zona a pleno (100 %).</summary>
    public const int FullLightingLevel = 100;

    /// <summary>Consumo base de referencia por zona (kWh/h) usado para estimar el ahorro.</summary>
    public const double BaseKwhPerHour = 2.5;

    public int CurrentLightingLevel { get; }
    public int RecommendedLightingLevel { get; }
    public double EstimatedSavingsKwh { get; }
    public string Action { get; }

    private LightingRecommendation(int recommendedLightingLevel, string action)
    {
        CurrentLightingLevel = FullLightingLevel;
        RecommendedLightingLevel = recommendedLightingLevel;
        Action = action;
        EstimatedSavingsKwh = Math.Round(
            (CurrentLightingLevel - RecommendedLightingLevel) / 100.0 * BaseKwhPerHour, 2);
    }

    /// <summary>
    /// Deriva la recomendación a partir de la tasa de ocupación (0..1) de la zona.
    /// Zona vacía → apagar al mínimo de seguridad; ocupación baja → atenuar; en otro
    /// caso → mantener al 100 %.
    /// </summary>
    public static LightingRecommendation FromOccupancy(double occupancyRate, double lowOccupancyThreshold = LowOccupancyThreshold)
    {
        if (occupancyRate < 0) occupancyRate = 0;
        if (occupancyRate <= 0.0001)
            return new LightingRecommendation(10, "Off");
        if (occupancyRate < lowOccupancyThreshold)
            return new LightingRecommendation(50, "Dim");
        return new LightingRecommendation(FullLightingLevel, "Maintain");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CurrentLightingLevel;
        yield return RecommendedLightingLevel;
        yield return Action;
    }
}
