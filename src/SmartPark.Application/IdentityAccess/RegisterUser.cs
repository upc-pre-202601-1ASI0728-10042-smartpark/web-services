using SmartPark.Application.Abstractions;
using SmartPark.Domain.Common;
using SmartPark.Domain.IdentityAccess;

namespace SmartPark.Application.IdentityAccess;

public record RegisterUserCommand(string Email, string Password, string FullName, string Role);

/// <summary>Caso de uso: registro de un operador o conductor (TS-09).</summary>
public sealed class RegisterUserHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService jwt, IUnitOfWork uow)
{
    public async Task<AuthResult> HandleAsync(RegisterUserCommand cmd, CancellationToken ct = default)
    {
        var email = Email.Create(cmd.Email);
        if (await users.ExistsByEmailAsync(email, ct))
            throw new DomainException("El correo ya está registrado.");

        if (!Enum.TryParse<UserRole>(cmd.Role, ignoreCase: true, out var role))
            throw new DomainException($"Rol inválido: '{cmd.Role}'.");

        var user = UserAccount.Register(email, hasher.Hash(cmd.Password), cmd.FullName, role);
        users.Add(user);
        await uow.SaveChangesAsync(ct);

        var token = jwt.Issue(user);
        return new AuthResult(token.Token, token.ExpiresAt, user.Role.ToString(), user.FullName);
    }
}
