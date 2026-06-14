using Microsoft.Extensions.DependencyInjection;
using SmartPark.Application.IdentityAccess;
using SmartPark.Application.Notifications;
using SmartPark.Application.ParkingOperations;
using SmartPark.Application.SafetyIncident;

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
        services.AddScoped<RegisterDeviceTokenHandler>();
        return services;
    }
}
