using System.Text;
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
