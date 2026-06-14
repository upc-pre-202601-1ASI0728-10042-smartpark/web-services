namespace SmartPark.Application.Contracts;

public record SmokeAlertDto(string DetectorId, string ZoneId, int LevelNumber, bool SmokeDetected, double SmokeLevel, string Status, DateTimeOffset LastReading);

/// <summary>Payload de ingesta de alerta de humo desde el simulador IoT o un sensor real.</summary>
public record SmokeAlertIngestDto(string DetectorId, string ZoneId, int LevelNumber, double SmokeLevel, DateTimeOffset DetectedAt, string[] AffectedOccupiedSpaces);
