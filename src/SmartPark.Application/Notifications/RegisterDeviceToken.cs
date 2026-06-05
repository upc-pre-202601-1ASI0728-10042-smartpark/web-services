using SmartPark.Application.Abstractions;
using SmartPark.Domain.Notifications;

namespace SmartPark.Application.Notifications;

public record RegisterDeviceTokenCommand(Guid UserId, string Token, string Platform);

/// <summary>Caso de uso: registra el device token FCM del conductor (TS-05). Idempotente.</summary>
public sealed class RegisterDeviceTokenHandler(IDeviceTokenRepository tokens, IUnitOfWork uow)
{
    public async Task HandleAsync(RegisterDeviceTokenCommand cmd, CancellationToken ct = default)
    {
        if (await tokens.ExistsAsync(cmd.UserId, cmd.Token, ct))
            return;
        tokens.Add(DeviceToken.Register(cmd.UserId, cmd.Token, cmd.Platform));
        await uow.SaveChangesAsync(ct);
    }
}
