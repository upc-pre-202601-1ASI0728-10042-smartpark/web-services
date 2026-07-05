namespace SmartPark.Application.Contracts;

/// <summary>Comprobante devuelto al finalizar (salida) una sesión de estacionamiento.</summary>
public record SessionReceiptDto(
    Guid SessionId,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    double DurationMinutes,
    decimal Cost,
    string Currency);

/// <summary>Entrada del historial de sesiones del conductor.</summary>
public record SessionHistoryItemDto(
    Guid SessionId,
    string? VehicleLocation,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    double? DurationMinutes,
    decimal Cost,
    string Currency,
    bool IsActive);

/// <summary>
/// Resumen de flujo de vehículos del lote para el panel del operador (Web App).
/// </summary>
public record SessionSummaryDto(
    string LotId,
    int ActiveSessions,
    int EntriesLastHour,
    int ExitsLastHour,
    double AverageDurationMinutes,
    DateTimeOffset AsOf);

/// <summary>
/// Sesión de estacionamiento activa del conductor autenticado (Mobile App / Web App).
/// </summary>
public record SessionDto(
    string SessionId,
    string? Plate,
    string? ZoneId,
    string? ZoneCode,
    string? SpaceCode,
    string? LotId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    int DurationMinutes,
    decimal AccumulatedCost);
