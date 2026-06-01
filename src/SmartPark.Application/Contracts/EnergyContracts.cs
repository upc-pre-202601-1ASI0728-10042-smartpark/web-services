namespace SmartPark.Application.Contracts;

public record EnergyZoneDto(string LightingZoneId, double CurrentLevel, double RecommendedLevel, double SavingsPercent, string Status);
