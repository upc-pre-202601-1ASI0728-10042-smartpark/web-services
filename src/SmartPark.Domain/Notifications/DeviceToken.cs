using SmartPark.Domain.Common;

namespace SmartPark.Domain.Notifications;

/// <summary>
/// Entidad del bounded context Notifications: token de dispositivo (FCM) de un conductor,
/// usado para despachar notificaciones push.
/// </summary>
public sealed class DeviceToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public string Platform { get; private set; } = "Android";
    public DateTimeOffset RegisteredAt { get; private set; }

    private DeviceToken() { } // EF Core

    public static DeviceToken Register(Guid userId, string token, string platform)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId es obligatorio.");
        if (string.IsNullOrWhiteSpace(token)) throw new DomainException("El token es obligatorio.");
        return new DeviceToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            Platform = string.IsNullOrWhiteSpace(platform) ? "Android" : platform,
            RegisteredAt = DateTimeOffset.UtcNow,
        };
    }
}
