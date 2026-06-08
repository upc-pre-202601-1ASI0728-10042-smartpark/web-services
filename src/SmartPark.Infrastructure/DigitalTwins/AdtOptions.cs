namespace SmartPark.Infrastructure.DigitalTwins;

/// <summary>Configuración del data-plane de Azure Digital Twins (sección "Adt" de appsettings).</summary>
public sealed class AdtOptions
{
    /// <summary>Host: &lt;nombre&gt;.api.&lt;region&gt;.digitaltwins.azure.net</summary>
    public string HostName { get; set; } = default!;
}
