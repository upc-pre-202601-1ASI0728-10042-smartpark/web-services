namespace SmartPark.Domain.Notifications;

/// <summary>Repositorio de DeviceToken (puerto del dominio).</summary>
public interface IDeviceTokenRepository
{
    Task<bool> ExistsAsync(Guid userId, string token, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetTokensForUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    void Add(DeviceToken token);
}
