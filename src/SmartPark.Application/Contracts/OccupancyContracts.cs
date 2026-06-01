namespace SmartPark.Application.Contracts;

public record OccupancySummaryDto(string LotId, int TotalSpaces, int OccupiedSpaces, double OccupancyRate, DateTimeOffset AsOf);

public record ZoneOccupancyDto(string ZoneId, string Code, int LevelNumber, int TotalSpaces, int OccupiedSpaces, double OccupancyRate, string CongestionLevel);

public record ParkingSpaceDto(string SpaceId, string Code, string ZoneId, int LevelNumber, string OccupancyState, string SpaceType, DateTimeOffset LastUpdated);
