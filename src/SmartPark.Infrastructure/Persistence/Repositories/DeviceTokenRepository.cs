using Microsoft.EntityFrameworkCore;
using SmartPark.Domain.Notifications;

namespace SmartPark.Infrastructure.Persistence.Repositories;

public sealed class DeviceTokenRepository(SmartParkDbContext db) : IDeviceTokenRepository
{
    public Task<bool> ExistsAsync(Guid userId, string token, CancellationToken ct = default)
        => db.DeviceTokens.AnyAsync(t => t.UserId == userId && t.Token == token, ct);

    public async Task<IReadOnlyList<string>> GetTokensForUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToHashSet();
        return await db.DeviceTokens.Where(t => ids.Contains(t.UserId)).Select(t => t.Token).ToListAsync(ct);
    }

    public void Add(DeviceToken token) => db.DeviceTokens.Add(token);
}
