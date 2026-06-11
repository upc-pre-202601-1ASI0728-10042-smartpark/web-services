using Microsoft.AspNetCore.SignalR;

namespace SmartPark.Api.Hubs;

/// <summary>Hub SignalR para empujar alertas en tiempo real al dashboard del operador.</summary>
public sealed class AlertsHub : Hub { }
