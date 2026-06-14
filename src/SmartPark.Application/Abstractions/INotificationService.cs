namespace SmartPark.Application.Abstractions;

/// <summary>Puerto de despacho de notificaciones push a los conductores (implementado en Infraestructura).</summary>
public interface INotificationService
{
    Task SendToTokensAsync(IEnumerable<string> tokens, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default);
}
