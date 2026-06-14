namespace SmartPark.Infrastructure.Notifications;

public sealed class FcmOptions
{
    /// <summary>Ruta a la service account de Firebase. Si está vacía, se simula el envío (dev).</summary>
    public string ServiceAccountPath { get; set; } = string.Empty;
    public string ProjectId { get; set; } = "smartpark";
}
