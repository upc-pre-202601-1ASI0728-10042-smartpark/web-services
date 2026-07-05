using SmartPark.Application.Abstractions;
using SmartPark.Domain.Notifications;
using Prefs = SmartPark.Domain.Notifications.NotificationPreferences;

namespace SmartPark.Application.Notifications;

/// <summary>Preferencias de notificación del conductor (TS-05).</summary>
public record NotificationPreferencesDto(
    bool SmokeAlerts,
    bool AvailabilityAlerts,
    bool SessionReminders,
    bool Promotions);

public record GetNotificationPreferencesQuery(Guid UserId);
public record UpdateNotificationPreferencesCommand(Guid UserId, NotificationPreferencesDto Preferences);

/// <summary>
/// Caso de uso: devuelve las preferencias guardadas del conductor; si aún no ha
/// guardado ninguna, todas las preferencias se consideran activas por defecto.
/// </summary>
public sealed class GetNotificationPreferencesHandler(INotificationPreferencesRepository repo)
{
    public async Task<NotificationPreferencesDto> HandleAsync(GetNotificationPreferencesQuery q, CancellationToken ct = default)
    {
        var prefs = await repo.GetByUserAsync(q.UserId, ct);
        return prefs is null
            ? new NotificationPreferencesDto(true, true, true, true)
            : new NotificationPreferencesDto(prefs.SmokeAlerts, prefs.AvailabilityAlerts, prefs.SessionReminders, prefs.Promotions);
    }
}

/// <summary>Caso de uso: guarda (upsert) las preferencias de notificación del conductor.</summary>
public sealed class UpdateNotificationPreferencesHandler(INotificationPreferencesRepository repo, IUnitOfWork uow)
{
    public async Task<NotificationPreferencesDto> HandleAsync(UpdateNotificationPreferencesCommand cmd, CancellationToken ct = default)
    {
        var d = cmd.Preferences;
        var prefs = await repo.GetByUserAsync(cmd.UserId, ct);
        if (prefs is null)
        {
            prefs = Prefs.CreateDefault(cmd.UserId);
            repo.Add(prefs);
        }

        prefs.Update(d.SmokeAlerts, d.AvailabilityAlerts, d.SessionReminders, d.Promotions);
        await uow.SaveChangesAsync(ct);

        return new NotificationPreferencesDto(prefs.SmokeAlerts, prefs.AvailabilityAlerts, prefs.SessionReminders, prefs.Promotions);
    }
}
