using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;
using SmartPark.Domain.Common;
using SmartPark.Domain.ParkingSession;
using PSession = SmartPark.Domain.ParkingSession.ParkingSession;

namespace SmartPark.Application.ParkingOperations;

public record StartParkingSessionCommand(Guid DriverId, string? Plate = null, string? SpaceId = null, string? ZoneId = null);
public record RegisterVehicleLocationCommand(Guid SessionId, Guid DriverId, string SpaceId);
public record FinalizeParkingSessionCommand(Guid SessionId, Guid DriverId);
public record GetSessionHistoryQuery(Guid DriverId);
public record GetSessionSummaryQuery();
public record GetActiveSessionQuery(Guid DriverId);

/// <summary>Identificador del único lote de estacionamiento gestionado por SmartPark.</summary>
internal static class ParkingLot
{
    public const string Id = "LOT-SP-01";
}

/// <summary>
/// Caso de uso: inicia una sesión de estacionamiento (ingreso del vehículo) para el
/// conductor autenticado. Un conductor no puede tener dos sesiones activas a la vez.
/// </summary>
public sealed class StartParkingSessionHandler(IParkingSessionRepository sessions, IUnitOfWork uow, IDigitalTwinGateway twins)
{
    public async Task<Guid> HandleAsync(StartParkingSessionCommand cmd, CancellationToken ct = default)
    {
        if (await sessions.GetActiveByDriverAsync(cmd.DriverId, ct) is not null)
            throw new DomainException("El conductor ya tiene una sesión de estacionamiento activa.");

        var session = PSession.Start(cmd.DriverId, cmd.Plate, cmd.SpaceId);
        sessions.Add(session);
        await uow.SaveChangesAsync(ct);
        // Refleja el ingreso del vehículo en la ocupación del gemelo (baja la disponibilidad).
        if (!string.IsNullOrWhiteSpace(cmd.SpaceId))
            await twins.SetSpaceOccupancyAsync(cmd.ZoneId, cmd.SpaceId, true, ct);
        return session.Id;
    }
}

/// <summary>Caso de uso: registra o actualiza la ubicación (plaza) del vehículo.</summary>
public sealed class RegisterVehicleLocationHandler(IParkingSessionRepository sessions, IUnitOfWork uow)
{
    public async Task HandleAsync(RegisterVehicleLocationCommand cmd, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdAsync(cmd.SessionId, ct)
            ?? throw new DomainException("Sesión de estacionamiento no encontrada.");
        if (session.DriverId != cmd.DriverId)
            throw new DomainException("La sesión no pertenece al conductor.");

        session.RegisterVehicleLocation(VehicleLocation.Of(cmd.SpaceId));
        await uow.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Caso de uso: finaliza la sesión (salida del vehículo), calcula el costo con el
/// servicio de dominio <see cref="ParkingCostCalculator"/> y devuelve el comprobante.
/// </summary>
public sealed class FinalizeParkingSessionHandler(
    IParkingSessionRepository sessions,
    ParkingCostCalculator calculator,
    IUnitOfWork uow,
    IDigitalTwinGateway twins)
{
    public async Task<SessionReceiptDto> HandleAsync(FinalizeParkingSessionCommand cmd, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdAsync(cmd.SessionId, ct)
            ?? throw new DomainException("Sesión de estacionamiento no encontrada.");
        if (session.DriverId != cmd.DriverId)
            throw new DomainException("La sesión no pertenece al conductor.");
        if (!session.IsActive)
            throw new DomainException("La sesión ya fue finalizada.");

        var cost = calculator.Calculate(session.StartedAt, DateTimeOffset.UtcNow);
        session.Finalize(cost);
        await uow.SaveChangesAsync(ct);
        // Libera la plaza en la ocupación del gemelo (sube la disponibilidad).
        if (session.VehicleLocation is not null)
            await twins.SetSpaceOccupancyAsync(null, session.VehicleLocation.SpaceId, false, ct);

        var endedAt = session.EndedAt!.Value;
        return new SessionReceiptDto(
            session.Id, session.StartedAt, endedAt,
            Math.Round((endedAt - session.StartedAt).TotalMinutes, 2),
            session.AccumulatedCost.Amount, session.AccumulatedCost.Currency);
    }
}

/// <summary>Caso de uso: historial de sesiones del conductor (costo y duración).</summary>
public sealed class GetSessionHistoryHandler(IParkingSessionRepository sessions)
{
    public async Task<IReadOnlyList<SessionHistoryItemDto>> HandleAsync(GetSessionHistoryQuery q, CancellationToken ct = default)
    {
        var list = await sessions.GetByDriverAsync(q.DriverId, ct);
        return list.Select(s => new SessionHistoryItemDto(
            s.Id,
            s.VehicleLocation?.SpaceId,
            s.StartedAt,
            s.EndedAt,
            s.EndedAt is null ? null : Math.Round((s.EndedAt.Value - s.StartedAt).TotalMinutes, 2),
            s.AccumulatedCost.Amount,
            s.AccumulatedCost.Currency,
            s.IsActive)).ToList();
    }
}

/// <summary>
/// Caso de uso: resumen de flujo de vehículos del lote para el panel del operador
/// (sesiones activas, ingresos/salidas de la última hora y duración media).
/// </summary>
public sealed class GetSessionSummaryHandler(IParkingSessionRepository sessions)
{
    public async Task<SessionSummaryDto> HandleAsync(GetSessionSummaryQuery q, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var since = now.AddMinutes(-60);
        var all = await sessions.GetAllAsync(ct);

        var activeSessions = all.Count(s => s.EndedAt is null);
        var entriesLastHour = all.Count(s => s.StartedAt >= since);
        var exitsLastHour = all.Count(s => s.EndedAt is not null && s.EndedAt.Value >= since);

        var finalized = all.Where(s => s.EndedAt is not null).ToList();
        var averageDurationMinutes = finalized.Count == 0
            ? 0
            : Math.Round(finalized.Average(s => (s.EndedAt!.Value - s.StartedAt).TotalMinutes), 2);

        return new SessionSummaryDto(
            ParkingLot.Id, activeSessions, entriesLastHour, exitsLastHour, averageDurationMinutes, now);
    }
}

/// <summary>
/// Caso de uso: sesión de estacionamiento activa del conductor autenticado. Devuelve
/// <c>null</c> si el conductor no tiene ninguna sesión abierta.
/// </summary>
public sealed class GetActiveSessionHandler(IParkingSessionRepository sessions)
{
    public async Task<SessionDto?> HandleAsync(GetActiveSessionQuery q, CancellationToken ct = default)
    {
        var s = await sessions.GetActiveByDriverAsync(q.DriverId, ct);
        if (s is null) return null;

        var reference = s.EndedAt ?? DateTimeOffset.UtcNow;
        return new SessionDto(
            s.Id.ToString(),
            Plate: s.Plate,
            ZoneId: null,
            ZoneCode: null,
            SpaceCode: s.VehicleLocation?.SpaceId,
            LotId: ParkingLot.Id,
            Status: s.IsActive ? "Active" : "Finalized",
            StartedAt: s.StartedAt,
            EndedAt: s.EndedAt,
            DurationMinutes: (int)Math.Max(0, (reference - s.StartedAt).TotalMinutes),
            AccumulatedCost: s.AccumulatedCost.Amount);
    }
}
