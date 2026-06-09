namespace SmartPark.Infrastructure.Identity;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "SmartPark";
    public string Audience { get; set; } = "SmartParkClients";
    public string Key { get; set; } = default!;
    public int ExpiresMinutes { get; set; } = 120;
}
