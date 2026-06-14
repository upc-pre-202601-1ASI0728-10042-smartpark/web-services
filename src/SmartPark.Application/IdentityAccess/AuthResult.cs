namespace SmartPark.Application.IdentityAccess;

/// <summary>Resultado de un registro/login: el JWT y los datos básicos del usuario.</summary>
public record AuthResult(string Token, DateTimeOffset ExpiresAt, string Role, string FullName);
