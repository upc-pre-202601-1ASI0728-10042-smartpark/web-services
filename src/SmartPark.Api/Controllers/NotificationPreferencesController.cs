using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPark.Application.Notifications;

namespace SmartPark.Api.Controllers;

/// <summary>Preferencias de notificación del conductor (TS-05). Bounded Context: Notifications.</summary>
[ApiController]
[Authorize(Roles = "Driver")]
[Route("api/v1/notifications/preferences")]
public sealed class NotificationPreferencesController(
    GetNotificationPreferencesHandler get,
    UpdateNotificationPreferencesHandler update) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Preferencias guardadas del conductor (todas activas por defecto).</summary>
    [HttpGet]
    public async Task<ActionResult<NotificationPreferencesDto>> Get(CancellationToken ct)
        => Ok(await get.HandleAsync(new GetNotificationPreferencesQuery(UserId), ct));

    /// <summary>Guarda (upsert) las preferencias de notificación del conductor.</summary>
    [HttpPut]
    public async Task<ActionResult<NotificationPreferencesDto>> Update([FromBody] NotificationPreferencesDto prefs, CancellationToken ct)
        => Ok(await update.HandleAsync(new UpdateNotificationPreferencesCommand(UserId, prefs), ct));
}
