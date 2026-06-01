using SmartPark.Application.Abstractions;
using SmartPark.Domain.IdentityAccess;

namespace SmartPark.Application.IdentityAccess;

public record LoginQuery(string Email, string Password);

/// <summary>Caso de uso: inicio de sesión. Devuelve null si las credenciales son inválidas (TS-09).</summary>
public sealed class LoginHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService jwt)
{
    public async Task<AuthResult?> HandleAsync(LoginQuery query, CancellationToken ct = default)
    {
        var email = Email.Create(query.Email);
        var user = await users.GetByEmailAsync(email, ct);
        if (user is null || !hasher.Verify(query.Password, user.PasswordHash))
            return null;

        var token = jwt.Issue(user);
        return new AuthResult(token.Token, token.ExpiresAt, user.Role.ToString(), user.FullName);
    }
}
