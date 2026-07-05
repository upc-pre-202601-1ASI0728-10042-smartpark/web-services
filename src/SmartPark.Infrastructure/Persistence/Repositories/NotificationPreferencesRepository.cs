using Microsoft.EntityFrameworkCore;
using SmartPark.Domain.Notifications;

namespace SmartPark.Infrastructure.Persistence.Repositories;

public sealed class NotificationPreferencesRepository(SmartParkDbContext db) : INotificationPreferencesRepository
{
    public Task<NotificationPreferences?> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => db.NotificationPreferences.SingleOrDefaultAsync(p => p.Id == userId, ct);

    public void Add(NotificationPreferences preferences) => db.NotificationPreferences.Add(preferences);
}
