# SmartPark · Web Services (API)

API RESTful de **SmartPark (Apex Twin)** construida con **ASP.NET Core 8** bajo una
arquitectura **Domain-Driven Design** en capas (Domain · Application ·
Infrastructure · Api), organizada por *bounded contexts*.

Es el *backend* central de la solución: expone los servicios de negocio a la Web App
(Angular) y a la Mobile App (PowerApps), persiste en **PostgreSQL** vía EF Core,
integra el **Gemelo Digital (Azure Digital Twins)** mediante una *Anti-Corruption
Layer*, y emite alertas en tiempo real por **SignalR** y **Firebase Cloud Messaging**.

## Arquitectura (DDD en capas)

```
src/
├── SmartPark.Domain          # Modelo de dominio puro (sin dependencias de framework)
│   ├── Common                #   Shared Kernel: Entity, AggregateRoot, ValueObject, eventos
│   ├── IdentityAccess        #   BC: registro y autenticación de usuarios
│   ├── ParkingOperations     #   BC: ocupación y sesiones de estacionamiento
│   ├── SafetyIncident        #   BC: detección de humo e incidentes
│   └── Notifications         #   BC: device tokens para push
├── SmartPark.Application     # Casos de uso (commands/queries + handlers), puertos
├── SmartPark.Infrastructure  # EF Core, repositorios, JWT, ADT gateway (ACL), FCM
└── SmartPark.Api             # Composición, controllers, SignalR hubs, Swagger
tests/
└── SmartPark.Domain.Tests    # Pruebas unitarias del dominio (xUnit)
```

La dependencia apunta siempre **hacia adentro**: `Api → Infrastructure → Application → Domain`.
El dominio no conoce a EF Core, ASP.NET ni Azure; la infraestructura implementa los
puertos (`IUserRepository`, `IDigitalTwinGateway`, `IJwtTokenService`, …) definidos en
las capas internas.

### Patrones tácticos aplicados
- **Agregados** con factories e invariantes: `UserAccount`, `ParkingSession`, `Incident`.
- **Value Objects** con igualdad estructural: `Email`, `Money`, `SmokeReading`, `VehicleLocation`.
- **Eventos de dominio**: `UserRegistered`, `ParkingSessionStarted`, `SmokeAlertRaised`, `IncidentResolved`.
- **Repositorios** como puertos + **Unit of Work** sobre `DbContext`.
- **CQRS-lite**: handlers de comando/consulta sin framework de *mediator*.
- **Anti-Corruption Layer**: `AzureDigitalTwinsGateway` traduce el modelo ADT al dominio.

## Endpoints

| Método | Ruta                                   | Rol         | Descripción                                  |
|--------|----------------------------------------|-------------|----------------------------------------------|
| POST   | `/api/v1/auth/register`                | Público     | Registra un usuario (Operator/Driver).       |
| POST   | `/api/v1/auth/login`                   | Público     | Autentica y devuelve un JWT.                 |
| GET    | `/api/v1/occupancy/summary`            | Operator    | Resumen de ocupación del lote.               |
| GET    | `/api/v1/occupancy/zones`              | Operator    | Lista de zonas con ocupación.                |
| GET    | `/api/v1/occupancy/zones/{zoneId}/spaces` | Operator | Espacios de una zona.                        |
| GET    | `/api/v1/alerts/smoke`                 | Operator    | Alertas de humo activas.                     |
| POST   | `/api/v1/alerts/smoke`                 | Público*    | Ingesta de lectura de humo (IoT/simulador).  |
| POST   | `/api/v1/notifications/tokens`         | Driver      | Registra un device token para push.          |

\* La ingesta es anónima porque la consume el simulador IoT / Function; se protege por red.

**Tiempo real:** SignalR Hub en `/hubs/alerts` (evento `smokeAlert`).

## Ejecución local

```bash
dotnet restore SmartPark.sln
dotnet build SmartPark.sln
dotnet test SmartPark.sln
dotnet run --project src/SmartPark.Api      # Swagger en https://localhost:****/swagger
```

Configuración por `appsettings.json` / variables de entorno: cadena de conexión
PostgreSQL, sección `Jwt` (Issuer/Audience/Key), `AzureDigitalTwins:Host` y `Fcm`.
Los secretos van en `.env` / *user-secrets* (nunca versionados).

## Ramas
- `main` — releases estables (desencadena el deploy a Azure App Service).
- `develop` — rama de integración (los PRs apuntan aquí).
- `feature/*` — trabajo en curso por funcionalidad.

## CI/CD
- **CI** (`.github/workflows/ci.yml`): restore → build → test en cada push/PR a `develop` y `main`.
- **Deploy** (`.github/workflows/azure-deploy.yml`): publica y despliega a Azure App Service al integrar en `main`.
