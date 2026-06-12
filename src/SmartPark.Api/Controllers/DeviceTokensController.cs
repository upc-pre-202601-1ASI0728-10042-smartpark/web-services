using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPark.Application.Notifications;

namespace SmartPark.Api.Controllers;

/// <summary>Registro de device tokens FCM del conductor (TS-05). Bounded Context: Notifications.</summary>
[ApiController]
[Authorize(Roles = "Driver")]
[Route("api/v1/notifications/tokens")]
public sealed class DeviceTokensController(RegisterDeviceTokenHandler handler) : ControllerBase
{
    public record RegisterTokenRequest(string Token, string Platform);

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterTokenRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        await handler.HandleAsync(new RegisterDeviceTokenCommand(userId, req.Token, req.Platform), ct);
        return NoContent();
    }
}
