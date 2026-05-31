using SmartPark.Domain.IdentityAccess;

namespace SmartPark.Application.Abstractions;

public record AuthToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Puerto de emisión de JSON Web Tokens (implementado en Infraestructura).</summary>
public interface IJwtTokenService
{
    AuthToken Issue(UserAccount user);
}
