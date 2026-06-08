using System.Text.Json;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;

namespace SmartPark.Infrastructure.DigitalTwins;

/// <summary>
/// Anti-Corruption Layer sobre Azure.DigitalTwins.Core: traduce el grafo de twins
/// (DTDL) a los DTOs neutrales de la capa de aplicación. Autenticación con
/// DefaultAzureCredential (Managed Identity en la nube / az login en local).
/// </summary>
public sealed class AzureDigitalTwinsGateway : IDigitalTwinGateway
{
    private const string M = "dtmi:com:apextwin:smartpark";
    private readonly DigitalTwinsClient _client;
    private readonly ILogger<AzureDigitalTwinsGateway> _log;

    public AzureDigitalTwinsGateway(IOptions<AdtOptions> options, ILogger<AzureDigitalTwinsGateway> log)
    {
        _log = log;
        var host = options.Value.HostName;
        var uri = new Uri(host.StartsWith("http") ? host : $"https://{host}");
        _client = new DigitalTwinsClient(uri, new DefaultAzureCredential());
    }

    public async Task<OccupancySummaryDto> GetLotOccupancyAsync(string lotId, CancellationToken ct = default)
    {
        var twin = await _client.GetDigitalTwinAsync<BasicDigitalTwin>(lotId, ct);
        var c = twin.Value.Contents;
        return new OccupancySummaryDto(lotId, GetInt(c, "totalSpaces"), GetInt(c, "occupiedSpaces"), GetDouble(c, "occupancyRate"), DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<ZoneOccupancyDto>> GetZonesAsync(int? levelNumber = null, CancellationToken ct = default)
    {
        var q = $"SELECT * FROM digitaltwins T WHERE IS_OF_MODEL(T, '{M}:ParkingZone;1')";
        var result = new List<ZoneOccupancyDto>();
        await foreach (var t in _client.QueryAsync<BasicDigitalTwin>(q, ct))
        {
            var lvl = ParseLevel(t.Id);
            if (levelNumber.HasValue && lvl != levelNumber.Value) continue;
            var c = t.Contents;
            result.Add(new ZoneOccupancyDto(t.Id, GetString(c, "code"), lvl, GetInt(c, "totalSpaces"), GetInt(c, "occupiedSpaces"), GetDouble(c, "occupancyRate"), GetString(c, "congestionLevel", "Low")));
        }
        return result;
    }

    public async Task<IReadOnlyList<ParkingSpaceDto>> GetSpacesByZoneAsync(string zoneId, CancellationToken ct = default)
    {
        var q = $"SELECT space FROM digitaltwins zone JOIN space RELATED zone.hasSpace WHERE zone.$dtId = '{zoneId}'";
        var result = new List<ParkingSpaceDto>();
        await foreach (var item in _client.QueryAsync<JsonElement>(q, ct))
        {
            var s = item.GetProperty("space");
            var id = s.GetProperty("$dtId").GetString()!;
            result.Add(new ParkingSpaceDto(id, Str(s, "code"), zoneId, ParseLevel(id), Str(s, "occupancyState", "Free"), Str(s, "spaceType", "Regular"), Date(s, "lastUpdated")));
        }
        return result;
    }

    public async Task<IReadOnlyList<SmokeAlertDto>> GetActiveSmokeAlertsAsync(CancellationToken ct = default)
    {
        var q = $"SELECT * FROM digitaltwins T WHERE IS_OF_MODEL(T, '{M}:SmokeDetector;1') AND T.smokeDetected = true";
        var result = new List<SmokeAlertDto>();
        await foreach (var t in _client.QueryAsync<BasicDigitalTwin>(q, ct))
        {
            var c = t.Contents;
            result.Add(new SmokeAlertDto(t.Id, GetString(c, "code"), ParseLevel(t.Id), GetBool(c, "smokeDetected"), GetDouble(c, "smokeLevel"), GetString(c, "status", "Normal"), DateTimeOffset.UtcNow));
        }
        return result;
    }

    public async Task<IReadOnlyList<EnergyZoneDto>> GetEnergyRecommendationsAsync(CancellationToken ct = default)
    {
        var q = $"SELECT * FROM digitaltwins T WHERE IS_OF_MODEL(T, '{M}:LightingZone;1')";
        var result = new List<EnergyZoneDto>();
        await foreach (var t in _client.QueryAsync<BasicDigitalTwin>(q, ct))
        {
            var c = t.Contents;
            result.Add(new EnergyZoneDto(t.Id, GetDouble(c, "currentLevel"), GetDouble(c, "recommendedLevel"), GetDouble(c, "savingsPercent"), GetString(c, "status", "Optimal")));
        }
        return result;
    }

    public Task UpdateSmokeStateAsync(string detectorId, double smokeLevel, DateTimeOffset at, CancellationToken ct = default)
        => Task.CompletedTask; // JSON Patch — implementado en el siguiente commit

    public Task<bool> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult(true);

    // ---- helpers ----
    private static int ParseLevel(string id)
    {
        var i = id.IndexOf("-L", StringComparison.Ordinal);
        if (i < 0) return 0;
        var num = new string(id[(i + 2)..].TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(num, out var n) ? n : 0;
    }
    private static int GetInt(IDictionary<string, object> c, string k) => c.TryGetValue(k, out var v) && v != null ? Convert.ToInt32(((JsonElement)v).ToString()) : 0;
    private static double GetDouble(IDictionary<string, object> c, string k) => c.TryGetValue(k, out var v) && v != null ? Convert.ToDouble(((JsonElement)v).ToString(), System.Globalization.CultureInfo.InvariantCulture) : 0;
    private static bool GetBool(IDictionary<string, object> c, string k) => c.TryGetValue(k, out var v) && v != null && ((JsonElement)v).GetBoolean();
    private static string GetString(IDictionary<string, object> c, string k, string def = "") => c.TryGetValue(k, out var v) && v != null ? ((JsonElement)v).ToString() : def;
    private static string Str(JsonElement e, string k, string def = "") => e.TryGetProperty(k, out var v) ? v.ToString() : def;
    private static DateTimeOffset Date(JsonElement e, string k) => e.TryGetProperty(k, out var v) && DateTimeOffset.TryParse(v.ToString(), out var d) ? d : DateTimeOffset.UtcNow;
}
