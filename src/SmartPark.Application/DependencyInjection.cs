using Microsoft.Extensions.DependencyInjection;
using SmartPark.Application.IdentityAccess;
using SmartPark.Application.Notifications;
using SmartPark.Application.ParkingOperations;
using SmartPark.Application.SafetyIncident;
using SmartPark.Domain.ParkingSession;

namespace SmartPark.Application;

/// <summary>Registra los casos de uso (handlers) de la capa de aplicación.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<OccupancyQueryHandler>();
        services.AddScoped<GetSpacesByZoneHandler>();
        services.AddScoped<IngestSmokeAlertHandler>();
        services.AddScoped<GetActiveAlertsHandler>();
        services.AddScoped<AcknowledgeSmokeAlertHandler>();
        services.AddScoped<ResolveSmokeAlertHandler>();
        services.AddScoped<RegisterDeviceTokenHandler>();
        services.AddScoped<GetNotificationPreferencesHandler>();
        services.AddScoped<UpdateNotificationPreferencesHandler>();

        // Parking Session (Sprint 2)
        services.AddSingleton<ParkingCostCalculator>();
        services.AddScoped<StartParkingSessionHandler>();
        services.AddScoped<RegisterVehicleLocationHandler>();
        services.AddScoped<FinalizeParkingSessionHandler>();
        services.AddScoped<GetSessionHistoryHandler>();
        services.AddScoped<GetSessionSummaryHandler>();
        services.AddScoped<GetActiveSessionHandler>();

        // Energy Efficiency (Sprint 2)
        services.AddScoped<EnergyRecommendationHandler>();
        return services;
    }
}
