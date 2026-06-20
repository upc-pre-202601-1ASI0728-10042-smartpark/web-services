using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartPark.Api.Hubs;
using SmartPark.Application;
using SmartPark.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Capas de la aplicacion (DDD)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Web: MVC + SignalR + OpenAPI
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartPark API", Version = "v1", Description = "API RESTful de SmartPark - Apex Twin." }));

// Autenticacion JWT
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? "")),
    });
builder.Services.AddAuthorization();

// CORS para la Web App (Angular) y la Mobile App (PowerApps)
builder.Services.AddCors(o => o.AddPolicy("smartpark", p => p
    .WithOrigins(builder.Configuration["Cors:WebApp"] ?? "http://localhost:4200", "https://make.powerapps.com")
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

// Dev/local: crea el esquema en la base de datos si aún no existe.
// Resiliente: si la BD no está disponible al iniciar, la app igual levanta
// (modo degradado) y el error queda registrado, en vez de tumbar el contenedor.
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartPark.Infrastructure.Persistence.SmartParkDbContext>();
        db.Database.EnsureCreated();
        app.Logger.LogInformation("Esquema de base de datos verificado/creado.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "No se pudo inicializar la base de datos al arrancar.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("smartpark");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AlertsHub>("/hubs/alerts");

app.Run();
