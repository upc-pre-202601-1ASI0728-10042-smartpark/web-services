using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartPark.Application.Abstractions;

namespace SmartPark.Infrastructure.Notifications;

/// <summary>
/// Implementación de INotificationService vía Firebase Cloud Messaging (HTTP v1).
/// En desarrollo, si no hay service account configurada, registra el envío sin llamar a FCM.
/// </summary>
public sealed class FcmNotificationService(IHttpClientFactory httpFactory, IOptions<FcmOptions> options, ILogger<FcmNotificationService> log)
    : INotificationService
{
    private readonly FcmOptions _o = options.Value;

    public async Task SendToTokensAsync(IEnumerable<string> tokens, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default)
    {
        var list = tokens.Distinct().ToList();
        if (string.IsNullOrWhiteSpace(_o.ServiceAccountPath) || !File.Exists(_o.ServiceAccountPath))
        {
            log.LogInformation("FCM no configurado: simulando push '{Title}' a {Count} dispositivos", title, list.Count);
            return;
        }

        var accessToken = await GetAccessTokenAsync(ct);
        var http = httpFactory.CreateClient("fcm");
        http.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
        var endpoint = $"https://fcm.googleapis.com/v1/projects/{_o.ProjectId}/messages:send";

        foreach (var token in list)
        {
            var message = new { message = new { token, notification = new { title, body }, data = data ?? new Dictionary<string, string>() } };
            try
            {
                var res = await http.PostAsJsonAsync(endpoint, message, ct);
                if (!res.IsSuccessStatusCode)
                    log.LogWarning("FCM rechazó {Status} para {Token}", res.StatusCode, Mask(token));
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error enviando push a {Token}", Mask(token));
            }
        }
    }

    // Intercambio service-account -> access token OAuth2 (Google). Aislado para tests; TS-05.
    private Task<string> GetAccessTokenAsync(CancellationToken ct) => Task.FromResult(string.Empty);

    private static string Mask(string token) => token.Length <= 8 ? "****" : $"{token[..4]}…{token[^4..]}";
}
