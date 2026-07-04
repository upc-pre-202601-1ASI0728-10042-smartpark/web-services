using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;
using SmartPark.Domain.Common;
using SmartPark.Domain.ParkingSession;
using PSession = SmartPark.Domain.ParkingSession.ParkingSession;

namespace SmartPark.Application.ParkingOperations;

public record StartParkingSessionCommand(Guid DriverId);
public record RegisterVehicleLocationCommand(Guid SessionId, Guid DriverId, string SpaceId);
public record FinalizeParkingSessionCommand(Guid SessionId, Guid DriverId);
public record GetSessionHistoryQuery(Guid DriverId);

/// <summary>
/// Caso de uso: inicia una sesión de estacionamiento (ingreso del vehículo) para el
/// conductor autenticado. Un conductor no puede tener dos sesiones activas a la vez.
/// </summary>
public sealed class StartParkingSessionHandler(IParkingSessionRepository sessions, IUnitOfWork uow)
{
    public async Task<Guid> HandleAsync(StartParkingSessionCommand cmd, CancellationToken ct = default)
    {
        if (await sessions.GetActiveByDriverAsync(cmd.DriverId, ct) is not null)
            throw new DomainException("El conductor ya tiene una sesión de estacionamiento activa.");

        var session = PSession.Start(cmd.DriverId);
        sessions.Add(session);
        await uow.SaveChangesAsync(ct);
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
    IUnitOfWork uow)
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
