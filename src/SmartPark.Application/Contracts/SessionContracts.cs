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
