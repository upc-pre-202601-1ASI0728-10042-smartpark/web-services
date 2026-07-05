using SmartPark.Application.Abstractions;
using SmartPark.Application.Contracts;

namespace SmartPark.Infrastructure.DigitalTwins;

/// <summary>
/// Gemelo digital de demostración con estado en memoria, COHERENTE con la
/// estructura canónica (config/layout.json y el modelo 3D): 2 niveles × 2 zonas
/// (A, B) = 4 zonas (ZONE-L{n}-{A|B}), 8 espacios por zona (SPACE-L{n}-{X}NN).
/// Refleja ocupación y humo en vivo; el simulador IoT lo muta. Se activa con
/// <c>Adt:Mode = Demo</c> en entornos sin Azure Digital Twins accesible.
/// </summary>
public sealed class DemoDigitalTwinGateway : IDigitalTwinGateway, IOccupancySimulator
{
    private sealed class Space
    {
        public required string Id;
        public required string Code;
        public required string ZoneId;
        public required int Level;
        public required string Type;   // Regular | Disabled | EV
        public required string State;  // Free | Occupied | Reserved
        public DateTimeOffset Updated = DateTimeOffset.UtcNow;
    }

    private sealed class Zone
    {
        public required string Id;
        public required string Code;
        public required int Level;
        public required List<Space> Spaces;
        public bool SmokeActive;
        public double SmokeLevel;
        public DateTimeOffset SmokeReading = DateTimeOffset.UtcNow;
    }

    private const string LotId = "LOT-JOCKEY";
    private const int SpacesPerZone = 8; // 2 filas × 4 columnas (layout.json)
    private readonly List<Zone> _zones = new();
    private readonly object _lock = new();
    private readonly Random _rng = new();

    public DemoDigitalTwinGateway()
    {
        // Ocupación inicial por zona (de 8): varía para una demo realista.
        var seed = new[]
        {
            (Level: 1, Code: "A", Occupied: 4),
            (Level: 1, Code: "B", Occupied: 6),
            (Level: 2, Code: "A", Occupied: 7),
            (Level: 2, Code: "B", Occupied: 3),
        };
        var globalIdx = 0;
        foreach (var s in seed)
        {
            var spaces = new List<Space>(SpacesPerZone);
            for (var n = 1; n <= SpacesPerZone; n++)
            {
                globalIdx++;
                var type = globalIdx % 12 == 0 ? "Disabled" : globalIdx % 9 == 0 ? "EV" : "Regular";
                var state = n <= s.Occupied ? "Occupied" : "Free";
                var code = $"{s.Code}{n:D2}";
                spaces.Add(new Space
                {
                    Id = $"SPACE-L{s.Level}-{code}", Code = code,
                    ZoneId = $"ZONE-L{s.Level}-{s.Code}", Level = s.Level, Type = type, State = state,
                });
            }
            _zones.Add(new Zone { Id = $"ZONE-L{s.Level}-{s.Code}", Code = s.Code, Level = s.Level, Spaces = spaces });
        }
    }

    private static string Congestion(double rate) =>
        rate >= 0.9 ? "Full" : rate >= 0.75 ? "High" : rate >= 0.5 ? "Medium" : "Low";

    private static bool IsTaken(Space s) => s.State is "Occupied" or "Reserved";

    public Task<OccupancySummaryDto> GetLotOccupancyAsync(string lotId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var all = _zones.SelectMany(z => z.Spaces).ToList();
            var occ = all.Count(IsTaken);
            return Task.FromResult(new OccupancySummaryDto(
                lotId, all.Count, occ, Math.Round((double)occ / all.Count, 3), DateTimeOffset.UtcNow));
        }
    }

    public Task<IReadOnlyList<ZoneOccupancyDto>> GetZonesAsync(int? levelNumber = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var list = _zones
                .Where(z => levelNumber == null || z.Level == levelNumber)
                .Select(z =>
                {
                    var occ = z.Spaces.Count(IsTaken);
                    var rate = Math.Round((double)occ / z.Spaces.Count, 3);
                    return new ZoneOccupancyDto(z.Id, z.Code, z.Level, z.Spaces.Count, occ, rate, Congestion(rate));
                })
                .ToList();
            return Task.FromResult<IReadOnlyList<ZoneOccupancyDto>>(list);
        }
    }

    public Task<IReadOnlyList<ParkingSpaceDto>> GetSpacesByZoneAsync(string zoneId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var z = _zones.FirstOrDefault(x => string.Equals(x.Id, zoneId, StringComparison.OrdinalIgnoreCase));
            if (z is null) return Task.FromResult<IReadOnlyList<ParkingSpaceDto>>(Array.Empty<ParkingSpaceDto>());
            var spaces = z.Spaces
                .Select(s => new ParkingSpaceDto(s.Id, s.Code, s.ZoneId, s.Level, s.State, s.Type, s.Updated))
                .ToList();
            return Task.FromResult<IReadOnlyList<ParkingSpaceDto>>(spaces);
        }
    }

    public Task<IReadOnlyList<SmokeAlertDto>> GetActiveSmokeAlertsAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            var alerts = _zones
                .Where(z => z.SmokeActive)
                .Select(z => new SmokeAlertDto(
                    $"SMOKE-L{z.Level}-{z.Code}", z.Id, z.Level, true, z.SmokeLevel, "Alert", z.SmokeReading))
                .ToList();
            return Task.FromResult<IReadOnlyList<SmokeAlertDto>>(alerts);
        }
    }

    public Task UpdateSmokeStateAsync(string detectorId, double smokeLevel, DateTimeOffset at, CancellationToken ct = default)
    {
        lock (_lock)
        {
            // detectorId esperado: SMOKE-L{n}-{X}; si no coincide, lo asocia a la zona por nivel/código.
            var z = _zones.FirstOrDefault(x => detectorId.Contains($"L{x.Level}-{x.Code}", StringComparison.OrdinalIgnoreCase))
                    ?? _zones.FirstOrDefault(x => detectorId.EndsWith(x.Code, StringComparison.OrdinalIgnoreCase));
            if (z is not null)
            {
                z.SmokeActive = smokeLevel >= 200;
                z.SmokeLevel = smokeLevel;
                z.SmokeReading = at;
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult(true);

    /// <summary>Entradas/salidas de vehículos: cambia varios espacios al azar.</summary>
    public void SimulateTick()
    {
        lock (_lock)
        {
            var all = _zones.SelectMany(z => z.Spaces).ToList();
            var changes = 2 + _rng.Next(4); // 2..5 cambios por tick
            for (var i = 0; i < changes; i++)
            {
                var s = all[_rng.Next(all.Count)];
                s.State = s.State == "Free" ? "Occupied" : "Free";
                s.Updated = DateTimeOffset.UtcNow;
            }
        }
    }
}
