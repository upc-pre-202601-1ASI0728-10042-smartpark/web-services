using SmartPark.Domain.Common;

namespace SmartPark.Domain.Notifications;

/// <summary>
/// Entidad del bounded context Notifications: preferencias de notificación de un
/// conductor. Se identifica por el <see cref="Entity{TId}.Id"/> del usuario (una fila
/// por usuario). Todas las preferencias están activas por defecto.
/// </summary>
public sealed class NotificationPreferences : Entity<Guid>
{
    public bool SmokeAlerts { get; private set; }
    public bool AvailabilityAlerts { get; private set; }
    public bool SessionReminders { get; private set; }
    public bool Promotions { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private NotificationPreferences() { } // EF Core

    /// <summary>Crea las preferencias por defecto (todo activo) para un usuario.</summary>
    public static NotificationPreferences CreateDefault(Guid userId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId es obligatorio.");
        return new NotificationPreferences
        {
            Id = userId,
            SmokeAlerts = true,
            AvailabilityAlerts = true,
            SessionReminders = true,
            Promotions = true,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Actualiza (upsert) las cuatro preferencias del usuario.</summary>
    public void Update(bool smokeAlerts, bool availabilityAlerts, bool sessionReminders, bool promotions)
    {
        SmokeAlerts = smokeAlerts;
        AvailabilityAlerts = availabilityAlerts;
        SessionReminders = sessionReminders;
        Promotions = promotions;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
