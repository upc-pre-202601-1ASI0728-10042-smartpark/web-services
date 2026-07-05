namespace SmartPark.Application.Contracts;

/// <summary>
/// Recomendación de iluminación por zona derivada de la ocupación (TS-08). Contrato
/// consumido por la Web App del operador.
/// </summary>
public record EnergyZoneDto(
    string ZoneId,
    string Code,
    int LevelNumber,
    int OccupiedSpaces,
    int TotalSpaces,
    double OccupancyRate,
    int CurrentLightingLevel,
    int RecommendedLightingLevel,
    double EstimatedSavingsKwh,
    string Action);
