using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.Application.Abstractions;
using SmartPark.Domain.IdentityAccess;
using SmartPark.Domain.Notifications;
using SmartPark.Domain.ParkingSession;
using SmartPark.Domain.SafetyIncident;
using SmartPark.Infrastructure.DigitalTwins;
using SmartPark.Infrastructure.Identity;
using SmartPark.Infrastructure.Notifications;
using SmartPark.Infrastructure.Persistence;
using SmartPark.Infrastructure.Persistence.Repositories;

namespace SmartPark.Infrastructure;

/// <summary>Composición de la capa de infraestructura: persistencia, ACL del gemelo, identidad y notificaciones.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Persistencia (PostgreSQL) + Unit of Work
        services.AddDbContext<SmartParkDbContext>(opt => opt.UseNpgsql(config.GetConnectionString("SmartParkDb")));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SmartParkDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();

        // Digital Twin Synchronization (Anti-Corruption Layer)
        services.Configure<AdtOptions>(config.GetSection("Adt"));
        services.AddSingleton<IDigitalTwinGateway, AzureDigitalTwinsGateway>();

        // Identity (JWT + hashing)
        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        // Notifications (FCM)
        services.Configure<FcmOptions>(config.GetSection("Fcm"));
        services.AddHttpClient();
        services.AddScoped<INotificationService, FcmNotificationService>();

        return services;
    }
}
