namespace SmartPark.Domain.Notifications;

/// <summary>Repositorio de NotificationPreferences (puerto del dominio).</summary>
public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByUserAsync(Guid userId, CancellationToken ct = default);
    void Add(NotificationPreferences preferences);
}
